using BLL.Managers;
using BLL.Settings;
using DAL.Models;
using DAL.Models.Enums;
using DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Fayed_API.Services
{
    public class OrderCompletionBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OrderCompletionBackgroundService> _logger;

        public OrderCompletionBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<OrderCompletionBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OrderCompletionBackgroundService started.");

            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var notification = scope.ServiceProvider.GetRequiredService<INotificationService>();
                    var settings = scope.ServiceProvider.GetRequiredService<IOptions<ContractSettings>>();

                    await AutoCompleteOverdueOrdersAsync(uow, notification, stoppingToken);
                    await AutoConfirmDeliveriesAsync(uow, notification, settings.Value, stoppingToken);
                    await ReleaseEscrowAsync(uow, notification, settings.Value, stoppingToken);

                    await uow.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in background service.");
                }
            }
        }

        private async Task AutoCompleteOverdueOrdersAsync(IUnitOfWork uow, INotificationService notification, CancellationToken ct)
        {
            var overdueOrders = await uow.Repository<Order>().Query()
                .Include(o => o.Listing)
                .Where(o => o.Status == OrderStatus.InProgress
                         && !o.IsDisputed
                         && !o.IsDownPaymentPaid
                         && o.DeliveryDate != null
                         && o.DeliveryDate.Value.Date < DateTime.UtcNow.Date)
                .ToListAsync(ct);

            if (overdueOrders.Count == 0)
                return;

            _logger.LogInformation("Auto-completing {Count} overdue orders.", overdueOrders.Count);

            foreach (var order in overdueOrders)
            {
                order.Status = OrderStatus.Completed;
                uow.Repository<Order>().Update(order);

                var listingTitle = order.Listing?.Title ?? string.Empty;

                await notification.SendNotificationAsync(order.BuyerId,
                    "تم اكتمال الطلب تلقائياً",
                    $"تم اكتمال الطلب رقم {order.Id} على {listingTitle} لتجاوز تاريخ التسليم.",
                    "order_completed");

                await notification.SendNotificationAsync(order.SellerId,
                    "تم اكتمال الطلب تلقائياً",
                    $"تم اكتمال الطلب رقم {order.Id} على {listingTitle} لتجاوز تاريخ التسليم.",
                    "order_completed");
            }
        }

        private async Task AutoConfirmDeliveriesAsync(IUnitOfWork uow, INotificationService notification, ContractSettings settings, CancellationToken ct)
        {
            var pendingConfirmations = await uow.Repository<Order>().Query()
                .Include(o => o.Listing)
                .Where(o => o.Status == OrderStatus.InProgress
                         && !o.IsDisputed
                         && o.IsDownPaymentPaid
                         && o.DeliveryDate != null
                         && o.DeliveryDate.Value <= DateTime.UtcNow
                         && o.EscrowReleaseAt == null)
                .ToListAsync(ct);

            if (pendingConfirmations.Count == 0)
                return;

            _logger.LogInformation("Auto-confirming delivery for {Count} orders.", pendingConfirmations.Count);

            foreach (var order in pendingConfirmations)
            {
                order.EscrowReleaseAt = DateTime.UtcNow.AddDays(settings.EscrowDurationDays);
                uow.Repository<Order>().Update(order);

                var listingTitle = order.Listing?.Title ?? string.Empty;

                await notification.SendNotificationAsync(order.BuyerId,
                    "تم تأكيد استلام الشحنة تلقائياً",
                    $"تم تأكيد استلام {listingTitle} تلقائياً بعد تجاوز تاريخ التسليم. سيتم تحرير العربون بعد {settings.EscrowDurationDays} يوم.",
                    "delivery_confirmed",
                    $"/orders/{order.Id}");

                await notification.SendNotificationAsync(order.SellerId,
                    "تم تأكيد استلام الشحنة تلقائياً",
                    $"قام النظام بتأكيد استلام {listingTitle} بعد تجاوز تاريخ التسليم. سيتم تحرير العربون بعد {settings.EscrowDurationDays} يوم.",
                    "delivery_confirmed",
                    $"/orders/{order.Id}");
            }
        }

        private async Task ReleaseEscrowAsync(IUnitOfWork uow, INotificationService notification, ContractSettings settings, CancellationToken ct)
        {
            var escrowOrders = await uow.Repository<Order>().Query()
                .Include(o => o.Listing)
                .Where(o => o.Status == OrderStatus.InProgress
                         && !o.IsDisputed
                         && o.IsDownPaymentPaid
                         && o.EscrowReleaseAt != null
                         && o.EscrowReleaseAt.Value <= DateTime.UtcNow)
                .ToListAsync(ct);

            if (escrowOrders.Count == 0)
                return;

            _logger.LogInformation("Releasing escrow for {Count} orders.", escrowOrders.Count);

            foreach (var order in escrowOrders)
            {
                order.Status = OrderStatus.Completed;

                // Create ledger transactions for escrow release
                var buyerWallet = await uow.Repository<Wallet>().Query()
                    .FirstOrDefaultAsync(w => w.UserId == order.BuyerId, ct);

                var sellerWallet = await uow.Repository<Wallet>().Query()
                    .FirstOrDefaultAsync(w => w.UserId == order.SellerId, ct);

                if (buyerWallet != null)
                {
                    // Release frozen amount (Unfreeze)
                    var unfreezeTx = new Transaction
                    {
                        OrderId = order.Id,
                        WalletId = buyerWallet.Id,
                        Amount = order.DownPaymentAmount,
                        TransactionType = TransactionType.Unfreeze,
                        Status = TransactionStatus.Completed,
                        CreatedAt = DateTime.UtcNow
                    };
                    await uow.Repository<Transaction>().AddAsync(unfreezeTx);
                }

                if (sellerWallet != null)
                {
                    // Deposit seller payout
                    var depositTx = new Transaction
                    {
                        OrderId = order.Id,
                        WalletId = sellerWallet.Id,
                        Amount = order.SellerTotalPayout,
                        TransactionType = TransactionType.Deposit,
                        Status = TransactionStatus.Completed,
                        CreatedAt = DateTime.UtcNow
                    };
                    await uow.Repository<Transaction>().AddAsync(depositTx);

                    // Record commission
                    var commissionTx = new Transaction
                    {
                        OrderId = order.Id,
                        WalletId = sellerWallet.Id,
                        Amount = order.PlatformCommission,
                        TransactionType = TransactionType.Commission,
                        Status = TransactionStatus.Completed,
                        CreatedAt = DateTime.UtcNow
                    };
                    await uow.Repository<Transaction>().AddAsync(commissionTx);
                }

                uow.Repository<Order>().Update(order);

                var listingTitle = order.Listing?.Title ?? string.Empty;

                await notification.SendNotificationAsync(order.SellerId,
                    "تم تحرير العربون",
                    $"تم تحرير العربون للطلب رقم {order.Id} على {listingTitle}. المبلغ المحول: {order.SellerTotalPayout} ج.م (بعد خصم عمولة المنصة {order.PlatformCommission} ج.م).",
                    "escrow_released",
                    $"/orders/{order.Id}");

                await notification.SendNotificationAsync(order.BuyerId,
                    "تم تحرير العربون",
                    $"تم تحرير العربون للطلب رقم {order.Id} على {listingTitle}. الصفقة مكتملة.",
                    "order_completed",
                    $"/orders/{order.Id}");

                await notification.SendNotificationAsync(order.SellerId,
                    "خصم عمولة المنصة",
                    $"تم خصم عمولة المنصة بنسبة {settings.CommissionRate * 100}% بقيمة {order.PlatformCommission} ج.م من الطلب رقم {order.Id}.",
                    "commission_deducted",
                    $"/orders/{order.Id}");
            }
        }
    }
}

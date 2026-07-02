using BLL.DTOs.Common;
using BLL.DTOs.Listings;
using BLL.Managers.CloudinaryManager; // استدعاء السيرفس
using BLL.Mapping.Listings;
using DAL.Models;
using DAL.Models.Enums;
using DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace BLL.Managers
{
    public class ListingManager : IListingManager
    {
        private readonly IUnitOfWork _uow;
        private readonly ICloudinaryService _cloudinaryService; //  حقن السيرفس

        public ListingManager(IUnitOfWork uow, ICloudinaryService cloudinaryService)
        {
            _uow = uow;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<BaseResponse<PagedResult<ListingDto>>> GetPublishedAsync(PublishedListingsFilterDto filter)
        {
            var page = filter.Page < 1 ? 1 : filter.Page;
            var pageSize = filter.PageSize is < 1 or > 100 ? 12 : filter.PageSize;

            var query = _uow.Repository<Listing>().Query()
                .AsNoTracking()
                .Include(l => l.Factory)
                .Where(l => l.Status == ListingStatus.Published);

            if (!string.IsNullOrWhiteSpace(filter.Location))
                query = query.Where(l => l.Factory.Address.Contains(filter.Location));

            if (filter.CategoryId is not null)
                query = query.Where(l => l.CategoryId == filter.CategoryId);

            if (!string.IsNullOrWhiteSpace(filter.CustomCategory))
                query = query.Where(l => l.CustomCatName != null && l.CustomCatName.Contains(filter.CustomCategory));

            if (filter.MinQuantity is not null)
                query = query.Where(l => l.Quantity >= filter.MinQuantity);

            if (filter.MaxQuantity is not null)
                query = query.Where(l => l.Quantity <= filter.MaxQuantity);

            if (filter.MinPrice is not null)
                query = query.Where(l => l.MaxPrice >= filter.MinPrice);

            if (filter.MaxPrice is not null)
                query = query.Where(l => l.MinPrice <= filter.MaxPrice);

            var total = await query.CountAsync();

            query = filter.SortDirection?.ToLower() == "asc"
                ? query.OrderBy(l => l.PublishedAt ?? l.CreatedAt)
                : query.OrderByDescending(l => l.PublishedAt ?? l.CreatedAt);

            var entities = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new PagedResult<ListingDto>
            {
                Items = entities.Select(l => l.ToDto()).ToList(),
                Page = page,
                PageSize = pageSize,
                TotalCount = total
            };

            return BaseResponse<PagedResult<ListingDto>>.Success(result, "تم جلب المنتجات المنشورة بنجاح.");
        }

        public async Task<BaseResponse<ListingDetailsDto>> GetByIdAsync(int id)
        {
            var entity = await _uow.Repository<Listing>().Query()
                .AsNoTracking()
                .Include(l => l.Media)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (entity is null)
                return BaseResponse<ListingDetailsDto>.Failure("هذا المنتج غير موجود.");

            return BaseResponse<ListingDetailsDto>.Success(entity.ToDetailsDto(), "تم جلب تفاصيل المنتج بنجاح.");
        }

        public async Task<BaseResponse<ListingDetailsDto>> CreateAsync(CreateListingDto dto)
        {
            var entity = dto.ToEntity();
            entity.Status = ListingStatus.Draft;
            entity.CreatedAt = DateTime.UtcNow;

            // 2. معالجة رفع الملفات عبر Cloudinary 
            // رفع الفيديو لو موجود
            if (dto.Video != null)
            {
                entity.VideoUrl = await _cloudinaryService.UploadFileAsync(dto.Video, "listings/videos");
            }

            // رفع شهادة التحليل كـ Raw File لو موجودة
            if (dto.Certificate != null)
            {
                entity.CertificateUrl = await _cloudinaryService.UploadFileAsync(dto.Certificate, "listings/certificates");
            }

            // رفع باقة صور المنتج الميديا وتسكينها في جدول الـ ListingMedia
            if (dto.Images != null && dto.Images.Any())
            {
                bool isFirst = true;
                foreach (var imgFile in dto.Images)
                {
                    var imgUrl = await _cloudinaryService.UploadFileAsync(imgFile, "listings/images");
                    entity.Media.Add(new ListingMedia
                    {
                        MediaUrl = imgUrl,
                        MediaType = MediaType.Image,
                        IsMain = isFirst // أول صورة يتم رفعها تُعتبر الصورة الأساسية أوتوماتيكياً
                    });
                    isFirst = false;
                }
            }

            await _uow.Repository<Listing>().AddAsync(entity);
            await _uow.SaveChangesAsync();
            return BaseResponse<ListingDetailsDto>.Success(entity.ToDetailsDto(), "تم حفظ المنتج كمسودة بنجاح مع رفع الميديا المرفقة.");
        }


        public async Task<BaseResponse<bool>> UpdateAsync(int id, UpdateListingDto dto)
        {
            var repo = _uow.Repository<Listing>();
            // جلب المنتج مع الميديا الخاصة به للتمكن من معالجة الإضافات الجديدة
            var entity = await repo.Query().Include(l => l.Media).FirstOrDefaultAsync(l => l.Id == id);
            if (entity is null)
                return BaseResponse<bool>.Failure("عذراً، المنتج غير موجود لتعديله.");

            dto.Apply(entity);

            //  رفع الملفات الجديدة أثناء التعديل إن وجدت ومسح الملفات القديمة المعوضة
            if (dto.NewVideo != null)
            {
                if (!string.IsNullOrEmpty(entity.VideoUrl))
                    await _cloudinaryService.DeleteFileAsync(entity.VideoUrl, "video");

                entity.VideoUrl = await _cloudinaryService.UploadFileAsync(dto.NewVideo, "listings/videos");
            }

            if (dto.NewCertificate != null)
            {
                if (!string.IsNullOrEmpty(entity.CertificateUrl))
                    await _cloudinaryService.DeleteFileAsync(entity.CertificateUrl, "raw");

                entity.CertificateUrl = await _cloudinaryService.UploadFileAsync(dto.NewCertificate, "listings/certificates");
            }

            if (dto.NewImages != null && dto.NewImages.Any())
            {
                foreach (var imgFile in dto.NewImages)
                {
                    var imgUrl = await _cloudinaryService.UploadFileAsync(imgFile, "listings/images");
                    entity.Media.Add(new ListingMedia
                    {
                        MediaUrl = imgUrl,
                        MediaType = MediaType.Image,
                        IsMain = !entity.Media.Any(m => m.IsMain) // لو مفيش صورة رئيسية سابقة يعتبرها رئيسية
                    });
                }
            }

            repo.Update(entity);
            await _uow.SaveChangesAsync();
            return BaseResponse<bool>.Success(true, "تم تحديث بيانات المنتج والميديا بنجاح.");
        }

        public async Task<BaseResponse<bool>> PublishAsync(int id)
        {
            var repo = _uow.Repository<Listing>();
            var entity = await repo.GetByIdAsync(id);
            if (entity is null)
                return BaseResponse<bool>.Failure("عذراً، المنتج غير موجود لنشره.");

            entity.Status = ListingStatus.Published;
            entity.PublishedAt = DateTime.UtcNow;
            repo.Update(entity);
            await _uow.SaveChangesAsync();
            return BaseResponse<bool>.Success(true, "تم نشر المنتج على المنصة بنجاح.");
        }

        public async Task<BaseResponse<bool>> DeleteAsync(int id)
        {
            var repo = _uow.Repository<Listing>();
            var entity = await repo.Query().Include(l => l.Media).FirstOrDefaultAsync(l => l.Id == id);
            if (entity is null)
                return BaseResponse<bool>.Failure("عذراً، المنتج غير موجود لحذفه.");

            //  اللقطة الذكية: في حال الحذف، نقوم بمسح الميديا المرتبطة من سيرفر Cloudinary أيضاً لمنع تراكم الملفات المهملة!
            if (!string.IsNullOrEmpty(entity.VideoUrl))
                await _cloudinaryService.DeleteFileAsync(entity.VideoUrl, "video");

            if (!string.IsNullOrEmpty(entity.CertificateUrl))
                await _cloudinaryService.DeleteFileAsync(entity.CertificateUrl, "raw");

            if (entity.Media != null && entity.Media.Any())
            {
                foreach (var media in entity.Media)
                {
                    await _cloudinaryService.DeleteFileAsync(media.MediaUrl, "image");
                }
            }

            repo.Remove(entity); // Soft delete مدمج بالـ DbContext
            await _uow.SaveChangesAsync();
            return BaseResponse<bool>.Success(true, "تم حذف المنتج وكافة وسائطه التابعة بنجاح.");
        }

        public async Task<BaseResponse<List<ListingDto>>> GetByUserFactoryAsync(int userId)
        {
            var entities = await _uow.Repository<Listing>().Query()
                .AsNoTracking()
                .Where(l => l.Factory.UserId == userId)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            var dtos = entities.Select(l => l.ToDto()).ToList();
            return BaseResponse<List<ListingDto>>.Success(dtos, "تم جلب منتجات المصنع بنجاح.");
        }
    }
}
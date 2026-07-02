using DAL.Models;
using DAL.Models.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DAL.Data
{
    /// <summary>
    /// EF Core context for the Fayed B2B marketplace. All entity mappings live in
    /// <c>DAL.Data.Configurations</c> and are applied automatically from the assembly.
    /// </summary>
    public class FayedDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public FayedDbContext(DbContextOptions<FayedDbContext> options) : base(options)
        {
        }

        
        // Geography
        public DbSet<Governorate> Governorates => Set<Governorate>();
        public DbSet<City> Cities => Set<City>();

        // Factory & Verification
        public DbSet<Factory> Factories => Set<Factory>();
        public DbSet<Document> Documents => Set<Document>();
        public DbSet<VerificationCase> VerificationCases => Set<VerificationCase>();

        // AI Layer
        public DbSet<AIVerificationResult> AIVerificationResults => Set<AIVerificationResult>();
        public DbSet<AISearchLog> AISearchLogs => Set<AISearchLog>();

        // Catalog
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Listing> Listings => Set<Listing>();
        public DbSet<ListingMedia> ListingMedia => Set<ListingMedia>();
        public DbSet<SavedListing> SavedListings => Set<SavedListing>();

        // Communication
        public DbSet<Chat> Chats => Set<Chat>();
        public DbSet<Message> Messages => Set<Message>();

        // Orders & Finance
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<Wallet> Wallets => Set<Wallet>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<Review> Reviews => Set<Review>();

        // System
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        public DbSet<Dispute> Disputes => Set<Dispute>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // سطر أساسي وجوهري جداً عشان جداول الأيدنتيتي تترسم وتبني علاقاتها الداخلية صح قبل أي كود تاني
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(FayedDbContext).Assembly);
        }

        public override int SaveChanges()
        {
            ApplySoftDelete();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplySoftDelete();
            return base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>Translates hard deletes of <see cref="ISoftDeletable"/> entities into soft deletes.</summary>
        private void ApplySoftDelete()
        {
            foreach (var entry in ChangeTracker.Entries<ISoftDeletable>())
            {
                if (entry.State != EntityState.Deleted)
                    continue;

                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = DateTime.UtcNow;
            }
        }
    }
}
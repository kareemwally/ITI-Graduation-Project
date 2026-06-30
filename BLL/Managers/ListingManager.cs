using BLL.DTOs.Common;
using BLL.DTOs.Listings;
using BLL.Mapping;
using DAL.Models;
using DAL.Models.Enums;
using DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace BLL.Managers
{
    public class ListingManager : IListingManager
    {
        private readonly IUnitOfWork _uow;

        public ListingManager(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<PagedResult<ListingDto>> GetPublishedAsync(int page, int pageSize, string? materialType = null)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

            var query = _uow.Repository<Listing>().Query()
                .AsNoTracking()
                .Where(l => l.Status == ListingStatus.Published);

            if (!string.IsNullOrWhiteSpace(materialType))
                query = query.Where(l => l.MaterialType == materialType);

            var total = await query.CountAsync();

            var entities = await query
                .OrderByDescending(l => l.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<ListingDto>
            {
                Items = entities.Select(l => l.ToDto()).ToList(),
                Page = page,
                PageSize = pageSize,
                TotalCount = total
            };
        }

        public async Task<ListingDetailsDto?> GetByIdAsync(int id)
        {
            var entity = await _uow.Repository<Listing>().Query()
                .AsNoTracking()
                .Include(l => l.Media)
                .FirstOrDefaultAsync(l => l.Id == id);

            return entity?.ToDetailsDto();
        }

        public async Task<ListingDetailsDto> CreateAsync(CreateListingDto dto)
        {
            var entity = dto.ToEntity();
            entity.Status = ListingStatus.Draft;
            entity.CreatedAt = DateTime.UtcNow;

            await _uow.Repository<Listing>().AddAsync(entity);
            await _uow.SaveChangesAsync();
            return entity.ToDetailsDto();
        }

        public async Task<bool> UpdateAsync(int id, UpdateListingDto dto)
        {
            var repo = _uow.Repository<Listing>();
            var entity = await repo.GetByIdAsync(id);
            if (entity is null)
                return false;

            dto.Apply(entity);
            repo.Update(entity);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> PublishAsync(int id)
        {
            var repo = _uow.Repository<Listing>();
            var entity = await repo.GetByIdAsync(id);
            if (entity is null)
                return false;

            entity.Status = ListingStatus.Published;
            repo.Update(entity);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var repo = _uow.Repository<Listing>();
            var entity = await repo.GetByIdAsync(id);
            if (entity is null)
                return false;

            repo.Remove(entity); // soft delete handled by the DbContext
            await _uow.SaveChangesAsync();
            return true;
        }


        async Task<PagedResult<ListingDto>> IListingManager.SearchListingsAsync(ListingSearchParametersDto searchParams)
        {
            var query = _uow.Repository<Listing>().Query().AsNoTracking().Where(l => !l.IsDeleted && l.Status!=ListingStatus.Draft);

            if (searchParams.CategoryId.HasValue)
                query = query.Where(l => l.CategoryId == searchParams.CategoryId.Value);

            if (!string.IsNullOrWhiteSpace(searchParams.Location))
                query = query.Where(l => l.Factory.Address.Contains(searchParams.Location));
            if (searchParams.MinQuantity.HasValue)
                query = query.Where(l => l.Quantity >= searchParams.MinQuantity.Value);

            if (searchParams.MaxQuantity.HasValue)
                query = query.Where(l => l.Quantity <= searchParams.MaxQuantity.Value);

            if (searchParams.MinPrice.HasValue)
                query = query.Where(l => l.Price >= searchParams.MinPrice.Value);

            if (searchParams.MaxPrice.HasValue)
                query = query.Where(l => l.Price <= searchParams.MaxPrice.Value);

            // 3. الترتيب (Sorting)
            query = searchParams.SortBy?.ToLower() switch
            {
                "price_asc" => query.OrderBy(l => l.Price),
                "price_desc" => query.OrderByDescending(l => l.Price),
                "oldest" => query.OrderBy(l => l.CreatedAt),
                _ => query.OrderByDescending(l => l.CreatedAt) // الافتراضي: الأحدث أولاً
            };

            // 4. الباجنيشن (Pagination)
            var totalCount = await query.CountAsync(); // بنعد هما كام نتيجة في الداتا بيز

            var entities = await query
                .Skip((searchParams.PageNumber - 1) * searchParams.PageSize)
                .Take(searchParams.PageSize)
                .ToListAsync(); // التنفيذ الفعلي في الداتا بيز بيحصل هنا

            // 5. بنحول الـ Entity لـ DTO ونرجعه للفرونت
            return new PagedResult<ListingDto>
            {
                Items = entities.Select(l => l.ToDto()).ToList(),
                Page = searchParams.PageNumber,
                PageSize = searchParams.PageSize,
                TotalCount = totalCount
            };






        }
    }
}

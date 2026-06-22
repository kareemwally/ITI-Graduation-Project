using BLL.DTOs.Categories;
using BLL.Mapping;
using DAL.Models;
using DAL.UnitOfWork;

namespace BLL.Managers
{
    /// <summary>
    /// Depends only on <see cref="IUnitOfWork"/> (the DAL abstraction), never on EF Core directly,
    /// so it stays testable and storage-agnostic (DIP).
    /// </summary>
    public class CategoryManager : ICategoryManager
    {
        private readonly IUnitOfWork _uow;

        public CategoryManager(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IReadOnlyList<CategoryDto>> GetAllAsync()
        {
            var categories = await _uow.Repository<Category>().GetAllAsync();
            return categories.Select(c => c.ToDto()).ToList();
        }

        public async Task<IReadOnlyList<CategoryDto>> GetTreeAsync()
        {
            var all = (await _uow.Repository<Category>().GetAllAsync())
                .Select(c => c.ToDto())
                .ToList();

            var byId = all.ToDictionary(c => c.Id);
            var roots = new List<CategoryDto>();

            foreach (var node in all)
            {
                if (node.ParentId is int pid && byId.TryGetValue(pid, out var parent))
                    parent.Children.Add(node);
                else
                    roots.Add(node);
            }

            return roots;
        }

        public async Task<CategoryDto?> GetByIdAsync(int id)
        {
            var entity = await _uow.Repository<Category>().GetByIdAsync(id);
            return entity?.ToDto();
        }

        public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
        {
            var entity = dto.ToEntity();
            await _uow.Repository<Category>().AddAsync(entity);
            await _uow.SaveChangesAsync();
            return entity.ToDto();
        }

        public async Task<bool> UpdateAsync(int id, UpdateCategoryDto dto)
        {
            var repo = _uow.Repository<Category>();
            var entity = await repo.GetByIdAsync(id);
            if (entity is null)
                return false;

            dto.Apply(entity);
            repo.Update(entity);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var repo = _uow.Repository<Category>();
            var entity = await repo.GetByIdAsync(id);
            if (entity is null)
                return false;

            repo.Remove(entity);
            await _uow.SaveChangesAsync();
            return true;
        }
    }
}

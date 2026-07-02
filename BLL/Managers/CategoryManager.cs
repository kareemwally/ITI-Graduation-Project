using BLL.DTOs.Categories;
using BLL.DTOs.Common;
using BLL.Mapping.Categories;
using DAL.Models;
using DAL.UnitOfWork;

namespace BLL.Managers
{
    public class CategoryManager : ICategoryManager
    {
        private readonly IUnitOfWork _uow;

        public CategoryManager(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<BaseResponse<List<CategoryDto>>> GetAllAsync()
        {
            var categories = await _uow.Repository<Category>().GetAllAsync();
            var dtos = categories.Select(c => c.ToDto()).ToList();
            return BaseResponse<List<CategoryDto>>.Success(dtos, "تم جلب جميع الفئات بنجاح.");
        }

        public async Task<BaseResponse<List<CategoryDto>>> GetTreeAsync()
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

            return BaseResponse<List<CategoryDto>>.Success(roots, "تم جلب شجرة الفئات بنجاح.");
        }

        public async Task<BaseResponse<CategoryDto>> GetByIdAsync(int id)
        {
            var entity = await _uow.Repository<Category>().GetByIdAsync(id);
            if (entity is null)
                return BaseResponse<CategoryDto>.Failure("هذه الفئة غير موجودة.", statusCode: 404);

            return BaseResponse<CategoryDto>.Success(entity.ToDto(), "تم جلب الفئة بنجاح.");
        }

        public async Task<BaseResponse<CategoryDto>> CreateAsync(CreateCategoryDto dto)
        {
            var entity = dto.ToEntity();
            await _uow.Repository<Category>().AddAsync(entity);
            await _uow.SaveChangesAsync();
            return BaseResponse<CategoryDto>.Success(entity.ToDto(), "تم إنشاء الفئة بنجاح.", statusCode: 201);
        }

        public async Task<BaseResponse<bool>> UpdateAsync(int id, UpdateCategoryDto dto)
        {
            var repo = _uow.Repository<Category>();
            var entity = await repo.GetByIdAsync(id);
            if (entity is null)
                return BaseResponse<bool>.Failure("هذه الفئة غير موجودة.", statusCode: 404);

            dto.Apply(entity);
            repo.Update(entity);
            await _uow.SaveChangesAsync();
            return BaseResponse<bool>.Success(true, "تم تحديث الفئة بنجاح.");
        }

        public async Task<BaseResponse<bool>> DeleteAsync(int id)
        {
            var repo = _uow.Repository<Category>();
            var entity = await repo.GetByIdAsync(id);
            if (entity is null)
                return BaseResponse<bool>.Failure("هذه الفئة غير موجودة.", statusCode: 404);

            repo.Remove(entity);
            await _uow.SaveChangesAsync();
            return BaseResponse<bool>.Success(true, "تم حذف الفئة بنجاح.");
        }
    }
}

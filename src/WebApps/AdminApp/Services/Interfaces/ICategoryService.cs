using Examination.Shared.Categories;
using Examination.Shared.SeedWork;

namespace AdminApp.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<PagedList<CategoryDto>> GetCategoriesPagingAsync(CategorySearch taskListSearch);
        Task<CategoryDto> GetCategoryByIdAsync(string id);
        Task<bool> CreateAsync(CreateCategoryRequest request);
        Task<bool> UpdateAsync(UpdateCategoryRequest request);
        Task<bool> DeleteAsync(string id);
    }
}

using Examination.Shared.Questions;
using Examination.Shared.SeedWork;

namespace AdminApp.Services.Interfaces
{
    public interface IQuestionService
    {
        Task<ApiResult<PagedList<QuestionDto>>> GetQuestionsPagingAsync(QuestionSearch taskListSearch);
        Task<ApiResult<QuestionDto>> GetQuestionByIdAsync(string id);
        Task<bool> CreateAsync(CreateQuestionRequest request);
        Task<bool> UpdateAsync(UpdateQuestionRequest request);
        Task<bool> DeleteAsync(string id);
    }
}

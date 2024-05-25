using Examination.Shared;
using Examination.Shared.Exams;
using Examination.Shared.SeedWork;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Examination.Application.Queries.V1.Exams.GetExamsPaging
{
    public class GetExamsPagingQuery : IRequest<ApiResult<PagedList<ExamDto>>>
    {
        public string? CategoryId { get; set; }
        public string? SearchKeyword { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }
}

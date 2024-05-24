using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Examination.Shared;
using Examination.Shared.SeedWork;
using MediatR;

namespace Examination.Application.Queries.V1.Exams.GetHomeExamList
{
    public class GetHomeExamListQuery : IRequest<ApiResult<IEnumerable<ExamDto>>>
    {
        public GetHomeExamListQuery()
        {
        }
    }
}
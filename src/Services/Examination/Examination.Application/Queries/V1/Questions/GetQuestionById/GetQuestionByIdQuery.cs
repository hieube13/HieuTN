using Examination.Shared.Questions;
using Examination.Shared.SeedWork;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Examination.Application.Queries.V1.Questions.GetQuestionById
{
    public class GetQuestionByIdQuery : IRequest<ApiResult<QuestionDto>>
    {
        public GetQuestionByIdQuery(string id)
        {
            Id = id;
        }
        public string Id { set; get; }
    }
}

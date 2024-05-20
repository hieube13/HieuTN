using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Examination.Dtos;
using MediatR;

namespace Examination.Application.Queries.GetHomeExamList
{
    public class GetHomeExamListQuery : IRequest<IEnumerable<ExamDto>>
    {

    }
}
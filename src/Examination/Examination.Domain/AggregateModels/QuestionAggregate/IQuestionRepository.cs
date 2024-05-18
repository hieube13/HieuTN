using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Examination.Domain.SeedWork;

namespace Examination.Domain.AggregateModels.QuestionAggregate
{
    public interface IQuestionRepository : IRepositoryBase<Question>
    {
    }
}
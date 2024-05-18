using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Examination.Domain.SeedWork
{
    public interface IRepository<T> where T : IAggregateRoot
    {
        IUnitOfWork UnitOfWork { get; }
    }
}
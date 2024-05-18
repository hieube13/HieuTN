using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Examination.Domain.Exceptions
{
    public class ExamDomainException : Exception
    {
        public ExamDomainException()
        { }

        public ExamDomainException(string message)
            : base(message)
        { }

        public ExamDomainException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
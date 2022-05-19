using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Stacy.Core.Exceptions
{
    public class CoreUserAggregateException : AggregateException, ICoreException
    {
        public CoreUserAggregateException()
        {
        }

        public CoreUserAggregateException(string message) : base(message)
        {
        }

        public CoreUserAggregateException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public CoreUserAggregateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public CoreUserAggregateException(Exception[] exceptions) : base(exceptions)
        {
            
        }

        public CoreUserAggregateException(IEnumerable<CoreUserException> exceptions) : base(exceptions)
        {
        }

        public CoreUserAggregateException(string message, Exception[] exceptions) : base(message, exceptions)
        {

        }

        public CoreUserAggregateException(string message, IEnumerable<Exception> exceptions) : base(message, exceptions)
        {
        }
    }
}

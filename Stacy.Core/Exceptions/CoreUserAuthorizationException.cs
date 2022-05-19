using System;
using System.Runtime.Serialization;

namespace Stacy.Core.Exceptions
{
    public class CoreUserAuthorizationException : Exception, ICoreException
    {
        private string _internalMessage;

        public string InternalMessage
        {
            get { return !string.IsNullOrEmpty(_internalMessage) ? _internalMessage : ExtractRootCause(); }
            set { _internalMessage = value; }
        }

        public CoreUserAuthorizationException()
        {
        }

        public CoreUserAuthorizationException(string message)
            : base(message)
        {
        }

        public CoreUserAuthorizationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public CoreUserAuthorizationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private string ExtractRootCause()
        {
            string message = null;
            Exception ex = this;

            try
            {
                while (ex?.InnerException != null && (ex is CoreUserAuthorizationException || ex is CoreUserException || ex is CoreUserAggregateException))
                {
                    ex = ex.InnerException;
                    message = ex.Message;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return message;
        }
    }
}

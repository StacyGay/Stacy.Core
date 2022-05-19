using System;
using System.Runtime.Serialization;

namespace Stacy.Core.Exceptions
{
	public class CoreInternalException : Exception, ICoreException
	{
		public CoreInternalException()
		{
		}

		public CoreInternalException(string message)
			: base(message)
		{
		}

		public CoreInternalException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		public CoreInternalException(SerializationInfo info, StreamingContext context)
			: base(info,context)
		{
		}
	}
}

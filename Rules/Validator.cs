using System;
using System.Collections.Generic;
using System.Linq;

namespace Stacy.Core.Rules
{
	public class Validator : IValidator
	{
		public List<Exception> Exceptions { get; set; }

		public Validator()
		{
			Exceptions = new List<Exception>();
		}

		public virtual void Reset()
		{
			Exceptions = new List<Exception>();
		}

		public virtual void ThrowExceptions()
		{
			if (Exceptions.Any())
				throw new AggregateException(Exceptions);
		}

		public virtual List<Exception> GetExceptions()
		{
			return Exceptions;
		}
	}
}

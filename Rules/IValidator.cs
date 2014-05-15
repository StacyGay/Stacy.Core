using System;
using System.Collections.Generic;

namespace Stacy.Core.Rules
{
	public interface IValidator
	{
		List<Exception> Exceptions { get; set; }
		
		void Reset();
		void ThrowExceptions();
		List<Exception> GetExceptions();
	}
}
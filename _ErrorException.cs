using System;

namespace Stacy.Core
{
	[Serializable]
	public class _ErrorException : Exception
	{
		public _ErrorException(string errorMessage) : base(errorMessage)
		{
		}

		public _ErrorException(string errorMessage, Exception innerEx) : base(errorMessage, innerEx)
		{
		}

		public string ErrorMessage
		{
			get { return base.Message; }
		}
	}
}
namespace Stacy.Core.Connection
{
	public interface ISoapConnectionFactory<TClass, TInterface>
		where TInterface : class
	{
		string ServiceUrl { get; }
		string Request { get; }
		string Response { get; }

		TClass GetConnection();
	}
}

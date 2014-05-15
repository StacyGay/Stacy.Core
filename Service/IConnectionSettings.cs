namespace Stacy.Core.Service
{
	public interface IConnectionSettings
	{
		int ClientId { get; set; }
		int AccomId { get; set; }
		int IQWareGUID { get; set; }
		string PMSPort { get; set; }
		string PMSSecurePort { get; set; }
		bool AlwaysUseSSL { get; set; }
		string PMSWebServiceURL { get; set; }
	}
}

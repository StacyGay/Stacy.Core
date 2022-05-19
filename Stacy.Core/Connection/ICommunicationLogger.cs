namespace Stacy.Core.Connection
{
	public interface ICommunicationLogger
	{
        string ServiceType { get; set; }
		void LogRequest(CommunicationLog log);
        void LogResponse(CommunicationLog log);
	}
}

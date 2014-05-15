using System;

namespace Stacy.Core.Service
{
	public interface IConnectService<TClass, TInterface>
		where TClass : ClientBase<TInterface>, TInterface, new()
		where TInterface : class
	{
		string GetXMLReply();
		string GetXMLRequest();

		IConnectService<TClass, TInterface> SetEndpoint(bool boolUseSSL, string strServiceAddress, string port = "80",
		                                                string securePort = "443");

		string GetInspectorReply();
		string GetInspectorRequest();
		void logXML(string ecrmClientID, DateTime WhenSent, string receiveStatus, bool isError);
	}
}
namespace Stacy.Core.Service
{
	public class MInspector : IClientMessageInspector
	{
		public string xmlReply = "";
		public string xmlRequest = "";

		public void AfterReceiveReply(ref Message reply, object correlationState)
		{
			xmlReply = reply.ToString();
		}

		public object BeforeSendRequest(ref Message request, IClientChannel channel)
		{
			xmlRequest = request.ToString();
			return null;
		}
	}
}
using System.Xml.Serialization;

namespace Stacy.Core.Connection
{
    public abstract class ManualSoapResponse
    {
        [XmlRoot(ElementName = "soap:Fault")]
        public class ManualSoapFault
        {
            [XmlElement("faultcode")]
            public string FaultCode { get; set; }
            [XmlElement("faultstring")]
            public string FaultString { get; set; }
        }
    }
}

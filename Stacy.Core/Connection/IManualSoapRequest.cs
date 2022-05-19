using System.Xml.Serialization;

namespace Stacy.Core.Connection
{
    public interface IManualSoapRequest
    {
        [XmlIgnore]
        string SoapAction { get; }
    }
}

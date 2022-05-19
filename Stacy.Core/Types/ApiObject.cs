using System;
using System.Linq;
using Newtonsoft.Json;

namespace Stacy.Core.Types
{
    /// <summary>
    /// Inheriting from ApiObject abstract class provides the $type property for setting up deserialization of derived types for api action parameters
    /// </summary>
    public abstract class ApiObject
    {
        [JsonProperty(PropertyName = "$type", Order = -99)]
        public Type Type  => GetType();
    }
}

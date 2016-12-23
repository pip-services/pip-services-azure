using System.Runtime.Serialization;
using Newtonsoft.Json;
using PipServices.Commons.Data;

namespace DummyStatelessService.Model
{
    [DataContract]
    [JsonObject(MemberSerialization.OptIn)]
    public class DummyObject : IIdentifiable<string>
    {
        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DataMember(Name = "id", IsRequired = true)]
        public string Id { get; set; }

        [JsonProperty("key", DefaultValueHandling = DefaultValueHandling.Include)]
        [DataMember(Name = "key", IsRequired = true)]
        public string Key { get; set; }

        [JsonProperty("content", DefaultValueHandling = DefaultValueHandling.Include)]
        [DataMember(Name = "content", IsRequired = false)]
        public string Content { get; set; }
    }
}
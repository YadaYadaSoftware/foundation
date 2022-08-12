using Newtonsoft.Json;
using YadaYada.Bisque.Annotations;
using YadaYada.Bisque.Aws.CloudFormation;

namespace Data.Serverless.Snapshot
{
    public class SnapshotCustomResource : CustomResource, ISnapshotInfo
    {
        public SnapshotCustomResource(string key):base(key)
        {
            
        }
        [JsonProperty(PropertyName = "Properties." + nameof(DbInstanceId))]
        public CloudVariant DbInstanceId { get; set; }

        [JsonProperty(PropertyName = "Properties." + nameof(SnapshotName))]
        public CloudVariant SnapshotName { get; set; }

        [JsonProperty(PropertyName = "Properties." + nameof(Enabled))]
        public CloudVariant Enabled { get; set; }
    }
}

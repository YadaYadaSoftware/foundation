using YadaYada.Bisque.Annotations;

namespace Data.Serverless.Snapshot;

public class SnapshotRequestProperties : ISnapshotInfo
{
    public CloudVariant DbInstanceId { get; set; }
    public CloudVariant SnapshotName { get; set; }
    public CloudVariant Enabled { get; set; }
}
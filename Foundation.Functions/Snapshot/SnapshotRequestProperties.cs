using YadaYada.Bisque.Annotations;

namespace Foundation.Functions.Snapshot;

public class SnapshotRequestProperties : ISnapshotInfo
{
    public CloudVariant DbInstanceId { get; set; }
    public CloudVariant SnapshotName { get; set; }
    public CloudVariant Enabled { get; set; }
}
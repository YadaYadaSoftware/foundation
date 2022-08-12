using YadaYada.Bisque.Annotations;

namespace Data.Serverless.Snapshot;

public interface ISnapshotInfo
{
    CloudVariant DbInstanceId { get; set; }
    CloudVariant SnapshotName { get; set; }
    CloudVariant Enabled { get; set; }
}
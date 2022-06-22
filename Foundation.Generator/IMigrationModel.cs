namespace Foundation.Generators;

public interface IMigrationModel
{
    string FunctionResourceName { get; }
    string Name { get; }
    string ResourceName { get; }
    string Namespace { get; }
    string FullName { get; }
    string Id { get; }
}
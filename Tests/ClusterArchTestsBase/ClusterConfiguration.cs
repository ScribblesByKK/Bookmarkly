namespace ClusterArchTestsBase;

/// <summary>
/// Configuration that defines the assembly names for a module/cluster's projects.
/// Each module/cluster will have a set of projects following the same naming pattern.
/// </summary>
public sealed class ClusterConfiguration
{
    /// <summary>
    /// Creates a new cluster configuration.
    /// </summary>
    /// <param name="clusterPrefix">
    /// The prefix for the cluster's assembly names. 
    /// For example, "Bookmarkly" for the Bookmarkly module.
    /// </param>
    public ClusterConfiguration(string clusterPrefix)
    {
        ClusterPrefix = clusterPrefix;
        EntitiesAssemblyName = $"{clusterPrefix}.Entities.Abstractions";
        LibraryAssemblyName = $"{clusterPrefix}.Library.Abstractions";
        ViewContractsAssemblyName = $"{clusterPrefix}.Views.Abstractions";
        ViewModelsAssemblyName = $"{clusterPrefix}.ViewModels";
        ViewsAssemblyName = $"{clusterPrefix}.Views";
    }

    /// <summary>
    /// Gets the prefix for the cluster's assembly names.
    /// </summary>
    public string ClusterPrefix { get; }

    /// <summary>
    /// Gets the assembly name for the Entities project.
    /// </summary>
    public string EntitiesAssemblyName { get; }

    /// <summary>
    /// Gets the assembly name for the Library project.
    /// </summary>
    public string LibraryAssemblyName { get; }

    /// <summary>
    /// Gets the assembly name for the ViewContracts project.
    /// </summary>
    public string ViewContractsAssemblyName { get; }

    /// <summary>
    /// Gets the assembly name for the ViewModels project.
    /// </summary>
    public string ViewModelsAssemblyName { get; }

    /// <summary>
    /// Gets the assembly name for the Views project.
    /// </summary>
    public string ViewsAssemblyName { get; }
}

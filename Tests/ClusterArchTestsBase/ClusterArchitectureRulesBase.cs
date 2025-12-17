using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace ClusterArchTestsBase;

/// <summary>
/// Base class for cluster architecture tests that provides reusable architecture rules.
/// Inherit from this class in each module's architecture test project.
/// </summary>
public abstract class ClusterArchitectureRulesBase
{
    private readonly ClusterConfiguration _config;
    private Architecture? _architecture;
    private IObjectProvider<IType>? _entitiesTypes;
    private IObjectProvider<IType>? _libraryTypes;
    private IObjectProvider<IType>? _viewContractsTypes;
    private IObjectProvider<IType>? _viewModelsTypes;
    private IObjectProvider<IType>? _viewsTypes;

    /// <summary>
    /// Creates a new instance of the cluster architecture rules.
    /// </summary>
    /// <param name="config">The cluster configuration.</param>
    protected ClusterArchitectureRulesBase(ClusterConfiguration config)
    {
        _config = config;
    }

    /// <summary>
    /// Gets the assemblies to analyze. Override this in derived classes to provide
    /// the actual assemblies for the module/cluster.
    /// </summary>
    protected abstract IEnumerable<System.Reflection.Assembly> GetAssemblies();

    /// <summary>
    /// Gets the architecture being analyzed.
    /// </summary>
    public Architecture Architecture => _architecture ??= new ArchLoader()
        .LoadAssemblies(GetAssemblies().ToArray())
        .Build();

    /// <summary>
    /// Gets types from the Entities project.
    /// </summary>
    protected IObjectProvider<IType> EntitiesTypes => _entitiesTypes ??=
        Types().That().ResideInAssembly(_config.EntitiesAssemblyName).As("Entities Types");

    /// <summary>
    /// Gets types from the Library project.
    /// </summary>
    protected IObjectProvider<IType> LibraryTypes => _libraryTypes ??=
        Types().That().ResideInAssembly(_config.LibraryAssemblyName).As("Library Types");

    /// <summary>
    /// Gets types from the ViewContracts project.
    /// </summary>
    protected IObjectProvider<IType> ViewContractsTypes => _viewContractsTypes ??=
        Types().That().ResideInAssembly(_config.ViewContractsAssemblyName).As("ViewContracts Types");

    /// <summary>
    /// Gets types from the ViewModels project.
    /// </summary>
    protected IObjectProvider<IType> ViewModelsTypes => _viewModelsTypes ??=
        Types().That().ResideInAssembly(_config.ViewModelsAssemblyName).As("ViewModels Types");

    /// <summary>
    /// Gets types from the Views project.
    /// </summary>
    protected IObjectProvider<IType> ViewsTypes => _viewsTypes ??=
        Types().That().ResideInAssembly(_config.ViewsAssemblyName).As("Views Types");

    #region Dependency Rules

    /// <summary>
    /// Rule: Entities can be referenced by Library, ViewContracts, ViewModels and Views.
    /// This is validated by ensuring Entities don't depend on any of these projects.
    /// </summary>
    public IArchRule EntitiesShouldNotDependOnOtherClusterProjects =>
        Types().That().ResideInAssembly(_config.EntitiesAssemblyName)
            .Should().NotDependOnAny(LibraryTypes)
            .AndShould().NotDependOnAny(ViewContractsTypes)
            .AndShould().NotDependOnAny(ViewModelsTypes)
            .AndShould().NotDependOnAny(ViewsTypes)
            .Because("Entities should be a foundational layer with no dependencies on other cluster projects")
            .WithoutRequiringPositiveResults();

    /// <summary>
    /// Rule: Library can only be referenced by ViewModels.
    /// Library should not depend on ViewContracts, ViewModels, or Views.
    /// </summary>
    public IArchRule LibraryShouldNotDependOnViewLayers =>
        Types().That().ResideInAssembly(_config.LibraryAssemblyName)
            .Should().NotDependOnAny(ViewContractsTypes)
            .AndShould().NotDependOnAny(ViewModelsTypes)
            .AndShould().NotDependOnAny(ViewsTypes)
            .Because("Library should only be referenced by ViewModels and should not depend on view-related projects")
            .WithoutRequiringPositiveResults();

    /// <summary>
    /// Rule: ViewContracts should not depend on ViewModels or Views.
    /// </summary>
    public IArchRule ViewContractsShouldNotDependOnImplementations =>
        Types().That().ResideInAssembly(_config.ViewContractsAssemblyName)
            .Should().NotDependOnAny(ViewModelsTypes)
            .AndShould().NotDependOnAny(ViewsTypes)
            .Because("ViewContracts is the bridge between Views and ViewModels and should only contain abstractions")
            .WithoutRequiringPositiveResults();

    /// <summary>
    /// Rule: ViewModels should not depend on Views.
    /// </summary>
    public IArchRule ViewModelsShouldNotDependOnViews =>
        Types().That().ResideInAssembly(_config.ViewModelsAssemblyName)
            .Should().NotDependOnAny(ViewsTypes)
            .Because("ViewModels should be independent of Views to maintain separation of concerns")
            .WithoutRequiringPositiveResults();

    /// <summary>
    /// Rule: Views should not depend on ViewModels directly.
    /// Views should only depend on ViewContracts (abstractions).
    /// </summary>
    public IArchRule ViewsShouldNotDependOnViewModels =>
        Types().That().ResideInAssembly(_config.ViewsAssemblyName)
            .Should().NotDependOnAny(ViewModelsTypes)
            .Because("Views should depend on abstractions (ViewContracts), not on ViewModels directly")
            .WithoutRequiringPositiveResults();

    #endregion

    #region Access Modifier Rules

    /// <summary>
    /// Rule: All classes ending with DataManager, DataManagerBase, or DM must be internal.
    /// Only interfaces can be public.
    /// </summary>
    public IArchRule DataManagerClassesShouldBeInternal =>
        Classes().That()
            .HaveNameEndingWith("DataManager")
            .Or().HaveNameEndingWith("DataManagerBase")
            .Or().HaveNameEndingWith("DM")
            .Should().NotBePublic()
            .Because("DataManager classes must be internal; only their interfaces can be public")
            .WithoutRequiringPositiveResults();

    /// <summary>
    /// Rule: All classes and interfaces ending with Handler must be internal.
    /// </summary>
    public IArchRule HandlersShouldBeInternal =>
        Types().That()
            .HaveNameEndingWith("Handler")
            .Should().NotBePublic()
            .Because("Handler types (classes and interfaces) must be internal")
            .WithoutRequiringPositiveResults();

    /// <summary>
    /// Rule: All classes ending with Usecase must be public.
    /// </summary>
    public IArchRule UseCasesShouldBePublic =>
        Classes().That()
            .HaveNameEndingWith("Usecase")
            .Or().HaveNameEndingWith("UseCase")
            .Should().BePublic()
            .Because("UseCase classes must be public to be accessible from the host app")
            .WithoutRequiringPositiveResults();

    /// <summary>
    /// Rule: All classes in ViewModels must be internal.
    /// </summary>
    public IArchRule ViewModelClassesShouldBeInternal =>
        Classes().That().ResideInAssembly(_config.ViewModelsAssemblyName)
            .Should().NotBePublic()
            .Because("All classes in ViewModels must be internal; only the host app can reference ViewModels")
            .WithoutRequiringPositiveResults();

    /// <summary>
    /// Rule: All interfaces in ViewContracts must be public.
    /// </summary>
    public IArchRule ViewContractsInterfacesShouldBePublic =>
        Interfaces().That().ResideInAssembly(_config.ViewContractsAssemblyName)
            .Should().BePublic()
            .Because("All interfaces defined in ViewContracts must be public as they serve as contracts")
            .WithoutRequiringPositiveResults();

    #endregion

    #region Interface Implementation Rules

    /// <summary>
    /// Rule: Classes ending with ViewModel, ViewModelBase, or VM in ViewModels must implement IViewModel.
    /// </summary>
#pragma warning disable CS0618 // Type or member is obsolete - ImplementInterface API is obsolete but still functional
    public IArchRule ViewModelClassesShouldImplementIViewModel =>
        Classes().That().ResideInAssembly(_config.ViewModelsAssemblyName)
            .And().HaveNameEndingWith("ViewModel")
            .Or().HaveNameEndingWith("ViewModelBase")
            .Or().HaveNameEndingWith("VM")
            .Should().ImplementInterface("IViewModel", useRegularExpressions: true)
            .Because("ViewModel classes must implement the IViewModel interface")
            .WithoutRequiringPositiveResults();

    /// <summary>
    /// Rule: ViewModel interfaces in ViewContracts must extend IViewModel.
    /// </summary>
    public IArchRule ViewModelInterfacesShouldExtendIViewModel =>
        Interfaces().That().ResideInAssembly(_config.ViewContractsAssemblyName)
            .And().HaveNameEndingWith("ViewModel")
            .Should().ImplementInterface("IViewModel", useRegularExpressions: true)
            .Because("ViewModel interfaces in ViewContracts must extend IViewModel by default")
            .WithoutRequiringPositiveResults();

    /// <summary>
    /// Rule: View interfaces in ViewContracts must implement IView.
    /// </summary>
    public IArchRule ViewInterfacesShouldImplementIView =>
        Interfaces().That().ResideInAssembly(_config.ViewContractsAssemblyName)
            .And().HaveNameEndingWith("View")
            .And().DoNotHaveNameEndingWith("ViewModel")
            .Should().ImplementInterface("IView", useRegularExpressions: true)
            .Because("View interfaces in ViewContracts must implement the IView interface")
            .WithoutRequiringPositiveResults();
#pragma warning restore CS0618 // Type or member is obsolete

    #endregion
}

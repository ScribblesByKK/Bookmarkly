using System.Reflection;
using ArchUnitNET.MSTestV2;
using ClusterArchTestsBase;

namespace ArchitectureTests;

/// <summary>
/// Bookmarkly cluster architecture rules implementation.
/// Note: Views assembly is excluded on non-Windows platforms.
/// </summary>
public sealed class BookmarklyClusterArchitectureRules : ClusterArchitectureRulesBase
{
    public BookmarklyClusterArchitectureRules()
        : base(new ClusterConfiguration("Bookmarkly"))
    {
    }

    protected override IEnumerable<Assembly> GetAssemblies()
    {
        // Return the assemblies for the Bookmarkly cluster that can be built cross-platform
        yield return typeof(Bookmarkly.Entities.Abstractions.Placeholder).Assembly;
        yield return typeof(Bookmarkly.Library.Abstractions.Placeholder).Assembly;
        yield return typeof(Bookmarkly.Views.Abstractions.Placeholder).Assembly;
        yield return typeof(Bookmarkly.ViewModels.Placeholder).Assembly;
        // Note: Bookmarkly.Views assembly is excluded because it requires Windows to build
    }
}

[TestClass]
public sealed class BookmarklyArchitectureTests
{
    private static readonly BookmarklyClusterArchitectureRules Rules = new();

    #region Dependency Tests

    [TestMethod]
    public void EntitiesShouldNotDependOnOtherClusterProjects()
    {
        Rules.EntitiesShouldNotDependOnOtherClusterProjects.Check(Rules.Architecture);
    }

    [TestMethod]
    public void LibraryShouldNotDependOnViewLayers()
    {
        Rules.LibraryShouldNotDependOnViewLayers.Check(Rules.Architecture);
    }

    [TestMethod]
    public void ViewContractsShouldNotDependOnImplementations()
    {
        Rules.ViewContractsShouldNotDependOnImplementations.Check(Rules.Architecture);
    }

    [TestMethod]
    public void ViewModelsShouldNotDependOnViews()
    {
        Rules.ViewModelsShouldNotDependOnViews.Check(Rules.Architecture);
    }

    [TestMethod]
    public void ViewsShouldNotDependOnViewModels()
    {
        Rules.ViewsShouldNotDependOnViewModels.Check(Rules.Architecture);
    }

    #endregion

    #region Access Modifier Tests

    [TestMethod]
    public void DataManagerClassesShouldBeInternal()
    {
        Rules.DataManagerClassesShouldBeInternal.Check(Rules.Architecture);
    }

    [TestMethod]
    public void HandlersShouldBeInternal()
    {
        Rules.HandlersShouldBeInternal.Check(Rules.Architecture);
    }

    [TestMethod]
    public void UseCasesShouldBePublic()
    {
        Rules.UseCasesShouldBePublic.Check(Rules.Architecture);
    }

    [TestMethod]
    public void ViewModelClassesShouldBeInternal()
    {
        Rules.ViewModelClassesShouldBeInternal.Check(Rules.Architecture);
    }

    [TestMethod]
    public void ViewContractsInterfacesShouldBePublic()
    {
        Rules.ViewContractsInterfacesShouldBePublic.Check(Rules.Architecture);
    }

    #endregion

    #region Interface Implementation Tests

    [TestMethod]
    public void ViewModelClassesShouldImplementIViewModel()
    {
        Rules.ViewModelClassesShouldImplementIViewModel.Check(Rules.Architecture);
    }

    [TestMethod]
    public void ViewModelInterfacesShouldExtendIViewModel()
    {
        Rules.ViewModelInterfacesShouldExtendIViewModel.Check(Rules.Architecture);
    }

    [TestMethod]
    public void ViewInterfacesShouldImplementIView()
    {
        Rules.ViewInterfacesShouldImplementIView.Check(Rules.Architecture);
    }

    #endregion
}


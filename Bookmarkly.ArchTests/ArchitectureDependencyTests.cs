using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.Fluent;
using Xunit;
using System.Linq;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Bookmarkly.ArchTests;

public class ArchitectureDependencyTests
{
    private static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(
            typeof(Bookmarkly.Entities.Bookmark).Assembly,
            typeof(Bookmarkly.ViewContracts.IBookmarkService).Assembly,
            typeof(Bookmarkly.Library.BookmarkService).Assembly,
            typeof(Bookmarkly.ViewModels.MainViewModel).Assembly
        )
        .Build();

    [Fact]
    public void ViewModels_Should_Only_Reference_ViewContracts_Library_And_Entities()
    {
        // Get all types in ViewModels namespace
        var viewModelsTypes = Architecture.Types
            .Where(t => t.Namespace?.FullName.StartsWith("Bookmarkly.ViewModels") == true);

        // Check each type's dependencies
        foreach (var type in viewModelsTypes)
        {
            foreach (var dependency in type.Dependencies)
            {
                var targetNs = dependency.Target.Namespace?.FullName;
                if (targetNs == null) continue;
                
                var isAllowed = targetNs.StartsWith("Bookmarkly.ViewModels") ||
                               targetNs.StartsWith("Bookmarkly.ViewContracts") ||
                               targetNs.StartsWith("Bookmarkly.Library") ||
                               targetNs.StartsWith("Bookmarkly.Entities") ||
                               targetNs.StartsWith("System") ||
                               targetNs.StartsWith("Microsoft");
                
                Assert.True(isAllowed, 
                    $"ViewModels type '{type.FullName}' should not depend on '{dependency.Target.FullName}' from namespace '{targetNs}'");
            }
        }
    }

    [Fact]
    public void ViewContracts_Should_Only_Reference_Entities()
    {
        var viewContractsTypes = Architecture.Types
            .Where(t => t.Namespace?.FullName.StartsWith("Bookmarkly.ViewContracts") == true);

        foreach (var type in viewContractsTypes)
        {
            foreach (var dependency in type.Dependencies)
            {
                var targetNs = dependency.Target.Namespace?.FullName;
                if (targetNs == null) continue;
                
                var isAllowed = targetNs.StartsWith("Bookmarkly.ViewContracts") ||
                               targetNs.StartsWith("Bookmarkly.Entities") ||
                               targetNs.StartsWith("System") ||
                               targetNs.StartsWith("Microsoft");
                
                Assert.True(isAllowed, 
                    $"ViewContracts type '{type.FullName}' should not depend on '{dependency.Target.FullName}' from namespace '{targetNs}'");
            }
        }
    }

    [Fact]
    public void Library_Should_Only_Reference_Entities()
    {
        var libraryTypes = Architecture.Types
            .Where(t => t.Namespace?.FullName.StartsWith("Bookmarkly.Library") == true);

        foreach (var type in libraryTypes)
        {
            foreach (var dependency in type.Dependencies)
            {
                var targetNs = dependency.Target.Namespace?.FullName;
                if (targetNs == null) continue;
                
                var isAllowed = targetNs.StartsWith("Bookmarkly.Library") ||
                               targetNs.StartsWith("Bookmarkly.Entities") ||
                               targetNs.StartsWith("System") ||
                               targetNs.StartsWith("Microsoft");
                
                Assert.True(isAllowed, 
                    $"Library type '{type.FullName}' should not depend on '{dependency.Target.FullName}' from namespace '{targetNs}'");
            }
        }
    }
}
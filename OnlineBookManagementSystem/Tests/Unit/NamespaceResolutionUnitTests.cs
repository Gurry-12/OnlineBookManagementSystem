using System.Text.RegularExpressions;
using Xunit;

namespace OnlineBookManagementSystem.Tests.Unit;

/// <summary>
/// Unit tests for namespace resolution consistency
/// Feature: clean-code-refactoring
/// </summary>
public class NamespaceResolutionUnitTests
{
    /// <summary>
    /// Test that controllers and views don't contain old namespace references
    /// Validates: Requirements 1.2, 1.3
    /// </summary>
    [Fact]
    [Trait("Feature", "clean-code-refactoring")]
    public void Controllers_And_Views_Should_Not_Have_Old_Namespace_References()
    {
        // Arrange
        var projectRoot = GetProjectRoot();
        Assert.NotNull(projectRoot);

        var controllerFiles = Directory.GetFiles(
            Path.Combine(projectRoot, "Presentation", "Controllers"),
            "*.cs",
            SearchOption.AllDirectories);

        var viewFiles = Directory.GetFiles(
            Path.Combine(projectRoot, "Presentation", "Views"),
            "*.cshtml",
            SearchOption.AllDirectories);

        var allFiles = controllerFiles.Concat(viewFiles);

        // Act & Assert
        foreach (var file in allFiles)
        {
            var content = File.ReadAllText(file);
            var fileName = Path.GetFileName(file);

            // Check for old namespace references that should not exist
            Assert.False(HasOldNamespaceReferences(content),
                $"File {fileName} contains old namespace references that should be updated");
        }
    }

    /// <summary>
    /// Test that ViewModel references use correct new namespaces
    /// Validates: Requirements 1.2, 1.3
    /// </summary>
    [Fact]
    [Trait("Feature", "clean-code-refactoring")]
    public void ViewModels_Should_Use_Correct_Namespaces()
    {
        // Arrange
        var projectRoot = GetProjectRoot();
        Assert.NotNull(projectRoot);

        var controllerFiles = Directory.GetFiles(
            Path.Combine(projectRoot, "Presentation", "Controllers"),
            "*.cs",
            SearchOption.AllDirectories);

        var viewFiles = Directory.GetFiles(
            Path.Combine(projectRoot, "Presentation", "Views"),
            "*.cshtml",
            SearchOption.AllDirectories);

        var allFiles = controllerFiles.Concat(viewFiles);

        // Act & Assert
        foreach (var file in allFiles)
        {
            var content = File.ReadAllText(file);
            var fileName = Path.GetFileName(file);

            // Check that ViewModel references use correct new namespaces
            Assert.True(HasCorrectViewModelNamespaces(content, file),
                $"File {fileName} has incorrect ViewModel namespace references");
        }
    }

    private static string? GetProjectRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();

        // Look for the project root by finding the .csproj file
        while (currentDir != null && !Directory.GetFiles(currentDir, "*.csproj").Any())
        {
            currentDir = Directory.GetParent(currentDir)?.FullName;
        }

        return currentDir;
    }

    private static bool HasOldNamespaceReferences(string content)
    {
        // Check for old namespace patterns that should not exist
        var oldNamespacePatterns = new[]
        {
            @"OnlineBookManagementSystem\.Models\.ViewModel",
            @"OnlineBookManagementSystem\.Models\.",
            @"OnlineBookManagementSystem\.Controllers\.", // Except for inheritance
            @"OnlineBookManagementSystem\.Interfaces\."
        };

        foreach (var pattern in oldNamespacePatterns)
        {
            if (Regex.IsMatch(content, pattern))
            {
                // Allow Controllers namespace only for inheritance (: BaseController)
                if (pattern.Contains("Controllers") &&
                    Regex.IsMatch(content, @":\s*BaseController"))
                {
                    continue;
                }
                return true;
            }
        }

        return false;
    }

    private static bool HasCorrectViewModelNamespaces(string content, string filePath)
    {
        // Define correct namespace mappings for ViewModels
        var viewModelNamespaces = new Dictionary<string, string>
        {
            ["BookFormViewModel"] = "OnlineBookManagementSystem.Presentation.ViewModels.Books",
            ["BookListViewModel"] = "OnlineBookManagementSystem.Presentation.ViewModels.Books",
            ["BookDetailsViewModel"] = "OnlineBookManagementSystem.Presentation.ViewModels.Books",
            ["CategoryClassifyViewModel"] = "OnlineBookManagementSystem.Presentation.ViewModels.Books",

            ["LoginViewModel"] = "OnlineBookManagementSystem.Presentation.ViewModels.AuthViewModels",
            ["RegisterViewModel"] = "OnlineBookManagementSystem.Presentation.ViewModels.AuthViewModels",
            ["ForgotPasswordViewModel"] = "OnlineBookManagementSystem.Presentation.ViewModels.AuthViewModels",
            ["ResetPasswordViewModel"] = "OnlineBookManagementSystem.Presentation.ViewModels.AuthViewModels",

            ["CartViewModel"] = "OnlineBookManagementSystem.Presentation.ViewModels.Cart",
            ["CartSummaryViewModel"] = "OnlineBookManagementSystem.Presentation.ViewModels.Cart",
            ["CheckOutViewModel"] = "OnlineBookManagementSystem.Presentation.ViewModels.Cart",
            ["CheckOutRequestViewModel"] = "OnlineBookManagementSystem.Presentation.ViewModels.Cart",
            ["ShoppingCartViewModel"] = "OnlineBookManagementSystem.Presentation.ViewModels.Cart",
            ["AdminCartViewModel"] = "OnlineBookManagementSystem.Presentation.ViewModels.Admin",

            ["AdminViewModel"] = "OnlineBookManagementSystem.Presentation.ViewModels.Admin",
            ["AdminDashboardViewModel"] = "OnlineBookManagementSystem.Presentation.ViewModels.Admin",
            ["AdminOrderListViewModel"] = "OnlineBookManagementSystem.Presentation.ViewModels.Admin",
            ["AdminUsersViewModel"] = "OnlineBookManagementSystem.Presentation.ViewModels.Admin",

            ["UserViewModel"] = "OnlineBookManagementSystem.Presentation.ViewModels.User",
            ["UserDashboardViewModel"] = "OnlineBookManagementSystem.Presentation.ViewModels.User",
            ["UserProfileViewModel"] = "OnlineBookManagementSystem.Presentation.ViewModels.User",
            ["ProfileViewModel"] = "OnlineBookManagementSystem.Presentation.ViewModels.User",

            ["SuperAdminDashboardViewModel"] = "OnlineBookManagementSystem.Presentation.ViewModels.SuperAdmin",
            ["ManageUsersViewModel"] = "OnlineBookManagementSystem.Presentation.ViewModels.SuperAdmin",
            ["SystemSettingsViewModel"] = "OnlineBookManagementSystem.Presentation.ViewModels.SuperAdmin",
            ["UserWithRoleViewModel"] = "OnlineBookManagementSystem.Presentation.ViewModels.SuperAdmin",

            ["ActivityLogViewModel"] = "OnlineBookManagementSystem.Presentation.ViewModels.Activity",
            ["ActivityLogsViewModel"] = "OnlineBookManagementSystem.Presentation.ViewModels.Activity",

            ["ReviewDisplayViewModel"] = "OnlineBookManagementSystem.Presentation.ViewModels.Reviews",
            ["ReviewModerationViewModel"] = "OnlineBookManagementSystem.Presentation.ViewModels.Reviews",
            ["ReviewAnalyticsViewModel"] = "OnlineBookManagementSystem.Presentation.ViewModels.Reviews",
            ["BookRatingViewModel"] = "OnlineBookManagementSystem.Presentation.ViewModels.Reviews",
            ["PaginatedResult"] = "OnlineBookManagementSystem.Presentation.ViewModels.Reviews",

            ["CategoryViewModel"] = "OnlineBookManagementSystem.Presentation.ViewModels.Shared",
            ["CategoryWithCount"] = "OnlineBookManagementSystem.Presentation.ViewModels.Shared"
        };

        // Check if file uses ViewModels and if they have correct namespaces
        foreach (var kvp in viewModelNamespaces)
        {
            var viewModelName = kvp.Key;
            var expectedNamespace = kvp.Value;

            // If the file references this ViewModel
            if (Regex.IsMatch(content, $@"\b{Regex.Escape(viewModelName)}\b"))
            {
                // Check if it has the correct using statement or full namespace
                var hasCorrectUsing = Regex.IsMatch(content, $@"using\s+{Regex.Escape(expectedNamespace)};");
                var hasCorrectFullNamespace = Regex.IsMatch(content, $@"{Regex.Escape(expectedNamespace)}\.{Regex.Escape(viewModelName)}");

                if (!hasCorrectUsing && !hasCorrectFullNamespace)
                {
                    return false;
                }
            }
        }

        return true;
    }
}
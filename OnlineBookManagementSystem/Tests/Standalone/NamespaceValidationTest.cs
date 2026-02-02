using System.Text.RegularExpressions;

namespace OnlineBookManagementSystem.Tests.Standalone;

/// <summary>
/// Standalone test for namespace resolution validation
/// This can be run independently of the main project build
/// </summary>
public class NamespaceValidationTest
{
    public static void Main(string[] args)
    {
        var validator = new NamespaceValidationTest();
        var result = validator.ValidateNamespaceResolution();

        Console.WriteLine($"Namespace Resolution Test: {(result ? "PASSED" : "FAILED")}");

        if (!result)
        {
            Environment.Exit(1);
        }
    }

    public bool ValidateNamespaceResolution()
    {
        try
        {
            var projectRoot = GetProjectRoot();
            if (projectRoot == null)
            {
                Console.WriteLine("Could not find project root");
                return false;
            }

            Console.WriteLine($"Testing namespace resolution in: {projectRoot}");

            var controllerFiles = Directory.GetFiles(
                Path.Combine(projectRoot, "Presentation", "Controllers"),
                "*.cs",
                SearchOption.AllDirectories);

            var viewFiles = Directory.GetFiles(
                Path.Combine(projectRoot, "Presentation", "Views"),
                "*.cshtml",
                SearchOption.AllDirectories);

            var allFiles = controllerFiles.Concat(viewFiles);
            var totalFiles = allFiles.Count();
            var passedFiles = 0;

            Console.WriteLine($"Checking {totalFiles} files...");

            foreach (var file in allFiles)
            {
                var content = File.ReadAllText(file);
                var fileName = Path.GetFileName(file);

                // Check for old namespace references
                if (HasOldNamespaceReferences(content))
                {
                    Console.WriteLine($"FAIL: {fileName} contains old namespace references");
                    return false;
                }

                // Check for correct ViewModel namespaces
                if (!HasCorrectViewModelNamespaces(content, file))
                {
                    Console.WriteLine($"FAIL: {fileName} has incorrect ViewModel namespace references");
                    return false;
                }

                passedFiles++;
            }

            Console.WriteLine($"SUCCESS: All {passedFiles} files passed namespace validation");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
            return false;
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
        var oldNamespacePatterns = new[]
        {
            @"OnlineBookManagementSystem\.Models\.ViewModel",
            @"OnlineBookManagementSystem\.Models\.",
            @"OnlineBookManagementSystem\.Controllers\.",
            @"OnlineBookManagementSystem\.Interfaces\."
        };

        foreach (var pattern in oldNamespacePatterns)
        {
            if (Regex.IsMatch(content, pattern))
            {
                // Allow Controllers namespace only for inheritance
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
            ["CheckOutViewModel"] = "OnlineBookManagementSystem.Presentation.ViewModels.Cart",

            ["AdminDashboardViewModel"] = "OnlineBookManagementSystem.Presentation.ViewModels.Admin",
            ["AdminOrderListViewModel"] = "OnlineBookManagementSystem.Presentation.ViewModels.Admin",

            ["UserDashboardViewModel"] = "OnlineBookManagementSystem.Presentation.ViewModels.User",
            ["UserProfileViewModel"] = "OnlineBookManagementSystem.Presentation.ViewModels.User",
            ["ProfileViewModel"] = "OnlineBookManagementSystem.Presentation.ViewModels.User",

            ["SuperAdminDashboardViewModel"] = "OnlineBookManagementSystem.Presentation.ViewModels.SuperAdmin",
            ["ManageUsersViewModel"] = "OnlineBookManagementSystem.Presentation.ViewModels.SuperAdmin",
            ["SystemSettingsViewModel"] = "OnlineBookManagementSystem.Presentation.ViewModels.SuperAdmin",
            ["UserWithRoleViewModel"] = "OnlineBookManagementSystem.Presentation.ViewModels.SuperAdmin",

            ["ActivityLogViewModel"] = "OnlineBookManagementSystem.Presentation.ViewModels.Activity",
            ["ActivityLogsViewModel"] = "OnlineBookManagementSystem.Presentation.ViewModels.Activity",

            ["ReviewAnalyticsViewModel"] = "OnlineBookManagementSystem.Presentation.ViewModels.Reviews",
            ["PaginatedResult"] = "OnlineBookManagementSystem.Presentation.ViewModels.Reviews"
        };

        foreach (var kvp in viewModelNamespaces)
        {
            var viewModelName = kvp.Key;
            var expectedNamespace = kvp.Value;

            if (Regex.IsMatch(content, $@"\b{Regex.Escape(viewModelName)}\b"))
            {
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
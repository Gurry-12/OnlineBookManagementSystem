using Microsoft.AspNetCore.Html;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace OnlineBookManagementSystem.Infrastructure.Services.Infrastructure.Performance
{
    /// <summary>
    /// Service for optimizing web assets including images, CSS, and JavaScript
    /// </summary>
    public class AssetOptimizationService : IAssetOptimizationService
    {
        private readonly ILogger<AssetOptimizationService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public AssetOptimizationService(
            ILogger<AssetOptimizationService> logger,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            _logger = logger;
            _configuration = configuration;
            _environment = environment;
        }

        // Interface implementations
        public async Task<byte[]> OptimizeImageAsync(byte[] imageData, string format, int maxWidth = 1200, int quality = 85)
        {
            await Task.CompletedTask; // For async consistency
            // Basic implementation - in production, use ImageSharp or similar
            return imageData;
        }

        public async Task<Dictionary<string, byte[]>> GenerateResponsiveImagesAsync(byte[] imageData, string format)
        {
            await Task.CompletedTask; // For async consistency
            var result = new Dictionary<string, byte[]>
            {
                ["original"] = imageData,
                ["large"] = imageData,
                ["medium"] = imageData,
                ["small"] = imageData
            };
            return result;
        }

        public string MinifyCss(string cssContent)
        {
            return MinifyCssAsync(cssContent).Result;
        }

        public string MinifyJavaScript(string jsContent)
        {
            return MinifyJavaScriptAsync(jsContent).Result;
        }

        public byte[] CompressContent(byte[] content)
        {
            using var output = new MemoryStream();
            using (var gzip = new GZipStream(output, CompressionMode.Compress))
            {
                gzip.Write(content, 0, content.Length);
            }
            return output.ToArray();
        }

        public string GenerateAssetHash(byte[] content)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(content);
            return Convert.ToHexString(hash)[..8];
        }

        public bool ShouldServeFromCache(string assetPath, DateTime lastModified)
        {
            // Simple cache logic - serve from cache if file is older than 1 hour
            return DateTime.UtcNow - lastModified > TimeSpan.FromHours(1);
        }

        // Additional helper methods

        public string OptimizeImageUrl(string imageUrl, int? width = null, int? height = null, string? format = null)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                    return imageUrl;

                // For now, we'll implement basic optimization
                // In production, you might integrate with services like Cloudinary, ImageKit, etc.
                var optimizedUrl = imageUrl;

                // Add query parameters for optimization hints
                var parameters = new List<string>();

                if (width.HasValue)
                    parameters.Add($"w={width.Value}");

                if (height.HasValue)
                    parameters.Add($"h={height.Value}");

                if (!string.IsNullOrEmpty(format))
                    parameters.Add($"f={format}");

                // Add quality parameter for JPEG images
                if (imageUrl.Contains(".jpg", StringComparison.OrdinalIgnoreCase) ||
                    imageUrl.Contains(".jpeg", StringComparison.OrdinalIgnoreCase))
                {
                    parameters.Add("q=85"); // 85% quality for good balance
                }

                if (parameters.Any())
                {
                    var separator = imageUrl.Contains('?') ? "&" : "?";
                    optimizedUrl = $"{imageUrl}{separator}{string.Join("&", parameters)}";
                }

                return optimizedUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error optimizing image URL: {ImageUrl}", imageUrl);
                return imageUrl; // Return original URL on error
            }
        }

        public HtmlString GenerateLazyLoadImage(string src, string alt, string? cssClass = null, int? width = null, int? height = null)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append("<img");

                // Use data-src for lazy loading
                sb.Append($" data-src=\"{OptimizeImageUrl(src, width, height)}\"");

                // Placeholder image (1x1 transparent pixel)
                sb.Append(" src=\"data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7\"");

                sb.Append($" alt=\"{alt}\"");

                if (!string.IsNullOrEmpty(cssClass))
                    sb.Append($" class=\"{cssClass} lazy\"");
                else
                    sb.Append(" class=\"lazy\"");

                if (width.HasValue)
                    sb.Append($" width=\"{width.Value}\"");

                if (height.HasValue)
                    sb.Append($" height=\"{height.Value}\"");

                // Add loading attribute for native lazy loading support
                sb.Append(" loading=\"lazy\"");

                // Add decoding attribute for better performance
                sb.Append(" decoding=\"async\"");

                sb.Append(">");

                return new HtmlString(sb.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating lazy load image for: {Src}", src);
                return new HtmlString($"<img src=\"{src}\" alt=\"{alt}\">");
            }
        }

        public HtmlString GenerateResponsiveImage(string src, string alt, string? cssClass = null, Dictionary<int, string>? breakpoints = null)
        {
            try
            {
                var sb = new StringBuilder();

                // Use picture element for responsive images
                sb.AppendLine("<picture>");

                if (breakpoints != null && breakpoints.Any())
                {
                    // Sort breakpoints by width (descending)
                    var sortedBreakpoints = breakpoints.OrderByDescending(kvp => kvp.Key);

                    foreach (var breakpoint in sortedBreakpoints)
                    {
                        sb.AppendLine($"  <source media=\"(min-width: {breakpoint.Key}px)\" srcset=\"{OptimizeImageUrl(breakpoint.Value)}\">");
                    }
                }

                // Fallback img element
                sb.Append("  <img");
                sb.Append($" src=\"{OptimizeImageUrl(src)}\"");
                sb.Append($" alt=\"{alt}\"");

                if (!string.IsNullOrEmpty(cssClass))
                    sb.Append($" class=\"{cssClass}\"");

                sb.Append(" loading=\"lazy\"");
                sb.Append(" decoding=\"async\"");
                sb.AppendLine(">");
                sb.Append("</picture>");

                return new HtmlString(sb.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating responsive image for: {Src}", src);
                return new HtmlString($"<img src=\"{src}\" alt=\"{alt}\">");
            }
        }

        public async Task<string> MinifyCssAsync(string css)
        {
            try
            {
                if (string.IsNullOrEmpty(css))
                    return css;

                await Task.CompletedTask; // For async consistency

                // Basic CSS minification
                var minified = css
                    .Replace("\r\n", " ")
                    .Replace("\n", " ")
                    .Replace("\t", " ")
                    .Replace("  ", " ")
                    .Replace(" {", "{")
                    .Replace("{ ", "{")
                    .Replace(" }", "}")
                    .Replace("; ", ";")
                    .Replace(": ", ":")
                    .Replace(", ", ",")
                    .Trim();

                // Remove comments
                while (minified.Contains("/*"))
                {
                    var start = minified.IndexOf("/*");
                    var end = minified.IndexOf("*/", start);
                    if (end > start)
                    {
                        minified = minified.Remove(start, end - start + 2);
                    }
                    else
                    {
                        break;
                    }
                }

                return minified;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error minifying CSS");
                return css; // Return original CSS on error
            }
        }

        public async Task<string> MinifyJavaScriptAsync(string javascript)
        {
            try
            {
                if (string.IsNullOrEmpty(javascript))
                    return javascript;

                await Task.CompletedTask; // For async consistency

                // Basic JavaScript minification (very simple)
                // In production, you'd want to use a proper JS minifier like Terser
                var minified = javascript
                    .Replace("\r\n", " ")
                    .Replace("\n", " ")
                    .Replace("\t", " ")
                    .Replace("  ", " ")
                    .Trim();

                // Remove single-line comments (basic implementation)
                var lines = minified.Split(' ');
                var filteredLines = lines.Where(line => !line.TrimStart().StartsWith("//")).ToArray();
                minified = string.Join(" ", filteredLines);

                return minified;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error minifying JavaScript");
                return javascript; // Return original JavaScript on error
            }
        }

        public HtmlString GeneratePreloadLinks(List<string> criticalResources)
        {
            try
            {
                if (criticalResources == null || !criticalResources.Any())
                    return new HtmlString(string.Empty);

                var sb = new StringBuilder();

                foreach (var resource in criticalResources)
                {
                    var resourceType = GetResourceType(resource);
                    var crossorigin = resourceType == "font" ? " crossorigin" : "";

                    sb.AppendLine($"<link rel=\"preload\" href=\"{resource}\" as=\"{resourceType}\"{crossorigin}>");
                }

                return new HtmlString(sb.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating preload links");
                return new HtmlString(string.Empty);
            }
        }

        public HtmlString GenerateCriticalCss(string css)
        {
            try
            {
                if (string.IsNullOrEmpty(css))
                    return new HtmlString(string.Empty);

                // Extract critical CSS (above-the-fold styles)
                // This is a simplified implementation - in production, you'd use tools like Critical or Penthouse
                var criticalSelectors = new[]
                {
                    "body", "html", "h1", "h2", "h3", ".hero", ".navbar", ".container",
                    ".btn", ".card", ".showcase", ".header", ".main", ".footer"
                };

                var criticalCss = new StringBuilder();
                var lines = css.Split('\n');
                var inCriticalRule = false;
                var braceCount = 0;

                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();

                    if (!inCriticalRule)
                    {
                        // Check if this line starts a critical rule
                        if (criticalSelectors.Any(selector => trimmedLine.Contains(selector)))
                        {
                            inCriticalRule = true;
                            criticalCss.AppendLine(line);
                            braceCount = line.Count(c => c == '{') - line.Count(c => c == '}');
                        }
                    }
                    else
                    {
                        criticalCss.AppendLine(line);
                        braceCount += line.Count(c => c == '{') - line.Count(c => c == '}');

                        if (braceCount <= 0)
                        {
                            inCriticalRule = false;
                            braceCount = 0;
                        }
                    }
                }

                var minifiedCriticalCss = MinifyCssAsync(criticalCss.ToString()).Result;

                return new HtmlString($"<style>{minifiedCriticalCss}</style>");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating critical CSS");
                return new HtmlString(string.Empty);
            }
        }

        public async Task OptimizeStaticAssetsAsync()
        {
            try
            {
                _logger.LogInformation("Starting static asset optimization...");

                var wwwrootPath = Path.Combine(_environment.ContentRootPath, "Presentation", "wwwroot");

                if (!Directory.Exists(wwwrootPath))
                {
                    _logger.LogWarning("wwwroot directory not found: {Path}", wwwrootPath);
                    return;
                }

                // Optimize CSS files
                await OptimizeCssFilesAsync(wwwrootPath);

                // Optimize JavaScript files
                await OptimizeJavaScriptFilesAsync(wwwrootPath);

                // Generate asset manifest for cache busting
                await GenerateAssetManifestAsync(wwwrootPath);

                _logger.LogInformation("Static asset optimization completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error optimizing static assets");
            }
        }

        private async Task OptimizeCssFilesAsync(string wwwrootPath)
        {
            try
            {
                var cssPath = Path.Combine(wwwrootPath, "css");
                if (!Directory.Exists(cssPath))
                    return;

                var cssFiles = Directory.GetFiles(cssPath, "*.css", SearchOption.AllDirectories)
                    .Where(f => !f.EndsWith(".min.css"));

                foreach (var cssFile in cssFiles)
                {
                    var content = await File.ReadAllTextAsync(cssFile);
                    var minified = await MinifyCssAsync(content);

                    var minifiedPath = cssFile.Replace(".css", ".min.css");
                    await File.WriteAllTextAsync(minifiedPath, minified);

                    _logger.LogDebug("Minified CSS: {File}", Path.GetFileName(cssFile));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error optimizing CSS files");
            }
        }

        private async Task OptimizeJavaScriptFilesAsync(string wwwrootPath)
        {
            try
            {
                var jsPath = Path.Combine(wwwrootPath, "js");
                if (!Directory.Exists(jsPath))
                    return;

                var jsFiles = Directory.GetFiles(jsPath, "*.js", SearchOption.AllDirectories)
                    .Where(f => !f.EndsWith(".min.js"));

                foreach (var jsFile in jsFiles)
                {
                    var content = await File.ReadAllTextAsync(jsFile);
                    var minified = await MinifyJavaScriptAsync(content);

                    var minifiedPath = jsFile.Replace(".js", ".min.js");
                    await File.WriteAllTextAsync(minifiedPath, minified);

                    _logger.LogDebug("Minified JavaScript: {File}", Path.GetFileName(jsFile));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error optimizing JavaScript files");
            }
        }

        private async Task GenerateAssetManifestAsync(string wwwrootPath)
        {
            try
            {
                var manifest = new Dictionary<string, object>();
                var assets = new List<object>();

                // Scan for CSS and JS files
                var cssFiles = Directory.GetFiles(Path.Combine(wwwrootPath, "css"), "*.css", SearchOption.AllDirectories);
                var jsFiles = Directory.GetFiles(Path.Combine(wwwrootPath, "js"), "*.js", SearchOption.AllDirectories);

                foreach (var file in cssFiles.Concat(jsFiles))
                {
                    var relativePath = Path.GetRelativePath(wwwrootPath, file).Replace('\\', '/');
                    var fileInfo = new FileInfo(file);

                    assets.Add(new
                    {
                        path = "/" + relativePath,
                        size = fileInfo.Length,
                        lastModified = fileInfo.LastWriteTimeUtc,
                        hash = GetFileHash(file)
                    });
                }

                manifest["assets"] = assets;
                manifest["generatedAt"] = DateTime.UtcNow;

                var manifestJson = JsonSerializer.Serialize(manifest, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                var manifestPath = Path.Combine(wwwrootPath, "asset-manifest.json");
                await File.WriteAllTextAsync(manifestPath, manifestJson);

                _logger.LogInformation("Generated asset manifest with {AssetCount} assets", assets.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating asset manifest");
            }
        }

        private string GetResourceType(string resource)
        {
            var extension = Path.GetExtension(resource).ToLower();
            return extension switch
            {
                ".css" => "style",
                ".js" => "script",
                ".woff" or ".woff2" or ".ttf" or ".otf" => "font",
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" => "image",
                _ => "fetch"
            };
        }

        private string GetFileHash(string filePath)
        {
            try
            {
                using var sha256 = SHA256.Create();
                using var stream = File.OpenRead(filePath);
                var hash = sha256.ComputeHash(stream);
                return Convert.ToHexString(hash)[..8]; // First 8 characters
            }
            catch
            {
                return DateTime.UtcNow.Ticks.ToString()[^8..]; // Fallback to timestamp
            }
        }
    }
}
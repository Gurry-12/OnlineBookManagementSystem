namespace OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure
{
    /// <summary>
    /// Service for optimizing web assets (images, CSS, JS)
    /// </summary>
    public interface IAssetOptimizationService
    {
        /// <summary>
        /// Optimizes an image for web delivery
        /// </summary>
        Task<byte[]> OptimizeImageAsync(byte[] imageData, string format, int maxWidth = 1200, int quality = 85);

        /// <summary>
        /// Generates responsive image sizes
        /// </summary>
        Task<Dictionary<string, byte[]>> GenerateResponsiveImagesAsync(byte[] imageData, string format);

        /// <summary>
        /// Minifies CSS content
        /// </summary>
        string MinifyCss(string cssContent);

        /// <summary>
        /// Minifies JavaScript content
        /// </summary>
        string MinifyJavaScript(string jsContent);

        /// <summary>
        /// Compresses content using gzip
        /// </summary>
        byte[] CompressContent(byte[] content);

        /// <summary>
        /// Generates cache-busting hash for asset
        /// </summary>
        string GenerateAssetHash(byte[] content);

        /// <summary>
        /// Checks if asset should be served from cache
        /// </summary>
        bool ShouldServeFromCache(string assetPath, DateTime lastModified);
    }
}
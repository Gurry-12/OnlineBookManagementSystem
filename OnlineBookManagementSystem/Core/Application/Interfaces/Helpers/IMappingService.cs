namespace OnlineBookManagementSystem.Core.Application.Interfaces.Helpers
{
    /// <summary>
    /// Central mapping service interface for consistent object mapping across the application
    /// </summary>
    public interface IMappingService
    {
        /// <summary>
        /// Maps source object to destination type
        /// </summary>
        /// <typeparam name="TDestination">The destination type</typeparam>
        /// <param name="source">The source object</param>
        /// <returns>Mapped destination object</returns>
        TDestination Map<TDestination>(object source);

        /// <summary>
        /// Maps source object to destination type
        /// </summary>
        /// <typeparam name="TSource">The source type</typeparam>
        /// <typeparam name="TDestination">The destination type</typeparam>
        /// <param name="source">The source object</param>
        /// <returns>Mapped destination object</returns>
        TDestination Map<TSource, TDestination>(TSource source);

        /// <summary>
        /// Maps collection of source objects to destination type collection
        /// </summary>
        /// <typeparam name="TSource">The source type</typeparam>
        /// <typeparam name="TDestination">The destination type</typeparam>
        /// <param name="source">The source collection</param>
        /// <returns>Mapped destination collection</returns>
        IEnumerable<TDestination> Map<TSource, TDestination>(IEnumerable<TSource> source);

        /// <summary>
        /// Updates existing destination object with values from source object
        /// </summary>
        /// <typeparam name="TSource">The source type</typeparam>
        /// <typeparam name="TDestination">The destination type</typeparam>
        /// <param name="source">The source object</param>
        /// <param name="destination">The destination object to update</param>
        void Map<TSource, TDestination>(TSource source, TDestination destination);
    }
}
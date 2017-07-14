// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IStorage.cs" company="">
//   
// </copyright>
// <summary>
//   The Storage interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Platform.Utils.Owin.Storage
{
    /// <summary>
    /// The Storage interface.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public interface IStorage<T>
    {
        /// <summary>
        ///     Data storage indexer
        /// </summary>
        /// <param name="key">Stored object name</param>
        /// <returns>Object value form storage , null - otherwise</returns>
        T this[object key] { get; set; }
    }
}

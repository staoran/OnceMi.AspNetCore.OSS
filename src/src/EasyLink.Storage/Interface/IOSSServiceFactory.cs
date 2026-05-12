namespace EasyLink.Storage
{
    /// <summary>
    /// Creates object storage services from configured named options.
    /// </summary>
    public interface IOSSServiceFactory
    {
        /// <summary>
        /// Creates the default object storage service.
        /// </summary>
        /// <returns>The default object storage service.</returns>
        IOSSService Create();

        /// <summary>
        /// Creates a named object storage service.
        /// </summary>
        /// <param name="name">The configured service name.</param>
        /// <returns>The named object storage service.</returns>
        IOSSService Create(string name);
    }
}

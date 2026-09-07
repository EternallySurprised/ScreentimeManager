namespace ScreentimeManagerCore.Interfaces
{
    /// <summary>
    /// An interface representing a service that can perform remote shutdown operations on a host machine.
    /// </summary>
    public interface IRemoteShutdownService
    {
        /// <summary>
        /// Shuts down a remote host using the configured host and username.
        /// </summary>
        /// <param name="comment">Shutdown comment</param>
        void ShutdownHost(string comment);

        /// <summary>
        /// Shuts down a remote host. The password is expected to be set in the environment variable "PASSWD".
        /// </summary>
        /// <param name="host">Host to shutdown</param>
        /// <param name="username">Username to use for remote shutdown</param>
        /// <param name="comment">Shutdown comment</param>
        /// <exception cref="Exception">Thrown if shutdown fails</exception>
        void ShutdownHost(string host, string username, string comment);
    }
}
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ScreentimeManagerCore.Configuration;
using ScreentimeManagerCore.Interfaces;
using System.Diagnostics;

namespace ScreentimeManagerCore.Services
{
    public class NetRemoteShutdownService : IRemoteShutdownService
    {
        protected readonly ILogger<NetRemoteShutdownService> _logger;
        protected IOptions<RemoteShutdownConfiguration> _config;

        public NetRemoteShutdownService(ILogger<NetRemoteShutdownService> logger, IOptions<RemoteShutdownConfiguration> config)
        {
            _logger = logger;
            _config = config;
        }

        /// <summary>
        /// Shuts down a remote host using the configured host and username. The password is expected to be set in the environment variable "PASSWD".
        /// </summary>
        /// <param name="comment">Shutdown comment</param>
        public virtual void ShutdownHost(string comment = "Shutdown initiated.")
        {
            string host = _config.Value.Host ?? "localhost";
            string username = _config.Value.Username ?? "Administrator";
            ShutdownHost(host, username, comment);
        }

        /// <summary>
        /// Shuts down a remote host. The password is expected to be set in the environment variable "PASSWD".
        /// </summary>
        /// <param name="host">Host to shutdown</param>
        /// <param name="username">Username to use for remote shutdown</param>
        /// <param name="comment">Shutdown comment</param>
        /// <exception cref="Exception">Thrown if shutdown fails</exception>
        public virtual void ShutdownHost(string host, string username, string comment)
        {
            // Note: The password is not used in this command because the 'net rpc shutdown' command will get it from the environment variable "PASSWD"
            string cmdRes;
            bool result = RunCommand("net", $"rpc shutdown -S {host} -U {username} -f -C {comment}", out cmdRes);

            if (result)
            {
                _logger.LogDebug($"Successfully sent shutdown command to host {host}. Command output: {cmdRes}");
                return;
            }
            else
            {
                string msg = $"Failed to shutdown host {host}. Command output: {cmdRes}";
                _logger?.LogError(msg);
                throw new Exception(msg);
            }
        }

        /// <summary>
        /// Executes a system command
        /// </summary>
        /// <param name="command">Name of the command</param>
        /// <param name="args">Command parameters/arguments</param>
        /// <param name="output">Output of the command as string</param>
        /// <returns>True if the command was executed successully, otherwise false</returns>
        protected virtual bool RunCommand(string command, string args, out string output)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (string.IsNullOrEmpty(error))
            {
                output = result;
                return true;
            }
            else
            {
                output = error;
                return false;
            }
        }
    }
}

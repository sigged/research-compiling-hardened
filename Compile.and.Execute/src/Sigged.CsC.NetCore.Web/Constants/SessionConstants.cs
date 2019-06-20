namespace Sigged.CsC.NetCore.Web.Constants
{
    public static class SessionConstants
    {
        /// <summary>
        /// Interval for the clean up job
        /// </summary>
        public const int SessionCleanupJobInterval = 5; //seconds

        /// <summary>
        /// Time allowed for a session to remain idle
        /// <para>Must be greater than the <see cref="SessionCleanupJobInterval"/> value</para>
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        public const int SessionIdleTimeout = 30; //seconds

        /// <summary>
        /// Time allowed for a docker command to be executed gracefully
        /// </summary>
        public const int DockerCommandIdleTimeout = 30; //seconds
    }
}

namespace Sigged.CsCNetCore.Web.Constants
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
        public const int SessionIdleTimeout = 10; //seconds
    }
}

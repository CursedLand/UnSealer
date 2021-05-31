namespace UnSealer.Core
{
    public interface ILogger
    {
        /// <summary>
        /// Debug Level Message.
        /// </summary>
        /// <param name="m">Message.</param>
        void Debug(string m);
        /// <summary>
        /// Debug Level Message In Format.
        /// </summary>
        /// <param name="m">Message.</param>
        /// <param name="f">Formats.</param>
        void DebugFormat(string m, params object[] f);
        /// <summary>
        /// Error Level Message.
        /// </summary>
        /// <param name="m">Message.</param>
        void Error(string m);
        /// <summary>
        /// Error Level Message In Format.
        /// </summary>
        /// <param name="m">Message.</param>
        /// <param name="f">Formats.</param>
        void ErrorFormat(string m, params object[] f);
        /// <summary>
        /// Warn Level Message.
        /// </summary>
        /// <param name="m">Message.</param>
        void Warn(string m);
        /// <summary>
        /// Warn Level Message In Format.
        /// </summary>
        /// <param name="m">Message.</param>
        /// <param name="f">Formats.</param>
        void WarnFormat(string m, params object[] f);
        /// <summary>
        /// Info Level Message.
        /// </summary>
        /// <param name="m">Message.</param>
        void Info(string m);
        /// <summary>
        /// Info Level Message In Format.
        /// </summary>
        /// <param name="m">Message.</param>
        /// <param name="f">Formats.</param>
        void InfoFormat(string m, params object[] f);
    }
}
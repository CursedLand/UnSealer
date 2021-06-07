namespace UnSealer.Core {
	public interface ILogger {
		/// <summary>
		/// Normal Debug Message.
		/// </summary>
		/// <param name="m">Message.</param>
		void Debug(string m);
		/// <summary>
		/// Debug Formatted Message.
		/// </summary>
		/// <param name="m">Message.</param>
		/// <param name="f">Format Params.</param>
		void DebugFormat(string m, params object[] f);
		/// <summary>
		/// Normal Error Message.
		/// </summary>
		/// <param name="m">Message.</param>
		void Error(string m);
		/// <summary>
		/// Error Formatted Message.
		/// </summary>
		/// <param name="m">Message.</param>
		/// <param name="f">Format Params.</param>
		void ErrorFormat(string m, params object[] f);
		/// <summary>
		/// Normal Warn Message.
		/// </summary>
		/// <param name="m">Message.</param>
		void Warn(string m);
		/// <summary>
		/// Warn Formatted Message.
		/// </summary>
		/// <param name="m">Message.</param>
		/// <param name="f">Format Params.</param>
		void WarnFormat(string m, params object[] f);
		/// <summary>
		/// Normal Info Message.
		/// </summary>
		/// <param name="m">Message.</param>
		void Info(string m);
		/// <summary>
		/// Info Formatted Message.
		/// </summary>
		/// <param name="m">Message.</param>
		/// <param name="f">Format Params.</param>
		void InfoFormat(string m, params object[] f);
	}
}
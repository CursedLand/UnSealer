
#region Usings
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using UnSealer.Core;
#endregion

namespace UnSealer.CLI {
	public class ConsoleLogger : Core.ILogger {
		private readonly Logger _logger = new LoggerConfiguration().WriteTo.Console(LogEventLevel.Verbose, "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}", null, null, null, AnsiConsoleTheme.Literate).CreateLogger();

		public void Debug(string m)
			=> _logger.Debug(m);
		public void DebugFormat(string m, params object[] f)
			=> _logger.Debug(m, f);
		public void Error(string m)
			=> _logger.Error(m);
		public void ErrorFormat(string m, params object[] f)
			=> _logger.Error(m, f);
		public void Info(string m)
			=> _logger.Information(m);
		public void InfoFormat(string m, params object[] f)
			=> _logger.Information(m, f);
		public void Warn(string m)
			=> _logger.Warning(m);
		public void WarnFormat(string m, params object[] f)
			=> _logger.Warning(m, f);
	}
}
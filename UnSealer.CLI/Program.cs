
#region Usings
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using static UnSealer.Core.UnSealerEngine;
using UnSealer.Core;
#endregion

namespace UnSealer.CLI {
    internal class Program {

		private static ILogger ConsoleLogger = new ConsoleLogger();
		private static IList<Protection> Protections;

        [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        static void Main(string[] args) {
			Console.Clear();
			Console.Title = "UnSealer - v" + UnSealerVersion;
			Console.SetWindowSize(83, 33);
			Console.SetBufferSize(83, 9001);
			Banner();
			Protections = PluginDiscovery.GetCurrentDirPlugins(ConsoleLogger);
			if (args.Length <= 0) {
				Console.Write("[~] Enter Arguments : ");
				var pargs = Console.ReadLine()!.Replace("\"", string.Empty).Split(' ');
				Console.Clear();
				Banner();
				var ParsedArgs = new ArgumentsParser(Protections, pargs).Result;
				ExecuteEngine(ParsedArgs, ConsoleLogger);
			}
			else {
				var ArgsParsed = new ArgumentsParser(Protections, args).Result;
				ExecuteEngine(ArgsParsed, ConsoleLogger);
			}
		}
        internal static void Banner() {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("\r\n\r\n                          __  __     ____         __       \r\n                         / / / /__  / __/__ ___ _/ /__ ____\r\n                        / /_/ / _ \\_\\ \\/ -_) _ `/ / -_) __/\r\n                        \\____/_//_/___/\\__/\\_,_/_/\\__/_/   \r\n\r\n                                   ");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
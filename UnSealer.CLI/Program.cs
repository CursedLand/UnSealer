
#region Usings
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using static UnSealer.Core.UnSealerEngine;
using UnSealer.Core;
using System.Diagnostics;
#endregion

namespace UnSealer.CLI
{
	internal class Program
	{

		private static ILogger ConsoleLogger = new ConsoleLogger();
		private static IList<Protection> Protections;

		[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
		static void Main(string[] args)
		{
			Console.Clear();
			Console.Title = "UnSealer - v" + UnSealerVersion;
			Console.SetWindowSize(83, 33);
			Console.SetBufferSize(83, 9001);

			Protections = PluginDiscovery.GetCurrentDirPlugins(ConsoleLogger);

			Banner();
			PrintUsage(Protections);

			if (args.Length <= 0)
			{
				Console.Write("[~] Enter Arguments : ");
				var pargs = Console.ReadLine()!.Replace("\"", string.Empty).Split(' ');
				Console.Clear();
				Banner();
				var ParsedArgs = new ArgumentsParser(Protections, pargs).Result;
				ExecuteEngine(ParsedArgs, ConsoleLogger);
			}
			else
			{
				var ArgsParsed = new ArgumentsParser(Protections, args).Result;
				ExecuteEngine(ArgsParsed, ConsoleLogger);
			}
		}

		/// <summary>
		/// Print usage and loaded plugin's information
		/// </summary>
		/// <param name="protections">Loaded plugins</param>
		internal static void PrintUsage(IList<Protection> protections)
		{
			if (protections == null || protections.Count <= 0)
			{
				Console.ForegroundColor = ConsoleColor.DarkYellow;
				Console.WriteLine($"No plugins found ！{Environment.NewLine}");
				Console.ForegroundColor = ConsoleColor.White;
				return;
			};

			var processName = Process.GetCurrentProcess().ProcessName;

			Console.ForegroundColor = ConsoleColor.Magenta;
			Console.WriteLine("Usage:");
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine($"{processName}.exe [your assembly full path] -{protections[0].Id}{Environment.NewLine}");

			Console.ForegroundColor = ConsoleColor.Magenta;
			Console.WriteLine("Plugin Loaded:");
			Console.ForegroundColor = ConsoleColor.Green;

			foreach (var protection in protections)
			{
				Console.WriteLine($"Option: -{protection.Id}");
				Console.WriteLine($"\t{protection.Name} : {protection.Description} {Environment.NewLine}");
			}

			Console.ForegroundColor = ConsoleColor.White;
		}

		internal static void Banner()
		{
			Console.ForegroundColor = ConsoleColor.Blue;
			Console.WriteLine("\r\n\r\n                          __  __     ____         __       \r\n                         / / / /__  / __/__ ___ _/ /__ ____\r\n                        / /_/ / _ \\_\\ \\/ -_) _ `/ / -_) __/\r\n                        \\____/_//_/___/\\__/\\_,_/_/\\__/_/   \r\n\r\n                                   ");
			Console.ForegroundColor = ConsoleColor.White;
		}
	}
}
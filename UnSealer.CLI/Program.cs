
#region Usings
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnSealer.Core;
using static UnSealer.Core.UnSealerEngine;
#endregion

namespace UnSealer.CLI
{
    internal class Program
    {
        private static ILogger ConsoleLogger = new ConsoleLogger();
        private static IList<Protection> Protections;

        [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        private static void Main(string[] args)
        {
            Console.Clear();
            Console.Title = $"UnSealer - v{UnSealerVersion}";
            Console.SetWindowSize(83, 33); // Interoperability on Linux / Mac.
            Console.SetBufferSize(83, 9001); // Interoperability on Linux / Mac.
            Banner();
            Protections = PluginDiscovery.GetCurrentDirPlugins(ConsoleLogger);
            if (args.Length <= 0) {
                Console.Write("[~] Enter Arguments : ");
                var newargs = Console.ReadLine().Replace("\"", string.Empty).Split(' ');
                Console.Clear();
                Banner();
                var Args = new ArgumentsParser(Protections, newargs).Result;
                ExecuteEngine(Args, ConsoleLogger);
            }
            else {
                var Args = new ArgumentsParser(Protections, args).Result;
                ExecuteEngine(Args, ConsoleLogger);
            }
        }
        private static void Banner()
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(@"

                          __  __     ____         __       
                         / / / /__  / __/__ ___ _/ /__ ____
                        / /_/ / _ \_\ \/ -_) _ `/ / -_) __/
                        \____/_//_/___/\__/\_,_/_/\__/_/   

                                   ");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
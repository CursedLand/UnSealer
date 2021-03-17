using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace UnSealer.Core.PluginDiscovery
{
    public class LoadPlugins
    {
        /// <summary>
        /// Discovering Dlls / Protections / Plugins Used :D
        /// </summary>
        /// <param name="Logger"> Logger. </param>
        /// <param name="Protections"> List Of Protections To Be Executed :) </param>
        public void Discover(Logger Logger, ref IList<Protection> Protections)
        {
            IList<Assembly> Assemblies = new List<Assembly>();
            foreach (var name in Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*.dll"))
            {
                if (name.Contains("dnlib") | name.Contains("AsmResolver") | name.Contains("UnSealer.Core") | name.Contains("HandyControl") | name.Contains("de4dot")) continue;
                try
                {
                    var Ass = Assembly.UnsafeLoadFrom(name);
                    Assemblies.Add(Ass);
                    Logger.Debug($"Loaded Plugin : {Ass.ManifestModule.Name}");
                }
                catch
                {
                    // Ignore :D
                }
            }
            foreach (var Ass in Assemblies)
                foreach (var Type in Ass.GetTypes().Where(x => !x.IsAbstract && typeof(Protection).IsAssignableFrom(x)))
                {
                    var DisPro = (Protection)Activator.CreateInstance(Type);
                    Protections.Add(DisPro);                    // Enum.GetName(...) Cuz Some Errors maybe happen.
                    Logger.Debug($"Found : {DisPro.Name} Lib : {Enum.GetName(typeof(ProtectionType), DisPro.Type)} By {DisPro.Author}");
                }
        }
    }
}

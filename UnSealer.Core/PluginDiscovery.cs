
#region Usings
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
#endregion

namespace UnSealer.Core
{
    public static class PluginDiscovery
    {
        public static IList<Protection> GetCurrentDirPlugins(ILogger Logger) {
            var _plugins = new List<Assembly>();
            foreach (var Location in Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*.dll")) {
                if (Location.Contains("AsmResolver") || Location.Contains("Echo"))
                    continue;
                try {
                    Assembly Ass = Assembly.UnsafeLoadFrom(Location);
                    _plugins.Add(Ass);
                    Logger.DebugFormat("Plugin Loaded {0}.", Ass.ManifestModule.Name);
                }
                catch {

                }
            }
            var _protections = new List<Protection>();
            foreach (var _plugin in _plugins) {
                foreach (var _protection in _plugin.GetTypes().Where(_type => !_type.IsAbstract && typeof(Protection).IsAssignableFrom(_type))) {
                    Protection _protect = Activator.CreateInstance(_protection) as Protection;
                    _protections.Add(_protect);
                }
            }
            return _protections;
        }
    }
}

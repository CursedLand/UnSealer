
#region Usings
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace UnSealer.Core {
	public static class PluginDiscovery {
		public static IList<Protection> GetCurrentDirPlugins(ILogger Logger)
		{
			var PluginsAsm = new List<Assembly>();
			var files = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*.dll");
			foreach (string path in files.Where(x => !x.Contains("AsmResolver") && !x.Contains("Echo"))) /* Linq is Real Love. */ {
				try {
					var Plugin = Assembly.UnsafeLoadFrom(path);
					PluginsAsm.Add(Plugin);
					Logger.DebugFormat("Plugin Loaded {0}.",
						Plugin.ManifestModule.Name);
				}
				catch { }
			}
			var LoadedProtections = new List<Protection>();
			foreach (var Plugin in PluginsAsm) {
				foreach (var Type in Plugin.GetTypes().Where(_type => !_type.IsAbstract && typeof(Protection).IsAssignableFrom(_type))) {
					LoadedProtections.Add(Activator.CreateInstance(Type) as Protection);
				}
			}
			return LoadedProtections;
		}
	}
}


#region Usings
using System;
using System.IO;
using System.Reflection;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Builder;
#endregion

namespace UnSealer.Core {
	public class Context {
		/// <summary>
		/// Initialization Of Context.
		/// </summary>
		/// <param name="path">Module Path.</param>
		/// <param name="logger">Logger Used In Context.</param>
		public Context(string path, ILogger logger) {
			ModulePath = path;
			Logger = logger;
			try {
				Module = ModuleDefinition.FromFile(ModulePath, new(Path.GetDirectoryName(ModulePath)));
				Factory = new();
				ImageBuilder = new(Factory);
				Importer = new(Module);
				Mscorlib = Module.CorLibTypeFactory.CorLibScope.GetAssembly().Resolve().ManifestModule;
			}
			catch (BadImageFormatException) {
				Logger.ErrorFormat("{0} Is Not .NET File!", Path.GetFileNameWithoutExtension(ModulePath));
			}
			catch (Exception e) {
				Logger.ErrorFormat("Error Happened While Loading Module : {0}", e.Message);
			}
			try {
				var Stream = new MemoryStream();
				var TempModule = ModuleDefinition.FromFile(ModulePath);
				TempModule.IsILOnly = true;
				TempModule.Write(Stream, new ManagedPEImageBuilder((MetadataBuilderFlags)0x1FFFF));
				ReflectionCorlib = Assembly.Load(Stream.ToArray()).ManifestModule;
			}
			catch {
				Logger.Warn("Corlib Reflection Module Can't Load.");
			}
			try {
				ReflectionModule = Assembly.UnsafeLoadFrom(ModulePath).ManifestModule;
			}
			catch {
				Logger.Warn("Reflection Module Can't Load.");
			}
		}
		public ReferenceImporter Importer { get; }
		public bool IsReflectionSafe => ReflectionModule != null;
		public bool IsReflectionCorlibSafe => ReflectionCorlib != null;
		public DotNetDirectoryFactory Factory { get; }
		public ManagedPEImageBuilder ImageBuilder { get; }
		public Pipeline Pipeline { get; set; }
		public ModuleDefinition Module { get; set; }
		public ModuleDefinition Mscorlib { get; }
		public Module ReflectionModule { get; }
		public Module ReflectionCorlib { get; }
		public string ModulePath { get; }
		public ILogger Logger { get; set; }
	}
}
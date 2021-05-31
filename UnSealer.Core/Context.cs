
#region Usings
using AsmResolver.DotNet;
using AsmResolver.DotNet.Builder;
using AsmResolver.DotNet.Serialized;
using System;
using System.IO;
using System.Reflection;
#endregion

namespace UnSealer.Core
{
    public class Context
    {
        public Context(string path, ILogger logger)
        {
            ModulePath = path;
            Logger = logger;
            try
            {
                Module = ModuleDefinition.FromFile(ModulePath,
                    new ModuleReaderParameters(Path.GetDirectoryName(ModulePath)));
                Factory = new();
                ImageBuilder = new(Factory);
                Importer = new(Module);
                Mscorlib = Module.CorLibTypeFactory.CorLibScope.GetAssembly().Resolve().ManifestModule;
            }
            catch (BadImageFormatException)
            {
                Logger.ErrorFormat("{0} Is Not .NET File!", Path.GetFileNameWithoutExtension(ModulePath));
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("Error Happened While Loading Module : {0}", e.Message);
            }
            try
            {
                MemoryStream TempStream = new MemoryStream();
                ModuleDefinition TempModule = ModuleDefinition.FromFile(ModulePath);
                TempModule.IsILOnly = true;
                TempModule.Write(TempStream, new ManagedPEImageBuilder(MetadataBuilderFlags.PreserveAll));
                ReflectionCorlib = Assembly.Load(TempStream.ToArray()).ManifestModule;
            }
            catch
            {
                logger.Warn("Corlib Reflection Module Can't Load.");
            }
            try
            {
                ReflectionModule = Assembly.UnsafeLoadFrom(ModulePath).ManifestModule;
            }
            catch
            {
                logger.Warn("Reflection Module Can't Load.");
            }
        }
        public ReferenceImporter Importer { get; }
        public bool IsReflectionSafe => ReflectionModule != null;
        public bool IsReflectionCorlibSafe => ReflectionCorlib != null;
        public DotNetDirectoryFactory Factory { get; }
        public ManagedPEImageBuilder ImageBuilder { get; }
        public Pipeline Pipeline { set; get; }
        public ModuleDefinition Module { set; get; }
        public ModuleDefinition Mscorlib { get; }
        public Module ReflectionModule { get; }
        public Module ReflectionCorlib { get; }
        public string ModulePath { get; }
        public ILogger Logger { set; get; }
    }
}
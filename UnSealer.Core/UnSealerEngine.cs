
#region Usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#endregion

namespace UnSealer.Core {
	public static class UnSealerEngine {

		public static readonly string UnSealerVersion = ((AssemblyFileVersionAttribute)typeof(UnSealerEngine).Assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), inherit: false)[0]).Version;

		public static void ExecuteEngine(ArgumentsParserResult args, ILogger logger) {
			IList<Protection> protections = args.Protections;
			Context context = new Context(args.Path, logger) {
				Pipeline = new()
			};
			context.Logger.InfoFormat("Loaded {0} Protections", protections.Count);
			context.Logger.InfoFormat("UnSealer v{0} (C) CLand - 2021", UnSealerVersion);
			context.Logger.Info("Constructing Pipeline ...");
			foreach (var Protection in protections) {
				context.Logger.InfoFormat("Executing {0} Protection", Protection.Name);
				Protection.InitPipeline(context, context.Pipeline);
			}
			RunPipelinePhase(context);
		}

		private static void RunPipelinePhase(Context Context) {
			Context.Pipeline.ExecutePipeLineStage(PipelineStage.BeginModule,
				Context.Module.GetDefs(),
				new Action<Context>(BeginModule), Context);
			Context.Pipeline.ExecutePipeLineStage(PipelineStage.ProcessModule,
				Context.Module.GetDefs(),
				new Action<Context>(ProcessModule), Context);
			Context.Pipeline.ExecutePipeLineStage(PipelineStage.OptimizeModule,
				Context.Module.GetDefs(),
				new Action<Context>(OptimizeModule), Context);
			Context.Pipeline.ExecutePipeLineStage(PipelineStage.WriteModule,
				Context.Module.GetDefs(),
				new Action<Context>(WriteModule), Context);
		}

        #region PipelineStages
        private static void BeginModule(Context Context) {
			foreach (var Type in Context.Module.GetAllTypes())
				foreach (var Method in Type.Methods.Where(Method => Method.HasMethodBody))
					Method.CilMethodBody.Instructions.ExpandMacros();
		}
		private static void ProcessModule(Context Context) { }
		private static void OptimizeModule(Context Context) {
			foreach (var Type in Context.Module.GetAllTypes())
				foreach (var Method in Type.Methods.Where(Method => Method.HasMethodBody))
					Method.CilMethodBody.Instructions.OptimizeMacros();
		}
		private static void WriteModule(Context Context)
			=> Context.Module.Write(Context.ModulePath.Insert(Context.ModulePath.Length - 4, "UnSealed"), Context.ImageBuilder);
        #endregion
	}
}
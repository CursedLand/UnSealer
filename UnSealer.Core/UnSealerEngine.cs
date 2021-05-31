
#region Usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#endregion

namespace UnSealer.Core
{
    public static class UnSealerEngine
    {
        /// <summary>
        /// Engine Executer.
        /// </summary>
        /// <param name="args">Arguments</param>
        /// <param name="logger">Logger</param>
        public static void ExecuteEngine(ArgumentsParserResult args,
            ILogger logger)
        {
            IList<Protection> protections = args.Protections;
            Context Context = new Context(args.Path, logger)
            {
                Pipeline = new(),
            };
            /*Tests(Context);*/
            Context.Logger.InfoFormat("Loaded {0} Protections", protections.Count);
            Context.Logger.InfoFormat("UnSealer v{0} (C) CLand - 2021", UnSealerVersion);
            Context.Logger.Info("Constructing Pipeline ...");
            foreach (Protection _protection in protections)
            {
                Context.Logger.InfoFormat("Executing {0} Protection", _protection.Name);
                _protection.InitPipeline(Context, Context.Pipeline); // initialize pipelines.
            }
            RunPipelinePhase(Context);
        }
        /// <summary>
        /// Pipeline Runner.
        /// </summary>
        /// <param name="Context">Context.</param>
        private static void RunPipelinePhase(Context Context)
        {
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
        #region PipelineActions
        private static void BeginModule(Context Context)
        {
            foreach (AsmResolver.DotNet.TypeDefinition _type in Context.Module.GetAllTypes())
            {
                foreach (AsmResolver.DotNet.MethodDefinition _method in _type.Methods.Where(Method => Method.HasMethodBody))
                {
                    _method.CilMethodBody.Instructions.ExpandMacros();
                }
            }
        }
        private static void ProcessModule(Context Context) { }
        private static void OptimizeModule(Context Context)
        {
            foreach (AsmResolver.DotNet.TypeDefinition _type in Context.Module.GetAllTypes())
            {
                foreach (AsmResolver.DotNet.MethodDefinition _method in _type.Methods.Where(Method => Method.HasMethodBody))
                {
                    _method.CilMethodBody.Instructions.OptimizeMacros();
                }
            }
        }
        private static void WriteModule(Context Context)
        {
            Context.Module.Write(Context.ModulePath.Insert(Context.ModulePath.Length - 4, "UnSealed"),
                Context.ImageBuilder);
        }
        #endregion
        #region Fields
        public static readonly string AName = "CursedLand.Harmony";
        public static readonly string UnSealerVersion = ((AssemblyFileVersionAttribute)typeof(UnSealerEngine).Assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false)[0]).Version;
        #endregion
        #region _meh
        internal static void Tests(Context x)
        {
            /*var sb = new StringBuilder();
            sb.AppendLine("object MathResult = null;");
            foreach (var MathCall in x.GetFromCorlib("System", "Math").Resolve().Methods.Where(q => q.IsStatic && !q.IsConstructor && q.Signature.ReturnType != x.Module.CorLibTypeFactory.Void)) 
            {
                
                sb.AppendLine($"else if (MathCall.FullName == \"{MathCall.FullName}\")");
                sb.AppendLine("{");
                var xD = "";
                if (MathCall.Signature.GetTotalParameterCount() is 1)
                    xD = $"({MathCall.Parameters.First().ParameterType.Name})P2";
                else
                    xD = $"({MathCall.Parameters.First().ParameterType.Name})P,({MathCall.Parameters.Last().ParameterType.Name})P2";
                sb.AppendLine($"MathResult = Math.{MathCall.Name}({xD});");
                if (MathCall.Signature.GetTotalParameterCount() is 1)
                    sb.AppendLine(@"new[] {
                            Instructions[x-1],
                            Instructions[x-2],
                        }.Nop();");
                else
                    sb.AppendLine("Instructions[x-1].Nop();");
                sb.AppendLine("}");
            }
            System.IO.File.WriteAllText("Hi.txt", sb.ToString());*/
        }
        #endregion
    }
}

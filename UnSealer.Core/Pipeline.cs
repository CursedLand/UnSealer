
#region Usings
using System;
using System.Collections.Generic;
using AsmResolver.DotNet;
using UnSealer.Core;
#endregion

namespace UnSealer.Core {
	public class Pipeline {
        private Dictionary<PipelineStage, List<ProtectionPhase>> _prestages;
		private Dictionary<PipelineStage, List<ProtectionPhase>> _poststages;
		/// <summary>
		/// Initialization Of Pipeline.
		/// </summary>
		public Pipeline() {
			_prestages = new Dictionary<PipelineStage, List<ProtectionPhase>>();
			_poststages = new Dictionary<PipelineStage, List<ProtectionPhase>>();
			PipelineStage[] array = (PipelineStage[])Enum.GetValues(typeof(PipelineStage));
			foreach (PipelineStage key in array)
			{
				_prestages[key] = new List<ProtectionPhase>();
				_poststages[key] = new List<ProtectionPhase>();
			}
		}
		/// <summary>
		/// Insert Post ProtectionPhase(<paramref name="ProtectionPhase"/>) PipelineStage(<paramref name="Stage"/>).
		/// </summary>
		/// <param name="Stage">PipelineStage That Get Added.</param>
		/// <param name="ProtectionPhase">Phase Will Added Into Specified Stage.</param>
		public void InsertPostStage(PipelineStage Stage, ProtectionPhase ProtectionPhase) 
			=> _poststages[Stage].Add(ProtectionPhase);
		/// <summary>
		/// Insert Pre ProtectionPhase(<paramref name="ProtectionPhase"/>) PipelineStage(<paramref name="Stage"/>).
		/// </summary>
		/// <param name="Stage">PipelineStage That Get Added.</param>
		/// <param name="ProtectionPhase">Phase Will Added Into Specified Stage.</param>
		public void InsertPreStage(PipelineStage Stage, ProtectionPhase ProtectionPhase)
			=> _prestages[Stage].Add(ProtectionPhase);
		/// <summary>
		/// Insert Post ProtectionPhases(<paramref name="ProtectionsPhases"/>) PipelineStage(<paramref name="Stage"/>).
		/// </summary>
		/// <param name="Stage">PipelineStage That Get Added.</param>
		/// <param name="ProtectionsPhases">Phases Will Added Into Specified Stage.</param>
		public void InsertPostStage(PipelineStage Stage, IEnumerable<ProtectionPhase> ProtectionsPhases)
			=> _poststages[Stage].AddRange(ProtectionsPhases);
		/// <summary>
		/// Insert Post ProtectionPhases(<paramref name="ProtectionsPhases"/>) PipelineStage(<paramref name="Stage"/>).
		/// </summary>
		/// <param name="Stage">PipelineStage That Get Added.</param>
		/// <param name="ProtectionsPhases">Phases Will Added Into Specified Stage.</param>
		public void InsertPreStage(PipelineStage Stage, IEnumerable<ProtectionPhase> ProtectionsPhases)
			=> _prestages[Stage].AddRange(ProtectionsPhases);
		/// <summary>
		/// Execute All Phases Stored Into PipelineStage(<paramref name="Stage"/>).
		/// </summary>
		/// <param name="Stage">Stage That Get Executed.</param>
		/// <param name="Targets">MetadataMembers That Accessed In Phases.</param>
		/// <param name="Invoke">Action Happen Between PreStages And PostStages.</param>
		/// <param name="Context">Context.</param>
		public void ExecutePipeLineStage(PipelineStage Stage, IEnumerable<MetadataMember> Targets, Action<Context> Invoke, Context Context) {
			var PreStages = _prestages[Stage].ToArray();
			foreach (var Prephase in PreStages)
				Prephase.Execute(Context, Prephase.PhaseTargets.Purify(Targets));
			Invoke(Context);
			var PostStages = _poststages[Stage].ToArray();
			foreach (var PostPhase in PostStages)
				PostPhase.Execute(Context, PostPhase.PhaseTargets.Purify(Targets));
		}
	}
}
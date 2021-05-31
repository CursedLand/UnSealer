
#region Usings
using AsmResolver.DotNet;
using System;
using System.Collections.Generic;
#endregion

namespace UnSealer.Core
{
    public class Pipeline
    {
        /// <summary>
        /// Initializer Of PipeLine
        /// </summary>
        public Pipeline()
        {
            _prestages = new Dictionary<PipelineStage, List<ProtectionPhase>>();
            _poststages = new Dictionary<PipelineStage, List<ProtectionPhase>>();
            foreach (PipelineStage _pipestage in (PipelineStage[])Enum.GetValues(typeof(PipelineStage)))
            {
                _prestages[_pipestage] = new List<ProtectionPhase>();
                _poststages[_pipestage] = new List<ProtectionPhase>();
            }
        }
        /// <summary>
        /// Inserts Post Pipeline Stage.
        /// </summary>
        /// <param name="Stage">Targeted Stage.</param>
        /// <param name="ProtectionPhase">Phase Uses That Stage.</param>
        public void InsertPostStage(PipelineStage Stage, ProtectionPhase ProtectionPhase)
        {
            _poststages[Stage].Add(ProtectionPhase);
        }

        /// <summary>
        /// Inserts Pre Pipeline Stage.
        /// </summary>
        /// <param name="Stage">Targeted Stage.</param>
        /// <param name="ProtectionPhase">Phase Uses That Stage.</param>
        public void InsertPreStage(PipelineStage Stage, ProtectionPhase ProtectionPhase)
        {
            _prestages[Stage].Add(ProtectionPhase);
        }

        /// <summary>
        /// Inserts Post Pipeline Stages.
        /// </summary>
        /// <param name="Stage">Targeted Stage.</param>
        /// <param name="ProtectionsPhases">Phases Uses That Stage.</param>
        public void InsertPostStage(PipelineStage Stage, IEnumerable<ProtectionPhase> ProtectionsPhases)
        {
            _poststages[Stage].AddRange(ProtectionsPhases);
        }

        /// <summary>
        /// Inserts Pre Pipeline Stages.
        /// </summary>
        /// <param name="Stage">Targeted Stage.</param>
        /// <param name="ProtectionsPhases">Phases Uses That Stage.</param>
        public void InsertPreStage(PipelineStage Stage, IEnumerable<ProtectionPhase> ProtectionsPhases)
        {
            _prestages[Stage].AddRange(ProtectionsPhases);
        }

        /// <summary>
        /// Process Pipeline Stage.
        /// </summary>
        /// <param name="Stage">Requested Stage.</param>
        /// <param name="Targets">Current Targets.</param>
        /// <param name="Invoke">Process Happen Between PrePhases And PostPhases.</param>
        /// <param name="Context">Context.</param>
        public void ExecutePipeLineStage(PipelineStage Stage,
                                         IEnumerable<MetadataMember> Targets,
                                         Action<Context> Invoke,
                                         Context Context)
        {
            foreach (ProtectionPhase _prephase in _prestages[Stage].ToArray())
            {
                _prephase.Execute(Context, _prephase.PhaseTargets.Purify(Targets));
            }

            Invoke(Context);
            foreach (ProtectionPhase _postphase in _poststages[Stage].ToArray())
            {
                _postphase.Execute(Context, _postphase.PhaseTargets.Purify(Targets));
            }
        }
        #region Internal Fields
        private Dictionary<PipelineStage, List<ProtectionPhase>> _prestages, _poststages;
        #endregion
    }
}

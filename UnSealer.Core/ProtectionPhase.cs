
#region Usings
using AsmResolver.DotNet;
using System.Collections.Generic;
#endregion

namespace UnSealer.Core
{
    public abstract class ProtectionPhase
    {
        /// <summary>
        /// Initializer Of ProtectionPhase.
        /// </summary>
        /// <param name="ParentBase">Protection That Uses This Phase.</param>
        public ProtectionPhase(Protection ParentBase)
        {
            Base = ParentBase;
        }

        /// <summary>
        /// Protection Uses This Phase.
        /// </summary>
        public Protection Base { get; internal set; }
        /// <summary>
        /// Phase Name.
        /// </summary>
        public abstract string Name { get; }
        /// <summary>
        /// Targets Targeted By Phase.
        /// </summary>
        public abstract ProtectionTargets PhaseTargets { get; }
        /// <summary>
        /// Executing Code Of The Phase.
        /// </summary>
        /// <param name="Context">Context.</param>
        /// <param name="Targets">Definitions Of Current Module.</param>
        public abstract void Execute(Context context, IEnumerable<MetadataMember> targets);
    }
}

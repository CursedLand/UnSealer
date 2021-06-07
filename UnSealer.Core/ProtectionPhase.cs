
#region Usings
using AsmResolver.DotNet;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace UnSealer.Core {
	public abstract class ProtectionPhase {
		/// <summary>
		/// Base Protection Instance.
		/// </summary>
		public Protection Base { get; internal set; }
		/// <summary>
		/// Initialization Of ProtectionPhase.
		/// </summary>
		/// <param name="ParentBase">Protection Parent.</param>
		public ProtectionPhase(Protection ParentBase)
			=> Base = ParentBase;
		/// <summary>
		/// Phase Name.
		/// </summary>
		public abstract string Name { get; }
		/// <summary>
		/// Members Required For The Phase.
		/// </summary>
		public abstract ProtectionTargets PhaseTargets { get; }
		/// <summary>
		/// Execution Of The Phase.
		/// </summary>
		/// <param name="context">Context.</param>
		/// <param name="targets">Targeted Members.</param>
		public abstract void Execute(Context context, IEnumerable<MetadataMember> targets);
	}
}
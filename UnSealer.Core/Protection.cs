namespace UnSealer.Core {
	public abstract class Protection {
		/// <summary>
		/// Protection Name.
		/// </summary>
		public abstract string Name { get; }
		/// <summary>
		/// Protection Description.
		/// </summary>
		public abstract string Description { get; }
		/// <summary>
		/// Protection Id.
		/// </summary>
		public abstract string Id { get; }
		/// <summary>
		/// Author Of The Protection.
		/// </summary>
		public abstract string Author { get; }
		/// <summary>
		/// Pipeline Initialization.
		/// </summary>
		/// <param name="context">Context.</param>
		/// <param name="pipeline">Pipeline Instance.</param>
		public abstract void InitPipeline(Context context, Pipeline pipeline);
	}
}
namespace UnSealer.Core
{
    /// <summary>
    /// Protection Inheritance.
    /// </summary>
    public abstract class Protection
    {
        /// <summary>
        /// Pipeline Initializer.
        /// </summary>
        /// <param name="Pipeline">Pipeline.</param>
        public abstract void InitPipeline(Context context, Pipeline pipeline);
        /// <summary>
        /// Name Of Protection.
        /// </summary>
        public abstract string Name { get; }
        /// <summary>
        /// Description Of Protection.
        /// </summary>
        public abstract string Description { get; }
        /// <summary>
        /// Id Of Protection.
        /// </summary>
        public abstract string Id { get; }
        /// <summary>
        /// Protection Author.
        /// </summary>
        public abstract string Author { get; }
    }
}
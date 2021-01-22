namespace UnSealer.Core
{
    public abstract class Protection
    {
        /// <summary>
        /// Name Of Protection 
        /// </summary>
        public abstract string Name { get; }
        /// <summary>
        /// Name Of The Author
        /// </summary>
        public abstract string Author { get; }
        /// <summary>
        /// A Quite Description For Protection
        /// </summary>
        public abstract string Description { get; }
        /// <summary>
        /// Type Of Lib Used
        /// </summary>
        public abstract ProtectionType Type { get; }
        /// <summary>
        /// Executing Phase
        /// </summary>
        /// <param name="Context"> Context Which Have Good Settings For Starting :) </param>
        public abstract void Execute(Context Context);
    }
}
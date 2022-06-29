namespace UnSealer.Core {
    public enum PipelineStage {
        /// <summary>
        /// Expanding Module Stage.
        /// </summary>
        BeginModule,
        /// <summary>
        /// Processing Module Stage.
        /// </summary>
        ProcessModule,
        /// <summary>
        /// Optimizing Module Stage.
        /// </summary>
        OptimizeModule,
        /// <summary>
        /// Module Writing Stage.
        /// </summary>
        WriteModule
    }
}
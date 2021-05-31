
#region Usings
using AsmResolver.DotNet;
#endregion

namespace UnSealer.Protections.Devirtualizers.CawkVM
{
    public struct MethodInfo
    {
        public MethodInfo(MethodDefinition Method,
            int Position, int Size, int ID)
        {
            Parent = Method;
            this.Position = Position;
            this.Size = Size;
            this.ID = ID;
        }
        public MethodDefinition Parent { get; }
        public int Position { get; }
        public int Size { get; }
        public int ID { get; }
    }
}
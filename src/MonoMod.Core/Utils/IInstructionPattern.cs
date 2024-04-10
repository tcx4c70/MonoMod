namespace MonoMod.Core.Utils
{
    public interface IInstructionPattern
    {
        public int MinLength { get; }

        public AddressMeaning AddressMeaning { get; }

        public bool TryMatchAt(nint start, int byteNum, out ulong address, out int length);

        public bool TryFindMatch(nint start, int byteNum, out ulong address, out int offset, out int length);
    }
}

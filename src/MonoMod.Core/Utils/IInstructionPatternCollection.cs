using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MonoMod.Core.Utils
{
    public interface IInstructionPatternCollection : IEnumerable<IInstructionPattern>
    {
        public int MaxMinLength { get; }

        public bool TryMatchAt(nint start, int byteNum, out ulong address, [MaybeNullWhen(false)] out IInstructionPattern matchingPattern, out int length);

        public bool TryFindMatch(nint start, int byteNum, out ulong address, [MaybeNullWhen(false)] out IInstructionPattern matchingPattern, out int offset, out int length);
    }
}

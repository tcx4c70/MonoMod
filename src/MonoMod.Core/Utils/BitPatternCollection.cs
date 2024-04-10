using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MonoMod.Core.Utils
{
    public class BitPatternCollection : IInstructionPatternCollection
    {
        private readonly BitPattern[] patterns;

        public int MaxMinLength { get; }

        public BitPatternCollection(params BitPattern[] patterns)
        {
            this.patterns = patterns;
        }

        public IEnumerator<IInstructionPattern> GetEnumerator()
        {
            return ((IEnumerable<BitPattern>)this.patterns).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool TryFindMatch(nint start, int byteNum, out ulong address, [MaybeNullWhen(false)] out IInstructionPattern matchingPattern, out int offset, out int length)
        {
            foreach (var pattern in patterns)
            {
                if (pattern.TryFindMatch(start, byteNum, out address, out offset, out length))
                {
                    matchingPattern = pattern;
                    return true;
                }
            }
            address = 0;
            matchingPattern = null;
            offset = 0;
            length = 0;
            return false;
        }

        public bool TryMatchAt(nint start, int byteNum, out ulong address, [MaybeNullWhen(false)] out IInstructionPattern matchingPattern, out int length)
        {
            foreach (var pattern in patterns)
            {
                if (pattern.TryMatchAt(start, byteNum, out address, out length))
                {
                    matchingPattern = pattern;
                    return true;
                }
            }
            address = 0;
            length = 0;
            matchingPattern = null;
            return false;
        }
    }
}

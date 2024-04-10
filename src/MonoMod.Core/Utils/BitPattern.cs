using System;
using System.Numerics;

namespace MonoMod.Core.Utils
{
    public sealed class BitPattern : IInstructionPattern
    {
        public int MinLength { get; }

        public AddressMeaning AddressMeaning { get; }

        private uint[] addressMask;
        private uint[] ignoreMask;
        private uint[] pattern;

        public BitPattern(AddressMeaning addressMeaning, uint[] addressMask, uint[] ignoreMask, uint[] pattern)
        {
            if (addressMask.Length != pattern.Length || ignoreMask.Length != pattern.Length)
            {
                throw new ArgumentException("addressMask, ignoreMask and pattern must have the same length");
            }
            this.AddressMeaning = addressMeaning;
            this.addressMask = addressMask;
            this.ignoreMask = ignoreMask;
            this.pattern = pattern;
            this.MinLength = pattern.Length * sizeof(uint);
        }

        public unsafe bool TryFindMatch(nint start, int byteNum, out ulong address, out int offset, out int length)
        {
            address = 0;
            length = 0;
            offset = (int)(sizeof(uint) - (start % sizeof(uint))) % sizeof(uint);
            while (offset < byteNum)
            {
                if (TryMatchAt(start + offset, byteNum - offset, out address, out length))
                {
                    return true;
                }
                offset += sizeof(uint);
            }
            return false;
        }

        public unsafe bool TryMatchAt(nint start, int byteNum, out ulong address, out int length)
        {
            address = 0;
            length = 0;
            if (start % sizeof(uint) != 0 || byteNum < MinLength)
            {
                return false;
            }

            var data = new ReadOnlySpan<uint>((void*)start, byteNum / sizeof(uint));
            int idx = 0;

            while (idx < pattern.Length)
            {
                if ((pattern[idx] & ~ignoreMask[idx]) != (data[idx] & ~ignoreMask[idx]))
                {
                    return false;
                }
                if (addressMask[idx] != 0)
                {
                    address = (data[idx] & addressMask[idx]) >> CountTrailZeroes(addressMask[idx]);
                }
                ++idx;
            }
            length = pattern.Length * sizeof(uint);
            return true;
        }

        private int CountTrailZeroes(uint value)
        {
            if (value == 0)
            {
                return 32;
            }

            int count = 0;
            while ((value & 1) == 0)
            {
                value >>= 1;
                ++count;
            }
            return count;
        }
    }
}

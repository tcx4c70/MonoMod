using MonoMod.Logs;
using System;

namespace MonoMod.Core.Utils
{
    /// <summary>
    /// An address meaning for use with a <see cref="BytePattern"/>.
    /// </summary>
    public readonly struct AddressMeaning : IEquatable<AddressMeaning>
    {
        /// <summary>
        /// Gets the <see cref="AddressKind"/> associated with this meaning.
        /// </summary>
        public AddressKind Kind { get; }
        /// <summary>
        /// Gets the offset from the match start that an address is relative to, if it is relative.
        /// </summary>
        public int RelativeToOffset { get; }

        public int ShiftBits { get; }

        /// <summary>
        /// Constructs an <see cref="AddressMeaning"/> for the specified <see cref="AddressKind"/>.
        /// </summary>
        /// <param name="kind">The <see cref="AddressKind"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="kind"/> is invalid -OR- <paramref name="kind"/> is relative.</exception>
        public AddressMeaning(AddressKind kind) : this(kind, 0) { }

        /// <summary>
        /// Constructs an <see cref="AddressMeaning"/> for the specified <see cref="AddressKind"/> and relative offset.
        /// </summary>
        /// <param name="kind">The <see cref="AddressKind"/>.</param>
        /// <param name="relativeOffset">The offset relative to the match start.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="kind"/> is invalid
        /// -OR- <paramref name="kind"/> is absolute
        /// -OR- <paramref name="relativeOffset"/> is less than zero.</exception>
        public AddressMeaning(AddressKind kind, int relativeOffset) : this(kind, relativeOffset, 0) { }

        public AddressMeaning(AddressKind kind, int relativeOffset, int shiftBits)
        {
            kind.Validate();
            if (!kind.IsRelative())
                throw new ArgumentOutOfRangeException(nameof(kind));
            if (relativeOffset < 0)
                throw new ArgumentOutOfRangeException(nameof(relativeOffset));
            if (shiftBits < 0)
                throw new ArgumentOutOfRangeException(nameof(shiftBits));
            Kind = kind;
            RelativeToOffset = relativeOffset;
            ShiftBits = shiftBits;
        }

        private unsafe nint DoProcessAddress(nint basePtr, int offset, ulong address)
        {
            nint addr;
            if (Kind.IsAbsolute())
            {
                addr = (nint)address;
            }
            else
            { // IsRelative
                var offs = Kind.Is32Bit()
                    ? Unsafe.As<ulong, int>(ref address)
                    : Unsafe.As<ulong, long>(ref address);
                addr = (nint)(basePtr + offset + (offs << ShiftBits));
            }
            if (Kind.IsIndirect())
            {
                addr = *(nint*)addr;
            }
            return addr;
        }

        /// <summary>
        /// Processes an address according to this <see cref="AddressMeaning"/> into an absolute address.
        /// </summary>
        /// <param name="basePtr">The base pointer of the match.</param>
        /// <param name="offset">The offset from the base pointer that the match occurred at.</param>
        /// <param name="address">The address which was extracted from the match.</param>
        /// <returns>The resolved target address.</returns>
        public nint ProcessAddress(nint basePtr, int offset, ulong address)
        {
            return DoProcessAddress(basePtr, offset + RelativeToOffset, address);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is AddressMeaning meaning && Equals(meaning);
        }

        /// <inheritdoc/>
        public bool Equals(AddressMeaning other)
        {
            return Kind == other.Kind &&
                   RelativeToOffset == other.RelativeToOffset;
        }

        // Force the use of DebugFormatter, because we might be patching DefaultInterpolatedStringHandler
        /// <inheritdoc/>
        public override string ToString() => DebugFormatter.Format($"AddressMeaning({Kind.FastToString()}, offset: {RelativeToOffset})");

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(Kind, RelativeToOffset);
        }

        /// <summary>
        /// Compares two <see cref="AddressMeaning"/>s for equality.
        /// </summary>
        /// <param name="left">The first <see cref="AddressMeaning"/> to compare.</param>
        /// <param name="right">The second <see cref="AddressMeaning"/> to compare.</param>
        /// <returns><see langword="true"/> if the two <see cref="AddressMeaning"/>s are equal; <see langword="false"/> otherwise.</returns>
        public static bool operator ==(AddressMeaning left, AddressMeaning right) => left.Equals(right);
        /// <summary>
        /// Compares two <see cref="AddressMeaning"/>s for inequality.
        /// </summary>
        /// <param name="left">The first <see cref="AddressMeaning"/> to compare.</param>
        /// <param name="right">The second <see cref="AddressMeaning"/> to compare.</param>
        /// <returns><see langword="true"/> if the two <see cref="AddressMeaning"/>s are not equal; <see langword="false"/> otherwise.</returns>
        public static bool operator !=(AddressMeaning left, AddressMeaning right) => !(left == right);
    }
}

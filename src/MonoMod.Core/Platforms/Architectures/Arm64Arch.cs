using MonoMod.Core.Utils;
using MonoMod.Utils;
using System;

namespace MonoMod.Core.Platforms.Architectures
{
    internal sealed class Arm64Arch : IArchitecture
    {
        public ArchitectureKind Target => ArchitectureKind.Arm64;

        public ArchitectureFeature Features => ArchitectureFeature.FixedInstructionSize;

        public unsafe IInstructionPatternCollection KnownMethodThunks => Helpers.GetOrInit(ref lazyKnownMethodThunks, CreateKnownMethodThunks);

        public IAltEntryFactory AltEntryFactory => throw new NotImplementedException();

        private readonly ISystem system;

        private BitPatternCollection? lazyKnownMethodThunks;

        private static BitPatternCollection CreateKnownMethodThunks()
        {
            if (PlatformDetection.Runtime is RuntimeKind.Framework or RuntimeKind.CoreCLR)
            {
                return new BitPatternCollection(
                    // FixupPrecode 0x4000 page size
                    new BitPattern(new(AddressKind.Rel64 | AddressKind.Indirect, 0, 2),
                        new uint[] {
                            0x00ffffe0,
                            0x00000000,
                            0x00000000,
                            0x00000000,
                            0x00000000,
                        },
                        new uint[] {
                            0x00ffffe0,
                            0x00000000,
                            0x00ffffe0,
                            0x00ffffe0,
                            0x00000000,
                        },
                        new uint[] {
                            0x5800000b, // ldr x11, pc + page_size
                            0xd61f0160, // br  x11
                            0x5800000c, // ldr x12, pc + page_size
                            0x5800000b, // ldr x11, pc + page_size + 0x4
                            0xd61f0160, // br  x11
                        })
                );
            }
            else
            {
                return new();
            }
        }

        private sealed class Abs64Kind : DetourKindBase
        {
            public static readonly Abs64Kind Instance = new();

            public override int Size => 4 + 4 + 8;

            public override int GetBytes(IntPtr from, IntPtr to, Span<byte> buffer, object? data, out IDisposable? allocHandle)
            {
                Unsafe.WriteUnaligned(ref buffer[0], 0x5800004FU); // ldr x15, .+8
                Unsafe.WriteUnaligned(ref buffer[4], 0xD61F01E0U); // br  x15
                Unsafe.WriteUnaligned(ref buffer[8], (ulong)to);   // target address
                allocHandle = null;
                return Size;
            }

            public override bool TryGetRetargetInfo(NativeDetourInfo orig, IntPtr to, int maxSize, out NativeDetourInfo retargetInfo)
            {
                retargetInfo = orig with { To = to };
                return true;
            }

            public override int DoRetarget(NativeDetourInfo orig, IntPtr to, Span<byte> buffer, object? data,
                out IDisposable? allocHandle, out bool needsRepatch, out bool disposeOldAlloc)
            {
                needsRepatch = true;
                disposeOldAlloc = true;
                return GetBytes(orig.From, to, buffer, data, out allocHandle);
            }
        }

        public Arm64Arch(ISystem system)
        {
            this.system = system;
        }

        public NativeDetourInfo ComputeDetourInfo(IntPtr from, IntPtr target, int maxSizeHint = -1)
        {
            if (maxSizeHint >= 0 && maxSizeHint < Abs64Kind.Instance.Size)
            {
                MMDbgLog.Warning($"Size too small for all known detour kinds; defaulting to Abs64. provided size: {maxSizeHint}");
            }
            return new (from, target, Abs64Kind.Instance, null);
        }

        public NativeDetourInfo ComputeRetargetInfo(NativeDetourInfo detour, IntPtr target, int maxSizeHint = -1)
        {
            throw new NotImplementedException();
        }

        public ReadOnlyMemory<IAllocatedMemory> CreateNativeVtableProxyStubs(IntPtr vtableBase, int vtableSize)
        {
            throw new NotImplementedException();
        }

        public IAllocatedMemory CreateSpecialEntryStub(IntPtr target, IntPtr argument)
        {
            throw new NotImplementedException();
        }

        public int GetDetourBytes(NativeDetourInfo info, Span<byte> buffer, out IDisposable? allocationHandle)
        {
            return DetourKindBase.GetDetourBytes(info, buffer, out allocationHandle);
        }

        public int GetRetargetBytes(NativeDetourInfo original, NativeDetourInfo retarget, Span<byte> buffer, out IDisposable? allocationHandle, out bool needsRepatch, out bool disposeOldAlloc)
        {
            return DetourKindBase.DoRetarget(original, retarget, buffer, out allocationHandle, out needsRepatch, out disposeOldAlloc);
        }
    }
}

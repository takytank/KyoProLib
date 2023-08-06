using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V11
{
	public static class BitOperations
	{
		private static readonly byte[] _log2DeBruijn = new byte[32]
		{
			00, 09, 01, 10, 13, 21, 02, 29,
			11, 14, 16, 18, 22, 25, 03, 30,
			08, 12, 20, 28, 15, 17, 24, 07,
			19, 27, 23, 06, 26, 05, 04, 31
		};

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int PopCount(uint bits)
		{
			bits = (bits & 0x55555555) + (bits >> 1 & 0x55555555);
			bits = (bits & 0x33333333) + (bits >> 2 & 0x33333333);
			bits = (bits & 0x0f0f0f0f) + (bits >> 4 & 0x0f0f0f0f);
			bits = (bits & 0x00ff00ff) + (bits >> 8 & 0x00ff00ff);
			bits = (bits & 0x0000ffff) + (bits >> 16 & 0x0000ffff);
			return (int)bits;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int PopCount(ulong bits)
		{
			bits = ((bits & 0xaaaaaaaaaaaaaaaa) >> 1) + (bits & 0x5555555555555555);
			bits = ((bits & 0xcccccccccccccccc) >> 2) + (bits & 0x3333333333333333);
			bits = ((bits & 0xf0f0f0f0f0f0f0f0) >> 4) + (bits & 0x0f0f0f0f0f0f0f0f);
			bits = ((bits & 0xff00ff00ff00ff00) >> 8) + (bits & 0x00ff00ff00ff00ff);
			bits = ((bits & 0xffff0000ffff0000) >> 16) + (bits & 0x0000ffff0000ffff);
			bits = ((bits & 0xffffffff00000000) >> 32) + (bits & 0x00000000ffffffff);
			return (int)bits;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Log2(uint value)
		{
			value |= value >> 01;
			value |= value >> 02;
			value |= value >> 04;
			value |= value >> 08;
			value |= value >> 16;

			return _log2DeBruijn[(value * 0x07C4ACDDU) >> 27];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Log2(ulong value)
		{
			uint msb = (uint)(value >> 32);
			return msb == 0 ? Log2((uint)value) : 32 + Log2(msb);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int LeadingZeroCount(uint value)
		{
			int count = 0;
			var data = default(Union32);
			data.AsFloat = (float)value + 0.5f;
			count += (int)(158 - (data.AsUint >> 23));

			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int LeadingZeroCount(ulong value)
		{
			int count = 0;
			var data = default(Union64);
			data.AsDouble = (double)value + 0.5;
			count += (int)(1054 - (data.AsUlong >> 52));

			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int TrailingZeroCount(uint value)
			=> value == 0 ? 32 : PopCount(~value & (value - 1));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int TrailingZeroCount(ulong value)
			=> value == 0 ? 64 : PopCount(~value & (value - 1));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint RotateLeft(uint value, int offset)
			=> (value << offset) | (value >> (32 - offset));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ulong RotateLeft(ulong value, int offset)
			=> (value << offset) | (value >> (64 - offset));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint RotateRight(uint value, int offset)
			=> (value >> offset) | (value << (32 - offset));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ulong RotateRight(ulong value, int offset)
			=> (value >> offset) | (value << (64 - offset));

		[StructLayout(LayoutKind.Explicit)]
		public struct Union64
		{
			[FieldOffset(0)]
			public ulong AsUlong;

			[FieldOffset(0)]
			public double AsDouble;
		}

		[StructLayout(LayoutKind.Explicit)]
		public struct Union32
		{
			[FieldOffset(0)]
			public uint AsUint;

			[FieldOffset(0)]
			public float AsFloat;
		}
	}
}

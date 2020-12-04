using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.Core31
{
	public static class Extensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int PopCount(this BitFlag bit)
			=> (int)Popcnt.PopCount((uint)bit.Flag);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<T> AsSpan<T>(this List<T> list)
		{
			return Unsafe.As<FakeList<T>>(list).Array.AsSpan(0, list.Count);
		}

		private class FakeList<T>
		{
			public T[] Array = null;
		}
	}
}

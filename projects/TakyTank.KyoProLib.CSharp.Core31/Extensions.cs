using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.Core31
{
	public static class Extensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int PopCount(this BitFlag bit)
			=> (int)BitOperations.PopCount((uint)bit.Flag);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<T> AsSpan<T>(this List<T> list)
		{
			return Unsafe.As<FakeList<T>>(list).Array.AsSpan(0, list.Count);
		}

		private class FakeList<T>
		{
			public T[] Array = null;
		}

		public static void Swap(this string str, int i, int j)
		{
			var span = str.AsWriteableSpan();
			(span[i], span[j]) = (span[j], span[i]);
		}

		public static char Replace(this string str, int index, char c)
		{
			var span = str.AsWriteableSpan();
			char old = span[index];
			span[index] = c;
			return old;
		}

		public static Span<char> AsWriteableSpan(this string str)
		{
			unsafe {
				fixed (char* p = str) {
					return MemoryMarshal.CreateSpan(ref *p, str.Length);
				}
			}
		}
	}
}

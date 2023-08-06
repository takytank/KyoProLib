using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V11
{
	public class SubArray
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long Kadane(long[] array, bool allowsEmpty = false)
			=> Max(array, allowsEmpty);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long Max(long[] array, bool allowsEmpty = false)
		{
			long max = allowsEmpty == false ? long.MinValue : 0;
			long sum = 0;
			foreach (var value in array) {
				sum = Math.Max(0, sum + value);
				max = Math.Max(max, sum);
			}

			return max;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static (long max, int l, int r) MaxRange(long[] array, bool allowsEmpty = false)
		{
			long max = allowsEmpty == false ? long.MinValue : 0;
			int tempL = 0;
			int l = 0;
			int r = 0;
			long sum = 0;
			for (int i = 0; i < array.Length; ++i) {
				if (sum <= 0) {
					tempL = i;
					sum = array[i];
				} else {
					sum += array[i];
				}

				if (sum > max) {
					max = sum;
					l = tempL;
					r = i + 1;
				}
			}

			return (max, l, r);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long Min(long[] array, bool allowsEmpty = false)
		{
			long min = allowsEmpty == false ? long.MaxValue : 0;
			long sum = 0;
			foreach (var value in array) {
				sum = Math.Min(0, sum + value);
				min = Math.Min(min, sum);
			}

			return min;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static (long min, int l, int r) MinRange(long[] array, bool allowsEmpty = false)
		{
			long min = allowsEmpty == false ? long.MaxValue : 0;
			int tempL = 0;
			int l = 0;
			int r = 0;
			long sum = 0;
			for (int i = 0; i < array.Length; ++i) {
				if (sum >= 0) {
					tempL = i;
					sum = array[i];
				} else {
					sum += array[i];
				}

				if (sum < min) {
					min = sum;
					l = tempL;
					r = i + 1;
				}
			}

			return (min, l, r);
		}
	}
}

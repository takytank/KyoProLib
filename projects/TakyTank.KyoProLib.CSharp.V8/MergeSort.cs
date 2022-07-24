using System;
using System.Collections.Generic;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V8
{
	public static class MergeSort
	{
		public static long InPlaceSort<T>(Span<T> array)
			where T : IComparable<T>
		{
			int n = array.Length;
			if (n <= 1) {
				return 0;
			}

			long inversionNumber = 0;
			var left = array[..(n / 2)].ToArray();
			var right = array[(n / 2)..].ToArray();
			inversionNumber += InPlaceSort<T>(left);
			inversionNumber += InPlaceSort<T>(right);

			int ai = 0;
			int li = 0;
			int ri = 0;
			while (ai < n) {
				if (li < left.Length && (ri == right.Length || left[li].CompareTo(right[ri]) <= 0)) {
					array[ai] = left[li];
					++ai;
					++li;
				} else {
					inversionNumber += n / 2 - li;
					array[ai] = right[ri];
					++ai;
					++ri;
				}
			}

			return inversionNumber;
		}

		public static (T[] sorted, long inversionNumber) Sort<T>(ReadOnlySpan<T> array)
			where T : IComparable<T>
		{
			int n = array.Length;
			if (n <= 1) {
				return (array.ToArray(), 0);
			}

			long inversionNumber = 0;
			var (left, lc) = Sort(array[..(n / 2)]);
			var (right, rc) = Sort(array[(n / 2)..]);

			inversionNumber += lc;
			inversionNumber += rc;

			int ai = 0;
			int li = 0;
			int ri = 0;
			var sorted = new T[n];
			while (ai < n) {
				if (li < left.Length && (ri == right.Length || left[li].CompareTo(right[ri]) <= 0)) {
					sorted[ai] = left[li];
					++ai;
					++li;
				} else {
					inversionNumber += n / 2 - li;
					sorted[ai] = right[ri];
					++ai;
					++ri;
				}
			}

			return (sorted, inversionNumber);
		}
	}
}

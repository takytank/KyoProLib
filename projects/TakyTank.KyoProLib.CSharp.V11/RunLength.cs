using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V11
{
	public static class RunLength
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static List<(char value, int count)> Zip(string str) => Zip<char>(str);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static List<(T value, int count)> Zip<T>(ReadOnlySpan<T> array)
			where T : IComparable<T>
		{
			int n = array.Length;
			var ret = new List<(T value, int count)>();
			int count = 0;
			T last = default;
			for (int i = 0; i < n; i++) {
				if (array[i].CompareTo(last) == 0) {
					++count;
				} else {
					if (count > 0) {
						ret.Add((last, count));
					}

					count = 1;
					last = array[i];
				}
			}

			ret.Add((last, count));

			return ret;
		}
	}
}

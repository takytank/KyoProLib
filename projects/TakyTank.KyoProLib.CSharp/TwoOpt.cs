using System;
using System.Collections.Generic;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class TwoOpt
	{
		private readonly int _n;
		private readonly int _m;
		private readonly int[] _array;
		private readonly Func<int, int, long> _getCost;
		public TwoOpt(
			ReadOnlySpan<int> src,
			bool isRing,
			Func<int, int, long> getCost)
		{
			_n = src.Length;
			_getCost = getCost;

			_m = isRing ? _n + 1 : _n;
			_array = new int[_m];
			for (int i = 0; i < _n; i++) {
				_array[i] = src[i];
			}

			if (isRing) {
				_array[_n] = _array[0];
			}

			for (int i = 1; i < _n; i++) {
				long min = long.MaxValue;
				int minIndex = 0;
				for (int j = i; j < _n; j++) {
					long cost = _getCost(_array[i - 1], _array[j]);
					if (cost < min) {
						min = cost;
						minIndex = j;
					}
				}

				(_array[minIndex], _array[i]) = (_array[i], _array[minIndex]);
			}
		}

		public int[] Optimize(int count)
		{
			var dst = new int[_m];
			for (int i = 0; i < _m; i++) {
				dst[i] = _array[i];
			}

			var rnd = new Random();
			for (int k = 0; k < count; k++) {
				int i = rnd.Next(0, _m - 1);
				int j = rnd.Next(1, _m);
				if (i + 1 == j) {
					continue;
				}

				if (j < i) {
					(i, j) = (j, i);
				}

				long before = _getCost(dst[i], dst[i + 1]) + _getCost(dst[j - 1], dst[j]);
				long after = _getCost(dst[i], dst[j - 1]) + _getCost(dst[i + 1], dst[j]);
				if (after < before) {
					++i;
					--j;
					while (i < j) {
						(dst[i], dst[j]) = (dst[j], dst[i]);
						++i;
						--j;
					}
				}
			}

			return dst;
		}
	}
}

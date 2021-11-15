using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V8
{
	public class TwoOpt<T>
	{
		private readonly int _n;
		private readonly int _m;
		private readonly T[] _array;
		private readonly Func<T, T, long> _getCost;
		private readonly Random _rnd = new Random();

		public T this[int i] => _array[i];
		public T[] Array => _array;

		public TwoOpt(
			ReadOnlySpan<T> src,
			bool isRing,
			Func<T, T, long> getCost)
		{
			_n = src.Length;
			_getCost = getCost;

			_m = isRing ? _n + 1 : _n;
			_array = new T[_m];
			for (int i = 0; i < _n; i++) {
				_array[i] = src[i];
			}

			if (isRing) {
				_array[_n] = _array[0];
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void InitializeByNearestNeighbour()
		{
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Optimize() => Optimize(1, 0, _m);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Optimize(int count) => Optimize(count, 0, _m);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Optimize(int count, int min, int max)
		{
			for (int k = 0; k < count; k++) {
				int i = _rnd.Next(min, max - 1);
				int j = _rnd.Next(min + 1, max);
				while (i + 1 == j) {
					j = _rnd.Next(min + 1, max);
				}

				if (j < i) {
					(i, j) = (j, i);
				}

				long before = _getCost(_array[i], _array[i + 1])
					+ _getCost(_array[j - 1], _array[j]);
				long after = _getCost(_array[i], _array[j - 1])
					+ _getCost(_array[i + 1], _array[j]);
				if (after < before) {
					++i;
					--j;
					while (i < j) {
						(_array[i], _array[j]) = (_array[j], _array[i]);
						++i;
						--j;
					}
				}
			}
		}
	}
}

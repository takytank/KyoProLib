using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V11
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
		public void InitializeByNearestNeighbour() => InitializeByNearestNeighbour(0, _n);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void InitializeByNearestNeighbour(int left, int right)
		{
			for (int i = left + 1; i < right; i++) {
				long min = long.MaxValue;
				int minIndex = 0;
				for (int j = i; j < right; j++) {
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
		public void Optimize(int count, int left, int right)
		{
			for (int k = 0; k < count; k++) {
				int i = _rnd.Next(left, right - 1);
				int j = _rnd.Next(left + 1, right);
				while (i + 1 == j) {
					j = _rnd.Next(left + 1, right);
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

		public (long delta, int targetIndex) EstimateExchange(int removeIndex, T newValue)
			=> EstimateExchange(removeIndex, newValue, 1, _m - 1);
		public (long delta, int targetIndex) EstimateExchange(
			int removeIndex, T newValue, int left, int right)
		{
			long after = long.MaxValue;
			int target = 1;
			for (int i = left; i <= right; i++) {
				int l = i - 1;
				if (l == removeIndex) {
					l--;
				}

				int r = i;
				if (r == removeIndex) {
					r++;
				}

				long tempAfter = _getCost(_array[l], newValue) + _getCost(newValue, _array[r]);
				if (tempAfter < after) {
					after = tempAfter;
					target = i;
				}
			}

			if (removeIndex == target || removeIndex == target - 1) {
				long before = _getCost(_array[removeIndex - 1], _array[removeIndex])
					+ _getCost(_array[removeIndex], _array[removeIndex + 1]);
				return (after - before, target);
			} else {
				long delta = 0;
				delta -= _getCost(_array[removeIndex - 1], _array[removeIndex]);
				delta -= _getCost(_array[removeIndex], _array[removeIndex + 1]);
				delta += _getCost(_array[removeIndex - 1], _array[removeIndex + 1]);
				delta -= _getCost(_array[target - 1], _array[target]);
				delta += after;
				return (delta, target);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Exchange(int removeIndex, T newValue, bool forced)
			=> Exchange(removeIndex, newValue, 1, _m - 1, forced);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Exchange(int removeIndex, T newValue, int left, int right, bool forced)
		{
			var (delta, target) = EstimateExchange(removeIndex, newValue, left, right);
			if (forced || delta < 0) {
				Exchange(removeIndex, target, newValue);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Exchange(int removeIndex, int insertIndex, T newValue)
		{
			if (removeIndex < insertIndex) {
				for (int i = removeIndex; i < insertIndex - 1; i++) {
					_array[i] = _array[i + 1];
				}

				_array[insertIndex - 1] = newValue;
			} else if (removeIndex > insertIndex) {
				for (int i = removeIndex - 1; i >= insertIndex; i--) {
					_array[i + 1] = _array[i];
				}

				_array[insertIndex] = newValue;
			} else {
				_array[removeIndex] = newValue;
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V8
{
	public class Doubling
	{
		private readonly int _n;
		private readonly int _k;
		private readonly int[,] _map;

		public Doubling(long maxStep, int[] transition)
			: this(transition.Length, maxStep, i => transition[i])
		{
		}

		public Doubling(int n, long maxStep, Func<int, int> transition)
		{
			_n = n;
			_k = 0;
			while (maxStep > 0) {
				++_k;
				maxStep >>= 1;
			}

			_map = new int[_k, n];
			for (int i = 0; i < _n; ++i) {
				_map[0, i] = transition(i);
			}

			Build();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Build()
		{
			for (int i = 0; i < _k - 1; ++i) {
				for (int j = 0; j < _n; ++j) {
					if (_map[i, j] < 0) {
						_map[i + 1, j] = -1;
					} else {
						_map[i + 1, j] = _map[i, _map[i, j]];
					}
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Query(int s, long length)
		{
			int t = s;
			for (int i = _k - 1; i >= 0; --i) {
				if (((1L << i) & length) != 0 && _map[i, t] >= 0) {
					t = _map[i, t];
				}
			}

			return t;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long FindRightest(int s, Func<int, bool> checker)
		{
			int t = s;
			long ret = 0;
			for (int i = _k - 1; i >= 0; i--) {
				int tempIndex = _map[i, t];
				if (tempIndex >= 0 && checker(tempIndex)) {
					t = tempIndex;
					ret += 1L << i;
				}
			}

			return ret;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long LowerBound(int s, int t)
		{
			int current = s;
			long ret = 0;
			for (int i = _k - 1; i >= 0; --i) {
				int temp = _map[i, current];
				if (temp >= 0 && temp < t) {
					current = temp;
					ret += 1L << i;
				}
			}

			return ret + 1;
		}
	}

	public class Doubling<T>
		where T : struct, IComparable<T>
	{
		private readonly int _n;
		private readonly int[,] _indexMap;
		private readonly T[] _initialValues;
		private readonly T[,] _valueMap;
		private readonly int _k;
		private readonly Func<T, T, T> _merge;

		public Doubling(
			int n,
			long maxStep,
			Func<int, T> initializeCost,
			Func<int, (int index, T cost)> transelate,
			Func<T, T, T> merge)
		{
			_n = n;
			_merge = merge;
			_k = 0;
			while (maxStep > 0) {
				++_k;
				maxStep >>= 1;
			}

			_indexMap = new int[_k, n];
			_valueMap = new T[_k, n];
			_initialValues = new T[n];
			for (int i = 0; i < n; ++i) {
				var (index, value) = transelate(i);
				_indexMap[0, i] = index;
				_valueMap[0, i] = value;
				_initialValues[i] = initializeCost(i);
			}

			Build();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Build()
		{
			for (int i = 0; i < _k - 1; i++) {
				for (int j = 0; j < _n; j++) {
					if (_indexMap[i, j] < 0) {
						_indexMap[i + 1, j] = -1;
					} else {
						_indexMap[i + 1, j] = _indexMap[i, _indexMap[i, j]];
						_valueMap[i + 1, j] = _merge(
							_valueMap[i, j],
							_valueMap[i, _indexMap[i, j]]);
					}
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Query(int s, long length)
		{
			int t = s;
			var ret = _initialValues[s];
			for (int i = _k - 1; i >= 0; i--) {
				if (((1L << i) & length) != 0) {
					ret = _merge(ret, _valueMap[i, t]);
					t = _indexMap[i, t];
				}
			}

			return ret;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long FindRightest(int s, Func<T, bool> checker)
		{
			int t = s;
			long ret = 0;
			T current = _initialValues[s];
			for (int i = _k - 1; i >= 0; i--) {
				int tempIndex = _indexMap[i, t];
				T tempValue = _merge(current, _valueMap[i, t]);
				if (tempIndex >= 0 && checker(tempValue)) {
					current = tempValue;
					t = tempIndex;
					ret += 1L << i;
				}
			}

			return ret;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long LowerBound(int s, T value)
		{
			int t = s;
			long ret = 0;
			T current = _initialValues[s];
			for (int i = _k - 1; i >= 0; i--) {
				int tempIndex = t = _indexMap[i, t];
				T tempValue = _merge(current, _valueMap[i, t]);
				if (tempIndex >= 0 && tempValue.CompareTo(value) < 0) {
					current = tempValue;
					t = _indexMap[i, t];
					ret += 1L << i;
				}
			}

			return ret + 1;
		}
	}
}

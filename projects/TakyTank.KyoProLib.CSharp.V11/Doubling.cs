using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V11
{
	public class Doubling
	{
		private readonly int _n;
		private readonly long _maxStep;
		private readonly int _k;
		private readonly int[,] _map;

		public Doubling(long maxStep, int[] transition)
			: this(transition.Length, maxStep, i => transition[i])
		{
		}

		public Doubling(int n, long maxStep, Func<int, int> transition)
		{
			_n = n;
			_maxStep = maxStep;
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
		public long FindRightest(int s, Func<int, bool> checker, long maxStep = -1)
		{
			if (maxStep == -1) {
				maxStep = _maxStep;
			}

			int t = s;
			long step = 0;
			for (int i = _k - 1; i >= 0; i--) {
				int tempIndex = _map[i, t];
				long tempStep = step + (1L << i);
				if (tempIndex >= 0 && tempStep <= maxStep && checker(tempIndex)) {
					t = tempIndex;
					step = tempStep;
				}
			}

			return step;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long LowerBound(int s, int t, long maxStep = -1)
		{
			if (maxStep == -1) {
				maxStep = _maxStep;
			}

			int current = s;
			long step = 0;
			for (int i = _k - 1; i >= 0; --i) {
				int temp = _map[i, current];
				long tempStep = step + (1L << i);
				if (temp >= 0 && tempStep <= maxStep && temp < t) {
					current = temp;
					step = tempStep;
				}
			}

			return step + 1;
		}
	}

	public class Doubling<T>
		where T : struct, IComparable<T>
	{
		private readonly int _n;
		private readonly long _maxStep;
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
			_maxStep = maxStep;
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
		public (long step, T value) FindRightest(int s, Func<T, bool> checker, long maxStep = -1)
		{
			if (maxStep == -1) {
				maxStep = _maxStep;
			}

			int t = s;
			long step = 0;
			T value = _initialValues[s];
			for (int i = _k - 1; i >= 0; i--) {
				int tempIndex = _indexMap[i, t];
				T tempValue = _merge(value, _valueMap[i, t]);
				long tempStep = step + (1L << i);
				if (tempIndex >= 0 && tempStep <= maxStep && checker(tempValue)) {
					value = tempValue;
					t = tempIndex;
					step = tempStep;
				}
			}

			return (step, value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long step, T value) LowerBound(int s, T target, long maxStep = -1)
		{
			if (maxStep == -1) {
				maxStep = _maxStep;
			}

			int t = s;
			long step = 0;
			T value = _initialValues[s];
			for (int i = _k - 1; i >= 0; i--) {
				int tempIndex = _indexMap[i, t];
				T tempValue = _merge(value, _valueMap[i, t]);
				long tempStep = step + (1L << i);
				if (tempIndex >= 0 && tempStep <= maxStep && tempValue.CompareTo(target) < 0) {
					value = tempValue;
					t = _indexMap[i, t];
					step = tempStep;
				}
			}

			return (step + 1, value);
		}
	}
}

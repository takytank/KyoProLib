using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class SparseTable<T>
	{
		private readonly T[] _baseArray;
		private readonly T _unity;
		private readonly Func<T, T, T> _operate;
		private T[,] _table;
		private int[] _lookup;

		public SparseTable(int n, T unity, Func<T, T, T> operate)
		{
			_baseArray = new T[n];
			_unity = unity;
			_operate = operate;
		}

		public SparseTable(T[] baseArray, T unity, Func<T, T, T> operate)
		{
			_baseArray = baseArray;
			_unity = unity;
			_operate = operate;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(int i, T value) => _baseArray[i] = value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Build()
		{
			int height = 0;
			while ((1 << height) <= _baseArray.Length) {
				++height;
			}

			int size = 1 << height;
			_table = new T[height, size];
			for (int i = 0; i < height; i++) {
				for (int j = 0; j < size; j++) {
					_table[i, j] = _unity;
				}
			}

			for (int i = 0; i < _baseArray.Length; ++i) {
				_table[0, i] = _baseArray[i];
			}

			for (int i = 1; i < height; ++i) {
				int length = 1 << i;
				for (int j = 0; j + length <= size; ++j) {
					_table[i, j] = _operate(
						_table[i - 1, j],
						_table[i - 1, j + (1 << (i - 1))]);
				}
			}

			_lookup = new int[_baseArray.Length + 1];
			for (int i = 2; i < _lookup.Length; ++i) {
				_lookup[i] = _lookup[i >> 1] + 1;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Query(int l, int r)
		{
			int rank = _lookup[r - l];
			return _operate(_table[rank, l], _table[rank, r - (1 << rank)]);
		}
	}
}

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

		public SparseTable(int n, T unity, Func<T, T, T> operate, bool fills = true)
		{
			_baseArray = new T[n];
			_unity = unity;
			_operate = operate;

			if (fills) {
				for (int i = 0; i < n; i++) {
					_baseArray[i] = unity;
				}
			}
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
			int n = _baseArray.Length;
			_lookup = new int[n + 1];
			for (int i = 2; i <= n; ++i) {
				_lookup[i] = _lookup[i >> 1] + 1;
			}

			int logN = _lookup[n] + 1;
			_table = new T[logN, n];
			for (int i = 0; i < _baseArray.Length; ++i) {
				_table[0, i] = _baseArray[i];
			}

			for (int i = 1; i < logN; ++i) {
				int length = 1 << i;
				int offset = 1 << (i - 1);
				for (int j = 0; j + length <= n; ++j) {
					_table[i, j] = _operate(
						_table[i - 1, j],
						_table[i - 1, j + offset]);
				}
			}

			
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Query(int l, int r)
		{
			int rank = _lookup[r - l];
			return _operate(_table[rank, l], _table[rank, r - (1 << rank)]);
		}
	}

	public class SparseTable2D<T>
	{
		private readonly T[,] _baseArray;
		private readonly T _unity;
		private readonly Func<T, T, T> _operate;
		private T[,,,] _table;
		private int[] _lookup;

		public SparseTable2D(int h, int w, T unity, Func<T, T, T> operate, bool fills = true)
		{
			_baseArray = new T[h, w];
			_unity = unity;
			_operate = operate;

			if (fills) {
				for (int i = 0; i < h; i++) {
					for (int j = 0; j < w; j++) {
						_baseArray[i, j] = _unity;
					}
				}
			}
		}

		public SparseTable2D(T[,] baseArray, T unity, Func<T, T, T> operate)
		{
			_baseArray = baseArray;
			_unity = unity;
			_operate = operate;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(int y, int x, T value) => _baseArray[y, x] = value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Build()
		{
			int h = _baseArray.GetLength(0);
			int w = _baseArray.GetLength(1);
			int n = Math.Max(h, w);
			_lookup = new int[n + 1];
			for (int i = 2; i <= n; ++i) {
				_lookup[i] = _lookup[i >> 1] + 1;
			}

			int logH = _lookup[h] + 1;
			int logW = _lookup[w] + 1;
			_table = new T[logH, logW, h, w];
			for (int i = 0; i < h; ++i) {
				for (int j = 0; j < w; ++j) {
					_table[0, 0, i, j] = _baseArray[i, j];
				}
			}

			for (int ii = 1; ii < logH; ++ii) {
				int length = 1 << ii;
				int offset = 1 << (ii - 1);
				for (int i = 0; i + length <= h; ++i) {
					for (int j = 0; j < w; ++j) {
						_table[ii, 0, i, j] = _operate(
							_table[ii - 1, 0, i, j],
							_table[ii - 1, 0, i + offset, j]);
					}
				}
			}

			for (int ii = 0; ii < logH; ++ii) {
				int lengthI = 1 << ii;
				for (int jj = 1; jj < logW; ++jj) {
					int lengthJ = 1 << jj;
					int offset = 1 << (jj - 1);
					for (int i = 0; i + lengthI <= h; ++i) {
						for (int j = 0; j + lengthJ <= w; ++j) {
							_table[ii, jj, i, j] = _operate(
								_table[ii, jj - 1, i, j],
								_table[ii, jj - 1, i, j + offset]);
						}
					}
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Query(int lx, int ly, int rx, int ry)
		{
			int kx = _lookup[rx - lx];
			int ky = _lookup[ry - ly];
			return _operate(
				_operate(
					_table[kx, ky, lx, ly],
					_table[kx, ky, rx - (1 << kx), ly]),
				_operate(
					_table[kx, ky, lx, ry - (1 << ky)],
					_table[kx, ky, rx - (1 << kx), ry - (1 << ky)])
				);
		}
	}
}

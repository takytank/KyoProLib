using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V11
{
	public class Grid<T>
	{
		private static readonly int[] _delta2 = { 1, 0, 1 };
		private static readonly int[] _delta4 = { 1, 0, -1, 0, 1 };
		private static readonly int[] _delta5 = { 1, 0, 0, -1, 0, 1 };
		private static readonly int[] _delta8 = { 1, 0, -1, 0, 1, 1, -1, -1, 1 };

		private readonly int _height;
		private readonly int _width;
		private readonly T[,] _grid;

		public int Height => _height;
		public int Width => _width;
		public T this[int i, int j]
		{
			get => _grid[i, j];
			set => _grid[i, j] = value;
		}

		public Grid(int height, int width)
		{
			_height = height;
			_width = width;
			_grid = new T[height, width];
		}

		public Grid(T[,] data)
		{
			_height = data.GetLength(0);
			_width = data.GetLength(1);
			_grid = data;
		}

		public Grid(int height, int width, Func<int, int, T> initialize)
			: this(height, width)
		{
			for (int i = 0; i < _height; i++) {
				for (int j = 0; j < _width; j++) {
					_grid[i, j] = initialize(i, j);
				}
			}
		}

		public Grid(int height, int width, T[][] data)
			: this(height, width, (i, j) => data[i][j])
		{
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ForEach(Func<T, bool> should, Action<int, int> action)
		{
			for (int i = 0; i < _height; i++) {
				for (int j = 0; j < _width; j++) {
					if (should(_grid[i, j]) == false) {
						continue;
					}

					action(i, j);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Grid<T> Copy()
		{
			var copied = new Grid<T>(_height, _width);
			for (int i = 0; i < _height; i++) {
				for (int j = 0; j < _width; j++) {
					copied[i, j] = this[i, j];
				}
			}

			return copied;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Grid<T> Rotate90R()
		{
			var newGrid = new Grid<T>(_width, _height);
			for (int i = 0; i < _height; i++) {
				for (int j = 0; j < _width; j++) {
					newGrid[j, _height - 1 - i] = _grid[i, j];
				}
			}

			return newGrid;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Grid<T> Rotate90L()
		{
			var newGrid = new Grid<T>(_width, _height);
			for (int i = 0; i < _height; i++) {
				for (int j = 0; j < _width; j++) {
					newGrid[_width - 1 - j, i] = _grid[i, j];
				}
			}

			return newGrid;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerable<(int i, int j)> Adjacence2(int i, int j)
		{
			for (int dn = 0; dn < 2; ++dn) {
				int d2i = i + _delta2[dn];
				int d2j = j + _delta2[dn + 1];
				if ((uint)d2i < (uint)_height && (uint)d2j < (uint)_width) {
					yield return (d2i, d2j);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerable<(int i, int j)> Adjacence4(int i, int j)
		{
			for (int dn = 0; dn < 4; ++dn) {
				int d4i = i + _delta4[dn];
				int d4j = j + _delta4[dn + 1];
				if ((uint)d4i < (uint)_height && (uint)d4j < (uint)_width) {
					yield return (d4i, d4j);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerable<(int i, int j)> Adjacence5(int i, int j)
		{
			for (int dn = 0; dn < 5; ++dn) {
				int d5i = i + _delta5[dn];
				int d5j = j + _delta5[dn + 1];
				if ((uint)d5i < (uint)_height && (uint)d5j < (uint)_width) {
					yield return (d5i, d5j);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerable<(int i, int j)> Adjacence8(int i, int j)
		{
			for (int dn = 0; dn < 8; ++dn) {
				int d8i = i + _delta8[dn];
				int d8j = j + _delta8[dn + 1];
				if ((uint)d8i < (uint)_height && (uint)d8j < (uint)_width) {
					yield return (d8i, d8j);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int ToIndex(int i, int j) => i * _width + j;
	}
}

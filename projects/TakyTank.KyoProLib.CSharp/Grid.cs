using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class Grid<T>
	{
		private static readonly int[] delta2_ = { 1, 0, 1 };
		private static readonly int[] delta4_ = { 1, 0, -1, 0, 1 };
		private static readonly int[] delta8_ = { 1, 0, -1, 0, 1, 1, -1, -1, 1 };

		private readonly int height_;
		private readonly int width_;
		private readonly T[,] grid_;

		public T this[int i, int j]
		{
			get => grid_[i, j];
			set => grid_[i, j] = value;
		}

		public Grid(int height, int width)
		{
			height_ = height;
			width_ = width;
			grid_ = new T[height, width];
		}

		public Grid(T[,] data)
		{
			height_ = data.GetLength(0);
			width_ = data.GetLength(1);
			grid_ = data;
		}

		public Grid(int height, int width, Func<int, int, T> initialize)
			: this(height, width)
		{
			for (int i = 0; i < height_; i++) {
				for (int j = 0; j < width_; j++) {
					grid_[i, j] = initialize(i, j);
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
			for (int i = 0; i < height_; i++) {
				for (int j = 0; j < width_; j++) {
					if (should(grid_[i, j]) == false) {
						continue;
					}

					action(i, j);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void DoIn2(int i, int j, Action<int, int> action)
		{
			for (int dn = 0; dn < 2; ++dn) {
				int d2i = i + delta2_[dn];
				int d2j = j + delta2_[dn + 1];
				if ((uint)d2i < (uint)height_ && (uint)d2j < (uint)width_) {
					action(d2i, d2j);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void DoIn4(int i, int j, Action<int, int> action)
		{
			for (int dn = 0; dn < 4; ++dn) {
				int d4i = i + delta4_[dn];
				int d4j = j + delta4_[dn + 1];
				if ((uint)d4i < (uint)height_ && (uint)d4j < (uint)width_) {
					action(d4i, d4j);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void DoIn8(int i, int j, Action<int, int> action)
		{
			for (int dn = 0; dn < 8; ++dn) {
				int d8i = i + delta8_[dn];
				int d8j = j + delta8_[dn + 1];
				if ((uint)d8i < (uint)height_ && (uint)d8j < (uint)width_) {
					action(d8i, d8j);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int ToIndex(int i, int j) => i * width_ + j;
	}
}

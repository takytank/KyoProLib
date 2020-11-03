using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace TakyTank.KyoProLib.CS8
{
	public class Imos2D
	{
		private readonly long[,] map_;
		private readonly int w_;
		private readonly int h_;

		public long this[int y, int x] => map_[y, x];
		public Imos2D(int h, int w)
		{
			w_ = w;
			h_ = h;
			map_ = new long[h + 1, w + 1];
		}

		public void Add(int y1, int x1, int y2, int x2, long value)
		{
			x2++;
			y2++;
			map_[y1, x1] += value;
			map_[y1, x2] -= value;
			map_[y2, x1] -= value;
			map_[y2, x2] += value;
		}

		public void Build()
		{
			for (int i = 0; i < h_; i++) {
				for (int j = 0; j < w_; j++) {
					map_[i, j + 1] += map_[i, j];
				}
			}

			for (int i = 0; i < h_; i++) {
				for (int j = 0; j < w_; j++) {
					map_[i + 1, j] += map_[i, j];
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long Query(Range row, Range column)
			=> Query(row.Start.Value, column.Start.Value, row.End.Value, column.End.Value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long Query(int h1, int w1, int h2, int w2)
		{
			h1--;
			w1--;

			long ret = map_[h2, w2];
			if (h1 >= 0) {
				ret -= map_[h1, w2];
			}

			if (w1 >= 0) {
				ret -= map_[h2, w1];
			}

			if (h1 >= 0 && w1 >= 0) {
				ret += map_[h1, w1];
			}

			return ret;
		}
	}

	public class Imos2D<T> where T : struct
	{
		private readonly T[,] map_;
		private readonly int w_;
		private readonly int h_;
		private readonly Func<T, T, T> add_;
		private readonly Func<T, T, T> subtract_;

		public T this[int y, int x] => map_[y, x];
		public Imos2D(
			int h, int w, T defaultValue, Func<T, T, T> add, Func<T, T, T> subtract)
		{
			w_ = w;
			h_ = h;
			map_ = new T[h + 1, w + 1];
			MemoryMarshal.CreateSpan(ref map_[0, 0], map_.Length).Fill(defaultValue);
			add_ = add;
			subtract_ = subtract;
		}

		public void Add(int y1, int x1, int y2, int x2, T value)
		{
			x2++;
			y2++;
			map_[y1, x1] = add_(map_[y1, x1], value);
			map_[y1, x2] = subtract_(map_[y1, x2], value);
			map_[y2, x1] = subtract_(map_[y2, x1], value);
			map_[y2, x2] = add_(map_[y2, x2], value);
		}

		public void Build()
		{
			for (int i = 0; i < h_; i++) {
				for (int j = 0; j < w_; j++) {
					map_[i, j + 1] = add_(map_[i, j + 1], map_[i, j]);
				}
			}

			for (int i = 0; i < h_; i++) {
				for (int j = 0; j < w_; j++) {
					map_[i + 1, j] = add_(map_[i + 1, j], map_[i, j]);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Query(Range row, Range column)
			=> Query(row.Start.Value, column.Start.Value, row.End.Value, column.End.Value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Query(int h1, int w1, int h2, int w2)
		{
			h1--;
			w1--;

			T ret = map_[h2, w2];
			if (h1 >= 0) {
				ret = subtract_(ret, map_[h1, w2]);
			}

			if (w1 >= 0) {
				ret = subtract_(ret, map_[h2, w1]);
			}

			if (h1 >= 0 && w1 >= 0) {
				ret = add_(ret, map_[h1, w1]);
			}

			return ret;
		}
	}
}

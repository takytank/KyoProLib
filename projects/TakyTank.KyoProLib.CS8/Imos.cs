using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace TakyTank.KyoProLib.CS8
{
	public class Imos1D
	{
		private readonly long[] raw_;
		private readonly long[] built_;
		private readonly int n_;

		public long this[int index] => built_[index + 1];
		public long Max() => built_[1..].Max();
		public long Min() => built_[1..].Min();

		public Imos1D(int n)
		{
			n_ = n;
			raw_ = new long[n + 1];
			built_ = new long[n + 1];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(Range range, long value) => Add(range.Start.Value, range.End.Value, value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(int l, int r, long value)
		{
			if (l >= r || l >= n_ || r <= 0) {
				return;
			}

			raw_[l] += value;
			raw_[r] -= value;
		}

		public void Build()
		{
			built_[0] = 0;
			for (int i = 0; i < n_; i++) {
				built_[i + 1] += raw_[i] + built_[i];
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long Query(Range range) => Query(range.Start.Value, range.End.Value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long Query(int l, int r)
		{
			if (l >= r || l >= n_ || r <= 0) {
				return 0;
			}

			return built_[r] - built_[l];
		}
	}

	public class Imos1D<T> where T : struct
	{
		private readonly T[] raw_;
		private readonly T[] built_;
		private readonly int n_;
		private readonly T defaultValue_;
		private readonly Func<T, T, T> add_;
		private readonly Func<T, T, T> subtract_;

		public T this[int index] => built_[index + 1];

		public T Max() => built_.Max();
		public T Min() => built_.Min();

		public Imos1D(int n, T defaultValue, Func<T, T, T> add, Func<T, T, T> subtract)
		{
			n_ = n;
			defaultValue_ = defaultValue;
			raw_ = new T[n + 1];
			raw_.AsSpan().Fill(defaultValue);
			built_ = new T[n + 1];
			built_.AsSpan().Fill(defaultValue);
			add_ = add;
			subtract_ = subtract;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(Range range, T value) => Add(range.Start.Value, range.End.Value, value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(int l, int r, T value)
		{
			if (l >= r || l >= n_ || r <= 0) {
				return;
			}

			raw_[l] = add_(raw_[l], value);
			raw_[r] = subtract_(raw_[r], value);
		}

		public void Build()
		{
			built_[0] = defaultValue_;
			for (int i = 0; i < n_; i++) {
				built_[i + 1] = add_(built_[i + 1], add_(raw_[i], built_[i]));
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Query(Range range) => Query(range.Start.Value, range.End.Value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Query(int l, int r)
		{
			if (l >= r || l >= n_ || r <= 0) {
				return defaultValue_;
			}

			return subtract_(built_[r], built_[l]);
		}
	}

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

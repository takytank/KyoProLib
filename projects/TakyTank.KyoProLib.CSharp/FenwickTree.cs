using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V8
{
	public class FenwickTree
	{
		public static long InversionNumber(IReadOnlyList<int> numbers)
			=> InversionNumber(numbers, numbers.Max());
		public static long InversionNumber(IReadOnlyList<int> numbers, int max)
		{
			int n = numbers.Count;
			var bit = new FenwickTree(max + 1);
			long ret = 0;
			for (int i = 0; i < n; i++) {
				ret += i - bit.Sum(numbers[i]);
				bit.Add(numbers[i], 1);
			}

			return ret;
		}

		private readonly int n_;
		private readonly long[] bit_;

		public long this[int index]
		{
			get => Sum(index, index + 1);
			set => Add(index, value);
		}

		public FenwickTree(int count)
		{
			n_ = count;
			bit_ = new long[count + 1];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(int index, long value)
		{
			++index;
			while (index <= n_) {
				bit_[index] += value;
				index += index & -index;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long Sum(int index)
		{
			++index;
			long sum = 0;
			while (index > 0) {
				sum += bit_[index];
				index -= index & -index;
			}

			return sum;
		}

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//public long Sum(Range range) => Sum(range.Start.Value, range.End.Value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long Sum(int l, int r) => Sum(r - 1) - Sum(l - 1);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int LowerBound(long w)
		{
			int m = n_ + 1;
			if (w <= 0) {
				return -1;
			} else {
				int x = 0;
				int r = 1;
				while (r < m) {
					r <<= 1;
				}

				for (int d = r; d > 0; d >>= 1) {
					if (x + d < m && bit_[x + d] < w) {
						w -= bit_[x + d];
						x += d;
					}
				}

				return x;
			}
		}

		public IEnumerator<long> GetEnumerator()
		{
			for (int i = 0; i < n_; i++) {
				yield return this[i];
			}
		}
	}

	public class ModFenwickTree
	{
		private readonly int n_;
		private readonly ModInt[] bit_;

		public ModInt this[int index]
		{
			get => Sum(index, index + 1);
			set => Add(index, value);
		}

		public ModFenwickTree(int count)
		{
			n_ = count;
			bit_ = new ModInt[count + 1];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(int index, ModInt value)
		{
			++index;
			while (index <= n_) {
				bit_[index] += value;
				index += index & -index;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ModInt Sum(int index)
		{
			++index;
			ModInt sum = 0;
			while (index > 0) {
				sum += bit_[index];
				index -= index & -index;
			}

			return sum;
		}

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//public ModInt Sum(Range range) => Sum(range.Start.Value, range.End.Value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ModInt Sum(int l, int r) => Sum(r - 1) - Sum(l - 1);

		public IEnumerator<ModInt> GetEnumerator()
		{
			for (int i = 0; i < n_; i++) {
				yield return this[i];
			}
		}
	}

	public class RangeFenwickTree
	{
		private readonly int n_;
		private readonly long[,] bit_;

		public long this[int index]
		{
			get => Sum(index, index + 1);
			set => Add(index, index + 1, value);
		}

		public RangeFenwickTree(int count)
		{
			n_ = count;
			bit_ = new long[2, n_ + 1];
		}

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//public void Add(Range range, long value) => Add(range.Start.Value, range.End.Value, value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(int l_0, int r_0, long value)
		{
			++l_0;
			++r_0;
			AddCore(0, l_0, -value * (l_0 - 1));
			AddCore(1, l_0, value);
			AddCore(0, r_0, value * (r_0 - 1));
			AddCore(1, r_0, -value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void AddCore(int p, int a_1, long value)
		{
			for (int i = a_1; i <= n_; i += i & -i) {
				bit_[p, i] += value;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long Sum(int index_0)
		{
			++index_0;
			return SumCore(0, index_0) + SumCore(1, index_0) * index_0;
		}

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//public long Sum(Range range) => Sum(range.Start.Value, range.End.Value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long Sum(int l_0, int r_0) => Sum(r_0 - 1) - Sum(l_0 - 1);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private long SumCore(int p, int index_1)
		{
			long res = 0;
			for (int i = index_1; i > 0; i -= i & -i) {
				res += bit_[p, i];
			}

			return res;
		}

		public IEnumerator<long> GetEnumerator()
		{
			for (int i = 0; i < n_; i++) {
				yield return this[i];
			}
		}
	};

	public class ModRangeFenwickTree
	{
		private readonly int n_;
		private readonly ModInt[,] bit_;

		public ModInt this[int index]
		{
			get => Sum(index, index + 1);
			set => Add(index, index + 1, value);
		}

		public ModRangeFenwickTree(int count)
		{
			n_ = count;
			bit_ = new ModInt[2, n_ + 1];
		}

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//public void Add(Range range, ModInt value) => Add(range.Start.Value, range.End.Value, value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(int l_0, int r_0, ModInt value)
		{
			++l_0;
			++r_0;
			AddCore(0, l_0, 0 - value * (l_0 - 1));
			AddCore(1, l_0, value);
			AddCore(0, r_0, value * (r_0 - 1));
			AddCore(1, r_0, 0 - value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void AddCore(int p, int a_1, ModInt value)
		{
			for (int i = a_1; i <= n_; i += i & -i) {
				bit_[p, i] += value;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ModInt Sum(int index_0)
		{
			++index_0;
			return SumCore(0, index_0) + SumCore(1, index_0) * index_0;
		}

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//public ModInt Sum(Range range) => Sum(range.Start.Value, range.End.Value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ModInt Sum(int l_0, int r_0) => Sum(r_0 - 1) - Sum(l_0 - 1);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ModInt SumCore(int p, int index_1)
		{
			ModInt res = 0;
			for (int i = index_1; i > 0; i -= i & -i) {
				res += bit_[p, i];
			}

			return res;
		}

		public IEnumerator<ModInt> GetEnumerator()
		{
			for (int i = 0; i < n_; i++) {
				yield return this[i];
			}
		}
	};

	public class FenwickTree2D
	{
		private readonly int h_;
		private readonly int w_;
		private readonly long[,] bit_;
		public FenwickTree2D(int h, int w)
		{
			h_ = h;
			w_ = w;
			bit_ = new long[h_ + 1, w_ + 1];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(int h, int w, long x)
		{
			++h;
			++w;
			for (int i = h; i <= h_; i += i & -i) {
				for (int j = w; j <= w_; j += j & -j) {
					bit_[i, j] += x;
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long Sum(int h, int w)
		{
			++h;
			++w;
			long sum = 0;
			for (int i = h; i > 0; i -= i & -i) {
				for (int j = w; j > 0; j -= j & -j) {
					sum += bit_[i, j];
				}
			}

			return sum;
		}

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//public long Query(Range row, Range column)
		//	=> Query(row.Start.Value, column.Start.Value, row.End.Value, column.End.Value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long Query(int h1, int w1, int h2, int w2)
			=> Sum(h2 - 1, w2 - 1)
				- Sum(h2 - 1, w1 - 1)
				- Sum(h1 - 1, w2 - 1)
				+ Sum(h1 - 1, w1 - 1);
	};

	public class ModFenwickTree2D
	{
		private readonly int h_;
		private readonly int w_;
		private readonly ModInt[,] bit_;
		public ModFenwickTree2D(int h, int w)
		{
			h_ = h;
			w_ = w;
			bit_ = new ModInt[h_ + 1, w_ + 1];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(int h, int w, long x)
		{
			++h;
			++w;
			for (int i = h; i <= h_; i += i & -i) {
				for (int j = w; j <= w_; j += j & -j) {
					bit_[i, j] += x;
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ModInt Sum(int h, int w)
		{
			++h;
			++w;
			ModInt sum = 0;
			for (int i = h; i > 0; i -= i & -i) {
				for (int j = w; j > 0; j -= j & -j) {
					sum += bit_[i, j];
				}
			}

			return sum;
		}

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//public ModInt Query(Range row, Range column)
		//	=> Query(row.Start.Value, column.Start.Value, row.End.Value, column.End.Value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ModInt Query(int h1, int w1, int h2, int w2)
			=> Sum(h2 - 1, w2 - 1)
				- Sum(h2 - 1, w1 - 1)
				- Sum(h1 - 1, w2 - 1)
				+ Sum(h1 - 1, w1 - 1);
	};
}

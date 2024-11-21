using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace TakyTank.KyoProLib.CSharp
{
	public class FenwickTree
	{
		/// <summary>
		/// 配列全体の転倒数をO(N*logN)で計算
		/// </summary>
		public static long InversionNumber(IReadOnlyList<int> numbers)
			=> InversionNumber(numbers, numbers.Max());
		public static long InversionNumber(IReadOnlyList<int> numbers, int max)
		{
			int n = numbers.Count;
			var bit = new FenwickTree(max + 1);
			long ret = 0;
			for (int i = 0; i < n; ++i) {
				// ここまでの数のうち、自分より大きいものの数を足す。
				// Sumが以下のものの数を返すので、補集合の数を計算。
				ret += i - bit.Sum(numbers[i]);
				bit.Add(numbers[i], 1);
			}

			return ret;
		}

		public static long[] InversionNumbers(IReadOnlyList<int> numbers, int k)
			=> InversionNumbers(numbers, k, numbers.Max());
		/// <summary>
		/// 連続する長さKの区間ごとの転倒数をO(N*logN)で計算
		/// </summary>
		public static long[] InversionNumbers(IReadOnlyList<int> numbers, int k, int max)
		{
			int n = numbers.Count;
			int count = n - k + 1;

			var bit = new FenwickTree(max + 1);
			long ret = 0;
			for (int i = 0; i < k - 1; ++i) {
				ret += i - bit.Sum(numbers[i]);
				bit.Add(numbers[i], 1);
			}

			var rets = new long[count];
			for (int i = 0; i < count; i++) {
				int j = i + k - 1;
				// jを含めない直近K-1個のうち、num[j]より大きいものの数を足す。
				// Sumが以下のものの数を返すので、補集合の数を計算。
				ret += k - 1 - bit.Sum(numbers[j]);
				bit.Add(numbers[j], 1);

				rets[i] = ret;

				// jを含めた直近K-1個のうち、num[i]より小さいものの数を引く。
				// 自分を含めないように先にBITから外しておく。
				bit.Add(numbers[i], -1);
				// 以下ではなく未満の数を数えられるように-1。
				ret -= bit.Sum(numbers[i] - 1);
			}

			return rets;
		}

		// bitのi番目の要素は、iの最下位bitに対応する長さ、かつ、iを右端とした閉区間に対応する。
		// 内部的には1-indexedで考え、0番目は使用しない。
		// bit[1] : [1, 1]
		// bit[2] : [1, 2]
		// bit[3] : [3, 3]
		// bit[4] : [1, 4]
		private readonly int _n;
		private readonly long[] _bit;

		public long this[int index]
		{
			get => Sum(index, index + 1);
			// 代入ではなくて加算なのが非直感的で間違えやすいのでsetは実装しない
			// set => Add(index, value);
		}

		public FenwickTree(int count)
		{
			_n = count;
			_bit = new long[count + 1];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(int index, long value)
		{
			// 最初に1-indexedにしてから処理。
			++index;
			// 元のindexを含む区間全てに加算したい。
			// これは下のbitから順次繰り上げていくことで実現可能。
			while (index <= _n) {
				_bit[index] += value;
				index += index & -index;
			}
		}

		/// <summary>
		/// 閉区間[0, index]の和をO(longN)で計算
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long Sum(int index)
		{
			// 最初に1-indexedにしてから処理。
			++index;
			// 元のindexの区間から繋がる1を含む区間までの和を計算。
			// これは下のbitから順次落としていくことで実現可能。
			long sum = 0;
			while (index > 0) {
				sum += _bit[index];
				index -= index & -index;
			}

			return sum;
		}

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//public long Sum(Range range) => Sum(range.Start.Value, range.End.Value);
		/// <summary>
		/// 半開区間[L, R)の和をO(logN)で計算
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long Sum(int l, int r) => Sum(r - 1) - Sum(l - 1);

		/// <summary>
		/// [0, x]の和がW以上になる最小のXをO(longN)で返す
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int LowerBound(long w)
		{
			int m = _n + 1;
			if (w <= 0) {
				return -1;
			} else {
				int x = 0;
				int r = 1;
				while (r < m) {
					r <<= 1;
				}

				for (int d = r; d > 0; d >>= 1) {
					if (x + d < m && _bit[x + d] < w) {
						w -= _bit[x + d];
						x += d;
					}
				}

				return x;
			}
		}

		public IEnumerator<long> GetEnumerator()
		{
			for (int i = 0; i < _n; i++) {
				yield return this[i];
			}
		}
	}

	public class ModFenwickTree
	{
		private readonly int _n;
		private readonly ModInt[] _bit;

		public ModInt this[int index]
		{
			get => Sum(index, index + 1);
			set => Add(index, value);
		}

		public ModFenwickTree(int count)
		{
			_n = count;
			_bit = new ModInt[count + 1];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(int index, ModInt value)
		{
			++index;
			while (index <= _n) {
				_bit[index] += value;
				index += index & -index;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ModInt Sum(int index)
		{
			++index;
			ModInt sum = 0;
			while (index > 0) {
				sum += _bit[index];
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
			for (int i = 0; i < _n; i++) {
				yield return this[i];
			}
		}
	}

	public class RangeFenwickTree
	{
		private readonly int _n;
		private readonly long[,] _bit;

		public long this[int index]
		{
			get => Sum(index, index + 1);
			set => Add(index, index + 1, value);
		}

		public RangeFenwickTree(int count)
		{
			_n = count;
			_bit = new long[2, _n + 1];
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
			for (int i = a_1; i <= _n; i += i & -i) {
				_bit[p, i] += value;
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
				res += _bit[p, i];
			}

			return res;
		}

		public IEnumerator<long> GetEnumerator()
		{
			for (int i = 0; i < _n; i++) {
				yield return this[i];
			}
		}
	};

	public class ModRangeFenwickTree
	{
		private readonly int _n;
		private readonly ModInt[,] _bit;

		public ModInt this[int index]
		{
			get => Sum(index, index + 1);
			set => Add(index, index + 1, value);
		}

		public ModRangeFenwickTree(int count)
		{
			_n = count;
			_bit = new ModInt[2, _n + 1];
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
			for (int i = a_1; i <= _n; i += i & -i) {
				_bit[p, i] += value;
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
				res += _bit[p, i];
			}

			return res;
		}

		public IEnumerator<ModInt> GetEnumerator()
		{
			for (int i = 0; i < _n; i++) {
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

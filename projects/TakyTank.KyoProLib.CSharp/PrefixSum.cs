using System;
using System.Collections.Generic;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	/// <summary>
	/// 前からの累積和を取るクラス
	/// </summary>
	public class PrefixSum
	{
		/// <summary>元の配列の長さ</summary>
		private readonly int _n;
		/// <summary>Build前は元の配列の値が、Build後は累積和の値が格納されている。</summary>
		/// <remarks>
		/// sumの計算でのif文を無くすために、元の配列のindexとは1ズレていて、1-basedで格納されている。
		/// 累積後は以下の様になる。
		/// [0] : ダミー(長さ0の区間の和=0に相当)
		/// [1] : [0, 0]の和
		/// [2] : [0, 1]の和
		/// [3] : [0, 2]の和
		/// :
		/// [N-1] : [0, N-2]の和
		/// [N] : [0, N-1]の和
		/// </remarks>
		private readonly long[] _values;

		public long this[int index]
		{
			// 元の配列のindexとは1ズレている
			set => _values[index + 1] = value;
			get => _values[index + 1];
		}

		/// <summary>
		/// 配列の長さだけを指定してインスタンスを生成
		/// </summary>
		/// <param name="n">配列長</param>
		public PrefixSum(int n)
		{
			_n = n;
			_values = new long[n + 1];
		}

		/// <summary>
		/// 累積前のインスタンスを指定してインスタンスを生成
		/// </summary>
		/// <param name="values">
		/// 累積したい配列。
		/// 中身がコピーされるので、累積和を取ったときに、ここで渡した配列自体には影響しない。
		/// </param>
		public PrefixSum(long[] values)
			: this(values.Length)
		{
			// 格納インデックスを1ずらしてコピー
			Array.Copy(values, 0, _values, 1, _n);
		}

		/// <summary>
		/// 累積和を取る
		/// </summary>
		/// <remarks>
		/// 2回以上呼び出した場合、呼び出す度に累積が行われる。
		/// </remarks>
		public void Build()
		{
			for (int i = 0; i < _n; i++) {
				_values[i + 1] += _values[i];
			}
		}

		/// <summary>
		/// 半開区間[L, R)の区間和をO(1)で計算
		/// </summary>
		/// <param name="l">区間の左端インデックス</param>
		/// <param name="r">区間の右端インデックス</param>
		/// <returns>区間和</returns>
		public long Sum(int l, int r)
		{
			// 区間[0, R)は [0, L) + [L, R) であるため、[L, R)の和は sum[0, R) - sum[0, L) で計算出来る。
			// [0, R)は半開区間なのでその和は[0, R-1]の和と等しくなるが、
			// 値を格納している配列のインデックスは1つズレているので、配列に渡す値は結局Rそのままになる。
			// Lも同様。
			return _values[r] - _values[l];
		}

		/// <summary>
		/// 区間和が最大となる区間と和をO(N)で求める
		/// </summary>
		/// <returns>
		/// l, r -> 和が最大となる半開区間[L, R)
		/// sum -> 区間和
		/// </returns>
		public (int l, int r, long sum) CalculateMaxSum()
		{
			// 全ての区間和を計算するとO(N^2)になるが、
			// 区間の右端を決め打ったときの最適な左端をO(1)で求めることで、全体としてO(N)にする。
			// Sumの実装から分かるとおり、[0, L)までの和を最小にすれば[L, R)が最大になる。
			// つまり、左から順番にRを決め打っていくときに、和が最小の位置を保持しつつ計算していけばよい。

			// 何も引かないことで0は達成できるので、初期値は0。
			long min = 0;
			int minIndex = -1;
			int l = 0;
			int r = 0;
			long max = long.MinValue;
			for (int i = 0; i < _n; ++i) {
				long value = _values[i + 1];
				if (value - min > max) {
					max = value - min;
					l = minIndex + 1; // 最小の箇所は引かれる部分なので、求めたい区間として有効なのはその1個右から
					r = i;
				}

				if (min > value) {
					minIndex = i;
					min = value;
				}
			}

			// 半開区間にするための+1
			++r;

			return (l, r, max);
		}

		/// <summary>
		/// 区間和が最小となる区間と和をO(N)で求める
		/// </summary>
		/// <returns>
		/// l, r -> 和が最小となる半開区間[L, R)
		/// sum -> 区間和
		/// </returns>
		public (int l, int r, long sum) CalculateMinSum()
		{
			// CalculateMaxSumと同様に和が最大の位置を保持しつつ計算する。
			long max = 0;
			int maxIndex = -1;
			int l = 0;
			int r = 0;
			long min = long.MaxValue;
			for (int i = 0; i < _n; ++i) {
				long value = _values[i + 1];
				if (value - max < min) {
					min = value - max;
					l = maxIndex + 1;
					r = i;
				}

				if (max < value) {
					maxIndex = i;
					max = value;
				}
			}

			++r;

			return (l, r, min);
		}
	}
}

public class SuffixSum
{
	private readonly int _n;
	private readonly long[] _sums;

	public long this[int index]
	{
		set => _sums[index] = value;
		get => _sums[index];
	}

	public SuffixSum(int n)
	{
		_n = n;
		_sums = new long[n + 1];
	}

	public SuffixSum(long[] values)
		: this(values.Length)
	{
		for (int i = 0; i < _n; i++) {
			_sums[i] = values[i];
		}
	}

	public void Build()
	{
		for (int i = _n - 1; i >= 0; i--) {
			_sums[i] += _sums[i + 1];
		}
	}

	public long Sum(int l, int r)
	{
		return _sums[l] - _sums[r];
	}
}
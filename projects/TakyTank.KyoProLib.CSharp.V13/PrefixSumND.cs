using System.Runtime.Intrinsics.X86;

namespace TakyTank.KyoProLib.CSharp.V13;

/// <summary>
/// 前からのN次元累積和を取るクラス
/// </summary>
public class PrefixSumND
{
	/// <summary>累積和の配列全体の長さ</summary>
	private readonly int _n;
	/// <summary>各次元の長さ</summary>
	private readonly int[] _lengths;
	/// <summary>各次元のインデックスを1増やした時に1次元にならしたときのインデックスがどのくらい増えるか</summary>
	/// <remarks>
	/// 2次元のときのインデックス計算で i * w + j とやっているときの w に相当するものを各次元に対して求めている。
	/// 1次元にならしたときに後ろの次元から順に計算しているので、前の方が値が大きい。
	/// </remarks>
	private readonly int[] _sizes;

	/// <summary>Build前は元の配列の値が、Build後は累積和の値が格納されている。</summary>
	private readonly long[] _values;

	public long this[int[] indexes]
	{
		set => _values[ToIndex(indexes)] = value;
		get => _values[ToIndex(indexes)];
	}

	public PrefixSumND(int[] lengths)
	{
		_lengths = lengths;
		// 領域和の計算時の分岐を減らすために、各次元ごとに+1のサイズを確保しておく。
		// 多次元配列の次元数を動的に変えるのは無理なので、
		// 多次元を1次元にならして配列で管理する。
		// インデックスアクセスもそっちの方が速いのでよい。
		int n = 1;
		foreach (var l in lengths) {
			n *= (l + 1);
		}

		_n = n;
		_values = new long[n];

		int d = lengths.Length;
		_sizes = new int[d];
		_sizes[^1] = 1;
		for (int i = d - 2; i >= 0; i--) {
			_sizes[i] = _sizes[i + 1] * (lengths[i + 1] + 1);
		}
	}

	/// <summary>
	/// 累積和を取る
	/// </summary>
	/// <remarks>2回以上呼び出した場合、呼び出す度に累積が行われる。</remarks>
	public void Build()
	{
		// 高速ゼータ変換。
		// すなわち、各次元ごとに累積和を取ることで、全体の累積和を計算。
		// 次元数が固定では無いため、多重ループ部分はDFSを使う。
		int d = _lengths.Length;
		var loops = new int[d];
		var srcs = new int[d];
		var dsts = new int[d];
		// 現在累積和を取っている次元かどうかの判定を高速に行う為に、数値では無くビットでループを回す。
		for (int b = 1; b < (1 << d); b <<= 1) {
			// 累積を取る次元は arr[i + 1] += arr[i] + v の計算をするので1少なく、それ以外は全体を回す。
			for (int k = 0; k < d; k++) {
				loops[k] = _lengths[k] + 1 - (((b >> k) & 1));
			}

			Dfs(0);

			void Dfs(int depth)
			{
				if (depth == d) {
					_values[ToIndexInternal(dsts)] += _values[ToIndexInternal(srcs)];
					return;
				}

				for (int i = 0; i < loops[depth]; i++) {
					srcs[depth] = i;
					dsts[depth] = i + ((b >> depth) & 1);
					Dfs(depth + 1);
				}
			}
		}
	}

	/// <summary>
	/// 各次元について半開区間[L, R)を満たす領域の和をO(2^D)で計算
	/// </summary>
	/// <param name="lefts">各次元における区間の左端インデックス</param>
	/// <param name="rights">各次元における区間の右端インデックス</param>
	/// <returns></returns>
	public long Sum(int[] lefts, int[] rights)
	{
		int d = _lengths.Length;
		for (int k = 0; k < d; k++) {
			if (lefts[k] >= rights[k]) {
				return 0;
			}
		}

		long ret = 0;
		var indexes = new int[d];
		// 累積和から領域和を計算するときの基本は [0, R) から [0, L) を引くことである。
		// これを各次元で考えないといけない。
		// 多次元の累積和から領域和を計算する場合、2次元の例を一般化し、重複部分を考慮するために包除を行う必要がある。
		// そのため、各次元についてleftの場合とrightの場合の2^D通りを計算する必要がある。
		for (int f = 0; f < 1 << d; f++) {
			// 包除の符号は、全部のインデックスがright側のときに+で、そこから一つleft側に変わる度に正負が入れ替わる。
			int sign = ((d - Popcnt.PopCount((uint)f)) & 1) == 0 ? 1 : -1;
			for (int k = 0; k < d; k++) {
				// ビットが立っている方をright側とする。
				// 元のインデックスで考えたとき、Rが半開区間であることと、Lよりも小さい領域の和を引かないといけないことから、
				// arr[R - 1] - arr[L - 1] を計算する必要がある。
				// しかし、累積和を計算した配列におけるインデックスは、元のインデックスと1つズレているため、
				// 結局 arr[R] - arr[L] を計算、すなわち、インデックスをそのまま使用すればよい。
				indexes[k] = ((f & (1 << k)) != 0) ? rights[k] : lefts[k];
			}

			long temp = sign * _values[ToIndexInternal(indexes)];
			ret += temp;
		}

		return ret;
	}

	/// <summary>
	/// 多次元配列を1次元配列にならしたときのインデックスを計算
	/// </summary>
	/// <param name="indexes">各次元のインデックス(外部から指定された元のindex)</param>
	/// <returns>1次元配列のインデックス</returns>
	private int ToIndex(int[] indexes)
	{
		// 次元数は合っている前提でチェックを飛ばす
		int index = 0;
		for (int i = 0; i < _sizes.Length; i++) {
			// 累積和配列上でのindexは、元の配列のindexとは1ズレている
			index += _sizes[i] * (indexes[i] + 1);
		}

		return index;
	}

	/// <summary>
	/// 多次元配列を1次元配列にならしたときのインデックスを計算(内部計算用)
	/// </summary>
	/// <param name="indexes">各次元のインデックス</param>
	/// <returns>1次元配列のインデックス</returns>
	private int ToIndexInternal(int[] indexes)
	{
		// 次元数は合っている前提でチェックを飛ばす
		int index = 0;
		for (int i = 0; i < _sizes.Length; i++) {
			index += _sizes[i] * indexes[i];
		}

		return index;
	}
}

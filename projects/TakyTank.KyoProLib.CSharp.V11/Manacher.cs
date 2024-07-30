using System.Numerics;

namespace TakyTank.KyoProLib.CSharp.V11;

/// <summary>回文検出アルゴリズム</summary>
public class Manacher<T> where T : INumber<T>
{
	private readonly int[] _info;
	/// <summary>
	/// 以下の様に回文の最大長を格納した長さ2N-1の配列。
	/// 偶数番目 -> 元の文字列の i/2 番目の文字を中心とした奇数長の回文の長さ。
	/// 奇数番目 -> 元の文字列の i/2 番目と i/2 + 1 番目の間を中心とした偶数長の回文の長さ。
	/// </summary>
	public ReadOnlySpan<int> Info => _info;

	/// <summary>連続する部分文字列のうち最長の回文の長さ</summary>
	public int MaxSize { get; private set; }
	/// <summary>最長の回文となる半開区間 [L, R)</summary>
	public int MaxL { get; private set; }
	public int MaxR { get; private set; }

	/// <summary>
	/// 回文判定を行う文字列を渡してインスタンスを生成
	/// </summary>
	public Manacher(ReadOnlySpan<T> values, T delimiter)
	{
		_info = DetectPalindrome(values, delimiter);
		MaxSize = 0;
		for (int i = 0; i < _info.Length; i++) {
			if (_info[i] > MaxSize) {
				MaxSize = _info[i];
				// インデックスiに対応した文字列上の位置Cは i / 2 である。
				// infoに格納されている回文長をDとした時、片側に伸びる距離は (D - 1) / 2 なので、
				// L = i / 2 - (D - 1) / 2, R = i / 2 + (D - 1) / 2 となる。
				// 回文長が偶数のときiも偶数で、切り捨て分Rがズレるが、
				// 先に分子を足してから2で割ると0.5+0.5の部分が繰り上がるので大丈夫になる。
				int d = _info[i] - 1;
				MaxL = (i - d) >> 1;
				MaxR = ((i + d) >> 1) + 1; // 最後の+1は半開区間にするため。
			}
		}
	}

	/// <summary>指定したインデックスを中心とした回文のサイズ</summary>
	public int GetSize(int i) => _info[i << 1];

	/// <summary>
	/// 半開区間 [L, R) が回文かどうかを返す
	/// </summary>
	public bool IsPalindrome(int l, int r)
	{
		int length = r - l;
		--r;
		if (r < l || (uint)l >= _info.Length || (uint)r >= _info.Length) {
			return false;
		}

		// 閉区間 (L, R) の中心(偶数の場合は切り捨て)のインデックスは C = (L + R) / 2 となる。
		// 最大長の情報配列は中心インデックスに対して、その文字そのものと、
		// 次の文字との間の、2倍の情報を保持している。
		// そのため、情報のインデックスに合わせるにはCを2倍する。そして、偶数長の場合は+1する。
		// つまり、最初から切り捨て無しで L + R だけでよい。
		return _info[l + r] >= length;
	}

	/// <summary>
	/// 指定した文字列の各位置、及び、文字関を中心とした回文の最大長を返す
	/// </summary>
	/// <param name="values">文字列</param>
	/// <param name="delimiter">区切りに使う文字。元の文字列に含まれていない文字なら何でも良よい。</param>
	/// <returns>
	/// 以下の様に回文の最大長を格納した配列を返す。
	/// 偶数番目 -> 元の文字列の i/2 番目の文字を中心とした奇数長の回文の長さ。
	/// 奇数番目 -> 元の文字列の i/2 番目と i/2 + 1 番目の間を中心とした偶数長の回文の長さ。
	/// </returns>
	private static int[] DetectPalindrome(ReadOnlySpan<T> values, T delimiter)
	{
		// 各文字の間にデリミターを挟んだ文字列に対して回文検出をかけることで、
		// 偶数長の回文を検出出来る。
		int length = values.Length * 2 - 1;
		var t = new T[length];
		for (int i = 0; i < length; i++) {
			if ((i & 1) == 0) {
				t[i] = values[i >> 1];
			} else {
				t[i] = delimiter;
			}
		}

		var ret = DetectPalindromeCore(t);
		// 元の文字列における回文長に換算する
		// 偶数のことがあるため、半径ではなく回文長にする。
		for (int i = 0; i < length; i++) {
			if ((i & 1) == 0) {
				// 0-basedでの偶数番目は、元の文字列の各文字を中心とした奇数長の回文の半径が格納されている。
				// これを回文長に直すときには2をかけて1を引く。
				// デリミターが一致した分の回文長は元の文字列の回文の長さに含まないため、
				// デリミター込みでの半径が1のときに1で、3, 5, 7,,, と2増える度に元の回文長は2ずつ増えていくことになる。
				// つまり、半径が偶数のときだけ1を引けば良い。
				ret[i] = ((ret[i] & 1) == 0) ? ret[i] - 1 : ret[i];
			} else {
				// 同様に奇数番目は、各デリミターを中心とした奇数長の回文の最大長が格納されている。
				// デリミターが一致した分、及び、中心のデリミターは元の文字列の回文の長さに含まないため、
				// デリミター込みでの回文長が1のときに0で、2, 4, 6,,, と2増える度に元の回文長は2ずつ増えていくことになる。
				// つまり、奇数のときだけ1を引けばよく、これはbit0を落とせばよい。
				ret[i] = ret[i] & ~1;
			}
		}

		return ret;
	}

	/// <summary>
	/// 指定した文字列の各位置を中心とした回文の最大半径を返す
	/// </summary>
	/// <remarks>つまり、奇数長の回文のみを検出する。</remarks>
	/// <param name="values">文字列</param>
	/// <returns>i番目の文字を中心として、自分を含んで片側に何文字分まで回文が続くかの配列を返す。</returns>
	private static int[] DetectPalindromeCore(ReadOnlySpan<T> values)
	{
		var radiuses = new int[values.Length];
		int c = 0;
		int radius = 0;
		while (c < values.Length) {
			// cを中心に同じ文字がどこまで連続するか
			while (0 <= c - radius && c + radius < values.Length && values[c - radius] == values[c + radius]) {
				++radius;
			}

			radiuses[c] = radius;

			// 左から探索しているため、cより前の位置の半径は既に確定している。
			// また、cを中心に対象であるため、c - k を中心とした回文が [c - radius + 1, c + radius - 1] を超えない限り、
			// c + k を中心とした回文はc-kを中心とした回文と一致する。
			// c + radius の先の状況によっては c + k の側だけ後続に続く可能性があるので、
			// 有効な区間を前後1ずつ削らないといけないことに注意。
			int k = 1;
			while (0 <= c - k && k + radiuses[c - k] < radius) {
				radiuses[c + k] = radiuses[c - k];
				++k;
			}

			// c + k - 1 まで半径を確定させたので、その次の位置から探索を継続する。
			c += k;
			// radiusが0まで初期化されないkで上のループを抜けた場合、
			// それは少なくともradiusの範囲をはみ出る半径の回文があるということである。
			// 既に分かっている分の回文を再探索しないように、radiusからはkだけ引けばよい。
			radius -= k;
		}

		return radiuses;
	}
}

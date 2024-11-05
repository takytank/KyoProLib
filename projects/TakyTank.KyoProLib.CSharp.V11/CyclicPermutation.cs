namespace TakyTank.KyoProLib.CSharp.V11;

/// <summary>巡回置換</summary>
public class CyclicPermutation
{
	public int[] LoopNumbers { get; set; }
	public int[] Indexes { get; set; }
	public int[][] Loops { get; set; }

	/// <summary>
	/// 置換を指定してインスタンスを生成
	/// </summary>
	/// <param name="p">置換に対応する0からの順列をソートした配列</param>
	public CyclicPermutation(int[] p)
	{
		int n = p.Length;

		var loopNumbers = new int[n];
		loopNumbers.AsSpan().Fill(-1);
		var indexes = new int[n];
		var loops = new List<List<int>>();
		for (int i = 0; i < n; i++) {
			if (loopNumbers[i] >= 0) {
				continue;
			}

			int cur = i;
			loopNumbers[cur] = loops.Count;
			var loop = new List<int> { cur };
			while (loopNumbers[p[cur]] < 0) {
				int nex = p[cur];
				loop.Add(nex);
				cur = nex;
				loopNumbers[cur] = loops.Count;
			}

			for (int j = 0; j < loop.Count; j++) {
				int v = loop[j];
				loopNumbers[v] = loops.Count;
				indexes[v] = j;
			}

			loops.Add(loop);
		}

		LoopNumbers = loopNumbers;
		Indexes = indexes;
		Loops = new int[loops.Count][];
		for (int i = 0; i < loops.Count; i++) {
			Loops[i] = loops[i].ToArray();
		}
	}

	/// <summary>頂点Vを含むループを取得</summary>
	public int[] GetLoop(int v) => Loops[LoopNumbers[v]];

	/// <summary>頂点Vからcount先の頂点番号を返す</summary>
	public int Next(int v, long count)
	{
		var loop = GetLoop(v);
		// countにオーバーフローするほどのデカい数はこないはず
		return loop[(count + Indexes[v]) % loop.Length];
	}

	/// <summary>頂点Vからvalue^k先の頂点番号を返す</summary>
	public int Next(int v, long value, long k)
	{
		var loop = GetLoop(v);
		int size = loop.Length;

		value %= size;

		long target = 1;
		while (k > 0) {
			if ((k & 1) != 0) {
				target = target * value % size;
			}

			value = value * value % size;
			k >>= 1;
		}

		target = (target + Indexes[v]) % size;

		return loop[target];
	}
}

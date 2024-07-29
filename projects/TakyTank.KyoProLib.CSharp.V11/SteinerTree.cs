using System.Runtime.CompilerServices;

namespace TakyTank.KyoProLib.CSharp.V11;

/// <summary>シュタイナー木クラス</summary>
public class SteinerTree
{
	private const long INF = long.MaxValue / 10;

	/// <summary>頂点数</summary>
	private readonly int _n;
	/// <summary>グラフの隣接リスト</summary>
	private readonly (int to, long cost)[][] _edges;
	private readonly List<(int to, long cost)>[] _tempEdges;

	/// <summary>最小コスト計算用の配列</summary>
	private long[][] _dp;

	public SteinerTree(int n)
	{
		_n = n;
		_tempEdges = new List<(int to, long cost)>[_n];
		for (int i = 0; i < _n; i++) {
			_tempEdges[i] = new List<(int to, long cost)>();
		}

		_edges = new (int v, long d)[n][];
	}

	public void AddEdge2W(int u, int v, long cost)
	{
		_tempEdges[u].Add((v, cost));
		_tempEdges[v].Add((u, cost));
	}

	/// <summary>
	/// 辺の追加が終わった後に呼び出す
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Build()
	{
		// Listのまま持つより1回配列に直した方が、最適化が効く。
		// コピーの負担を差し引いても全体として速くなる。
		for (int i = 0; i < _edges.Length; i++) {
			_edges[i] = _tempEdges[i].ToArray();
		}
	}

	/// <summary>
	/// 最小シュタイナー木のコストを O(V * 3^t + (V+E) * 2^t * logV) で求める
	/// </summary>
	/// <remarks>
	/// ターミナル数が少ないとき用の実装。
	/// </remarks>
	/// <param name="terminal">ターミナル点の集合</param>
	/// <returns>
	/// ターミナル点全て + 各頂点を繋いだときの最小コストの配列。
	/// ターミナル点の頂点位置を見れば、ターミナル点のみの(不要な頂点を除いた)最小コストが分かる。
	/// </returns>
	public long[] CalculateMinCost(int[] terminal)
	{
		int t = terminal.Length;
		if (t == 0) {
			return new long[_n];
		}

		int tt = 1 << t;
		if (_dp is null || _dp.Length < tt) {
			// 大きい配列なので、複数回呼ばれたときのことを考え、
			// サイズが大きくなったときのみ作り直す。
			_dp = new long[tt][];
			for (int i = 0; i < tt; i++) {
				_dp[i] = new long[_n];
			}
		}

		for (int i = 0; i < tt; i++) {
			_dp[i].AsSpan().Fill(INF);
		}

		for (int i = 0; i < t; i++) {
			_dp[1 << i][terminal[i]] = 0;
		}

		var que = new PriorityQueue<int, long>();
		// 繋いだターミナル点をビットフラグとし、小さい方から埋めていく。
		// DP[F, V] : 繋いだターミナル点のフラグがFで、頂点Vも繋いだときの最小コスト。
		for (int f = 1; f < tt; f++) {
			for (int v = 0; v < _n; v++) {
				// fのサブビットを列挙。
				// subとfからsubを除いた集合をVを経由して繋いで更新する。
				for (int sub = f; sub > 0; sub = (sub - 1) & f) {
					_dp[f][v] = Math.Min(_dp[f][v], _dp[sub][v] + _dp[f ^ sub][v]);
				}
			}

			// ダイクストラでFに(主に)ターミナル点以外の頂点を繋いだときのコストを更新。
			for (int v = 0; v < _n; v++) {
				que.Enqueue(v, _dp[f][v]);
			}

			while (que.TryDequeue(out int v, out long cost)) {
				if (_dp[f][v] < cost) {
					continue;
				}

				foreach (var e in _edges[v]) {
					long nextCost = _dp[f][v] + e.cost;
					if (_dp[f][e.to] > nextCost) {
						_dp[f][e.to] = nextCost;
						que.Enqueue(e.to, nextCost);
					}
				}
			}
		}

		return _dp[tt - 1];
	}
}

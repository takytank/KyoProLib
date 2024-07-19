using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace TakyTank.KyoProLib.CSharp.V8
{
	/// <summary>木構造クラス</summary>
	public class Tree
	{
		/// <summary>頂点数</summary>
		private readonly int _n;
		/// <summary>グラフの隣接リスト</summary>
		private readonly (int v, long cost)[][] _edges;
		private readonly List<(int v, long cost)>[] _tempEdges;

		// 以下、ChangeRootで指定した頂点を根としたときの根付き木の情報

		/// <summary>木の高さは2^h以下</summary>
		private int _h;
		/// <summary>[i, j] = 頂点jの2^i個上の親の頂点番号</summary>
		/// <remarks>
		/// iとjが逆の方が直感的な気はするが、
		/// 実際の使いどころでは同じ高さの2頂点を同時に使うことが多いので、
		/// この順番の方がキャッシュが効きそう(多分)
		/// </remarks>
		private int[,] _parents;
		/// <summary>各頂点の深さ</summary>
		private int[] _depth;

		/// <summary>
		/// 頂点数を指定してインスタンスを生成
		/// </summary>
		/// <param name="n">頂点数</param>
		public Tree(int n)
		{
			_n = n;
			_tempEdges = new List<(int v, long d)>[n];
			for (int i = 0; i < n; i++) {
				_tempEdges[i] = new List<(int v, long d)>();
			}

			_edges = new (int v, long d)[n][];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge(int u, int v, long cost) => _tempEdges[u].Add((v, cost));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ReadOnlySpan<(int v, long cost)> Edges(int index) => _edges[index].AsSpan();

		/// <summary>
		/// 頂点Sから各頂点への最短距離を求める
		/// </summary>
		/// <param name="s">始点</param>
		/// <param name="inf">到達出来ない頂点の距離</param>
		/// <returns>
		/// distances -> 最短距離の配列
		/// prevs -> 最短経路における各頂点の前の頂点
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long[] distances, int[] prevs) ShortestPath(int s, long inf = long.MaxValue)
		{
			var distances = new long[_n];
			distances.AsSpan().Fill(inf);
			distances[s] = 0;
			var prevs = new int[_n].Fill(-1);
			var que = new Queue<int>();
			que.Enqueue(s);
			while (que.Count > 0) {
				var cur = que.Dequeue();
				foreach (var (to, d) in _edges[cur]) {
					long nextDistance = distances[cur] + d;
					if (nextDistance < distances[to]) {
						distances[to] = nextDistance;
						prevs[to] = cur;
						que.Enqueue(to);
					}
				}
			}

			return (distances, prevs);
		}

		/// <summary>
		/// 頂点Sから頂点Tへの最短経路を復元
		/// </summary>
		/// <param name="s">始点</param>
		/// <param name="t">終点</param>
		/// <param name="prevs">最短距離と一緒に求めた各頂点の前の頂点の情報</param>
		/// <returns>最短経路の頂点がSからTまで順番に並んでいる配列</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int[] GetPath(int s, int t, int[] prevs)
		{
			var path = new List<int> { t };
			int cur = t;
			while (cur != s) {
				cur = prevs[cur];
				path.Add(cur);
			}

			path.Reverse();
			return path.ToArray();
		}

		/// <summary>
		/// 木の直径を O(N) で計算
		/// </summary>
		/// <returns>
		/// diameter -> 直径の値
		/// path -> 直径のうちの一つの頂点が順番に並んでいる
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long diameter, int[] path) Diameter()
		{
			// 適当な頂点から一番遠い頂点(A)を求め、
			// Aから一番遠い頂点(B)を求めたときに、AとBを繋ぐパスが直径の一つとなる。
			var (firstDistances, _) = ShortestPath(0);
			long max = 0;
			int a = 0;
			for (int i = 0; i < _n; i++) {
				if (firstDistances[i] > max) {
					max = firstDistances[i];
					a = i;
				}
			}

			var (distances, prevs) = ShortestPath(a);
			max = 0;
			int b = 0;
			for (int i = 0; i < _n; i++) {
				if (distances[i] > max) {
					max = distances[i];
					b = i;
				}
			}

			// パスの方向はどっちからでもいいので、
			// 最後の Reverse を省くために GetPath の中身を直接書いているが、
			// 別にそこまで拘らなくてもいいかもしれない。
			long diameter = max;
			var tempPath = new List<int> { b };
			while (b != a) {
				b = prevs[b];
				tempPath.Add(b);
			}

			var path = tempPath.ToArray();
			return (diameter, path);
		}

		/// <summary>
		/// 木の重心を O(N) で計算
		/// </summary>
		/// <remarks>重心分解のように再帰的に重心は求めない。</remarks>
		/// <returns>重心となる頂点番号</returns>
		public int[] Centroid()
		{
			var centroids = new List<int>();
			int half = _n / 2;
			// 探索開始頂点はどれでもよい
			FindCentroidCore(0, -1);

			return centroids.ToArray();

			// vを根とする部分木のサイズを返す
			int FindCentroidCore(int v, int p = -1)
			{
				int subSize = 1;
				bool isCentroid = true;
				foreach (var (child, _) in _edges[v]) {
					if (child == p) {
						continue;
					}

					int childSize = FindCentroidCore(child, v);
					if (childSize > half) {
						// 子を根とする部分木に半分より大きいものがあったらVは重心ではない
						isCentroid = false;
					}

					subSize += childSize;
				}

				// 親方向の部分木が半分より大きかったらVは重心ではない
				if (_n - subSize > half) {
					isCentroid = false;
				}

				if (isCentroid) {
					centroids.Add(v);
				}

				return subSize;
			}
		}

		/// <summary>
		/// 木の根を決めて親や深さなどの探索を O(N*logN) で行う
		/// </summary>
		/// <param name="root">根とする頂点の番号</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ChangeRoot(int root)
		{
			int m = _n - 1;
			_h = 0;
			while (m > 0) {
				++_h;
				m >>= 1;
			}

			_parents = new int[_h, _n];
			_depth = new int[_n];

			_parents[0, root] = -1;
			_depth[root] = 0;

			// DFS をして深さと1個上の親を求める。
			// 再帰を避けるために stack を使う。
			var stack = new Stack<int>();
			stack.Push(root);
			while (stack.Count > 0) {
				var v = stack.Pop();
				int p = _parents[0, v];
				int nextDepth = _depth[v] + 1;
				foreach (var (next, _) in _edges[v]) {
					if (next == p) {
						continue;
					}

					_parents[0, next] = v;
					_depth[next] = nextDepth;
					stack.Push(next);
				}
			}

			// ダブリングで2の冪乗個上の親を求める
			for (int i = 0; i < _h - 1; i++) {
				for (int j = 0; j < _n; j++) {
					if (_parents[i, j] < 0) {
						_parents[i + 1, j] = -1;
					} else {
						_parents[i + 1, j] = _parents[i, _parents[i, j]];
					}
				}
			}
		}

		/// <summary>
		/// ChangeRoot で定めた根付き木における、頂点Uと頂点Vの最近共通祖先(LCA)を求める
		/// </summary>
		/// <returns>LCAの頂点番号</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Lca(int u, int v) => LowestCommonAnsestor(u, v);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int LowestCommonAnsestor(int u, int v)
		{
			// まずUとVの深さを揃える。深い方をVとする。
			if (_depth[u] > _depth[v]) {
				(u, v) = (v, u);
			}

			// 深さの差のbitが立っている部分を順番に消していくイメージ。
			for (int i = 0; i < _h; i++) {
				if (((_depth[v] - _depth[u]) & (1 << i)) != 0) {
					v = _parents[i, v];
				}
			}

			if (u == v) {
				return u;
			}

			// LCAの手前まで順番に、UV同時に親を辿っていく。その1個親がLCA。
			for (int i = _h - 1; i >= 0; i--) {
				if (_parents[i, u] == _parents[i, v]) {
					continue;
				}

				u = _parents[i, u];
				v = _parents[i, v];
			}

			return _parents[0, u];
		}
	}

	public class Tree<T>
	{
		private readonly int _n;
		private readonly List<(int v, long cost, T info)>[] _tempEdges;
		private readonly (int v, long cost, T info)[][] _edges;

		private int _k;
		private int[,] _parents;
		private int[] _depth;

		public Tree(int n)
		{
			_n = n;
			_tempEdges = new List<(int v, long cost, T info)>[n];
			for (int i = 0; i < n; i++) {
				_tempEdges[i] = new List<(int v, long cost, T info)>();
			}

			_edges = new (int v, long cost, T info)[n][];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge(int u, int v, long cost, T info) => _tempEdges[u].Add((v, cost, info));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge2W(int u, int v, long cost, T info)
		{
			_tempEdges[u].Add((v, cost, info));
			_tempEdges[v].Add((u, cost, info));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Build()
		{
			for (int i = 0; i < _edges.Length; i++) {
				_edges[i] = _tempEdges[i].ToArray();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ReadOnlySpan<(int v, long cost, T info)> Edges(int index) => _edges[index].AsSpan();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long[] distances, int[] prevs) ShortestPath(int start, long inf = long.MaxValue)
		{
			var distances = new long[_n];
			distances.AsSpan().Fill(inf);
			distances[start] = 0;
			var prevs = new int[_n].Fill(-1);
			var que = new Queue<int>();
			que.Enqueue(start);
			while (que.Count > 0) {
				var cur = que.Dequeue();
				foreach (var (to, d, _) in _edges[cur]) {
					long nextDistance = distances[cur] + d;
					if (nextDistance < distances[to]) {
						distances[to] = nextDistance;
						prevs[to] = cur;
						que.Enqueue(to);
					}
				}
			}

			return (distances, prevs);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int[] GetPath(int s, int t, int[] prevs)
		{
			var path = new List<int> { t };
			int cur = t;
			while (cur != s) {
				cur = prevs[cur];
				path.Add(cur);
			}

			path.Reverse();
			return path.ToArray();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long diameter, int[] path) Diameter()
		{
			var (firstDistances, _) = ShortestPath(0);
			long max = 0;
			int fi = 0;
			for (int i = 0; i < _n; i++) {
				if (firstDistances[i] > max) {
					max = firstDistances[i];
					fi = i;
				}
			}

			var (distances, prevs) = ShortestPath(fi);
			max = 0;
			int si = 0;
			for (int i = 0; i < _n; i++) {
				if (distances[i] > max) {
					max = distances[i];
					si = i;
				}
			}

			long diameter = max;
			var tempPath = new List<int> { si };
			while (si != fi) {
				si = prevs[si];
				tempPath.Add(si);
			}

			var path = tempPath.ToArray();
			return (diameter, path);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void InitializeLca(int root)
		{
			int m = _n - 1;
			_k = 0;
			while (m > 0) {
				++_k;
				m >>= 1;
			}

			_parents = new int[_k, _n];
			_depth = new int[_n];

			_parents[0, root] = -1;
			_depth[root] = 0;

			var stack = new Stack<int>();
			stack.Push(root);
			while (stack.Count > 0) {
				var v = stack.Pop();
				int p = _parents[0, v];
				int nextDepth = _depth[v] + 1;
				foreach (var (next, _, _) in _edges[v]) {
					if (next == p) {
						continue;
					}

					_parents[0, next] = v;
					_depth[next] = nextDepth;
					stack.Push(next);
				}
			}

			for (int i = 0; i < _k - 1; i++) {
				for (int j = 0; j < _n; j++) {
					if (_parents[i, j] < 0) {
						_parents[i + 1, j] = -1;
					} else {
						_parents[i + 1, j] = _parents[i, _parents[i, j]];
					}
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Lca(int u, int v) => LowestCommonAnsestor(u, v);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int LowestCommonAnsestor(int u, int v)
		{
			if (_depth[u] > _depth[v]) {
				(u, v) = (v, u);
			}

			for (int i = 0; i < _k; i++) {
				if (((_depth[v] - _depth[u]) & (1 << i)) != 0) {
					v = _parents[i, v];
				}
			}

			if (u == v) {
				return u;
			}

			for (int i = _k - 1; i >= 0; i--) {
				if (_parents[i, u] == _parents[i, v]) {
					continue;
				}

				u = _parents[i, u];
				v = _parents[i, v];
			}

			return _parents[0, u];
		}
	}
}

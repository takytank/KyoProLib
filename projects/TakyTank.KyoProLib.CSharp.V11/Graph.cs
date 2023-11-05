using System.Runtime.CompilerServices;

namespace TakyTank.KyoProLib.CSharp.V11
{
	public class Graph
	{
		private readonly int _n;
		private readonly List<(int v, int e, long cost)>[] _edges;
		private int _edgeCount;

		public IReadOnlyList<(int v, int e, long cost)>[] Edges => _edges;

		public Graph(int n)
		{
			_n = n;
			_edges = new List<(int v, int e, long cost)>[n];
			for (int i = 0; i < n; i++) {
				_edges[i] = new List<(int v, int e, long cost)>();
			}

			_edgeCount = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge(int u, int v, long cost) => _edges[u].Add((v, _edgeCount++, cost));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge2W(int u, int v, long cost)
		{
			_edges[u].Add((v, _edgeCount, cost));
			_edges[v].Add((u, _edgeCount, cost));
			++_edgeCount;
		}

		/// <summary>
		/// 二部グラフかどうかを判定 O(V + E)
		/// </summary>
		/// <returns>
		/// isBipartite -> true:二部グラフ
		/// colors -> 各頂点を2色に塗り分けたときの色(0 or 1)
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (bool isBipartite, int[] colors) CheckIskBipartite()
		{
			// 0と1の2色で塗り分ける。
			// 色が塗られていない(=未探索)のときに-1。
			var colors = new int[_n];
			colors.AsSpan().Fill(-1);

			bool dfs(int v, int color)
			{
				colors[v] = color;
				foreach (var next in _edges[v].AsSpan()) {
					if (colors[next.v] != -1) {
						if (colors[next.v] == color) {
							return false;
						}

						continue;
					}

					if (dfs(next.v, 1 - color) == false) {
						return false;
					}
				}

				return true;
			}

			bool isBipartite = true;
			for (int v = 0; v < _n; ++v) {
				if (colors[v] != -1) {
					continue;
				}

				if (dfs(v, 0) == false) {
					isBipartite = false;
				}
			}

			return (isBipartite, colors);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int[] vertexes, int[] edges) DetectCycle()
		{
			var used = new int[_n];
			var prev = new (int v, int e)[_n];
			for (int i = 0; i < _n; i++) {
				prev[i] = (-1, -1);
			}

			var cycleV = new List<int>();
			var cycleE = new List<int>();
			for (int i = 0; i < _n; i++) {
				if (used[i] != 0) {
					continue;
				}

				bool ok = Dfs(i);
				if (ok) {
					break;
				}
			}

			bool Dfs(int v)
			{
				used[v] = 1;
				foreach (var edge in _edges[v].AsSpan()) {
					if (edge.e == prev[v].e) {
						continue;
					}

					if (edge.v == v) {
						cycleE.Add(edge.e);
						cycleV.Add(v);
						return true;
					}

					if (used[edge.v] == 0) {
						prev[edge.v] = (v, edge.e);
						if (Dfs(edge.v)) {
							return true;
						}
					} else if (used[edge.v] == 1) {
						int cur = v;
						cycleE.Add(edge.e);
						cycleV.Add(v);
						while (cur != edge.v) {
							cycleE.Add(prev[cur].e);
							cycleV.Add(prev[cur].v);
							cur = prev[cur].v;
						}

						cycleE.Reverse();
						cycleV.Reverse();
						return true;
					}
				}

				used[v] = 2;
				return false;
			}

			return (cycleV.ToArray(), cycleE.ToArray());
		}
	}

	public class Graph<T>
	{
		private readonly int _n;
		private readonly List<(int v, int e, T item)>[] _edges;
		private int _edgeCount;

		public IReadOnlyList<(int v, int e, T item)>[] Edges => _edges;

		public Graph(int n)
		{
			_n = n;
			_edges = new List<(int v, int e, T item)>[n];
			for (int i = 0; i < n; i++) {
				_edges[i] = new List<(int v, int e, T item)>();
			}

			_edgeCount = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge(int u, int v, T item) => _edges[u].Add((v, _edgeCount++, item));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge2W(int u, int v, T item)
		{
			_edges[u].Add((v, _edgeCount, item));
			_edges[v].Add((u, _edgeCount, item));
			++_edgeCount;
		}

		/// <summary>
		/// 二部グラフかどうかを判定 O(V + E)
		/// </summary>
		/// <returns>
		/// isBipartite -> true:二部グラフ
		/// colors -> 各頂点を2色に塗り分けたときの色(0 or 1)
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (bool isBipartite, int[] colors) CheckIskBipartite()
		{
			// 0と1の2色で塗り分ける。
			// 色が塗られていない(=未探索)のときに-1。
			var colors = new int[_n];
			colors.AsSpan().Fill(-1);

			bool dfs(int v, int color)
			{
				colors[v] = color;
				foreach (var next in _edges[v].AsSpan()) {
					if (colors[next.v] != -1) {
						if (colors[next.v] == color) {
							return false;
						}

						continue;
					}

					if (dfs(next.v, 1 - color) == false) {
						return false;
					}
				}

				return true;
			}

			bool isBipartite = true;
			for (int v = 0; v < _n; ++v) {
				if (colors[v] != -1) {
					continue;
				}

				if (dfs(v, 0) == false) {
					isBipartite = false;
				}
			}

			return (isBipartite, colors);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int[] vertexes, int[] edges) DetectCycle()
		{
			var used = new int[_n];
			var prev = new (int v, int e)[_n];
			for (int i = 0; i < _n; i++) {
				prev[i] = (-1, -1);
			}

			var cycleV = new List<int>();
			var cycleE = new List<int>();
			for (int i = 0; i < _n; i++) {
				if (used[i] != 0) {
					continue;
				}

				bool ok = Dfs(i);
				if (ok) {
					break;
				}
			}

			bool Dfs(int v)
			{
				used[v] = 1;
				foreach (var edge in _edges[v].AsSpan()) {
					if (edge.e == prev[v].e) {
						continue;
					}

					if (edge.v == v) {
						cycleE.Add(edge.e);
						cycleV.Add(v);
						return true;
					}

					if (used[edge.v] == 0) {
						prev[edge.v] = (v, edge.e);
						if (Dfs(edge.v)) {
							return true;
						}
					} else if (used[edge.v] == 1) {
						int cur = v;
						cycleE.Add(edge.e);
						cycleV.Add(v);
						while (cur != edge.v) {
							cycleE.Add(prev[cur].e);
							cycleV.Add(prev[cur].v);
							cur = prev[cur].v;
						}

						cycleE.Reverse();
						cycleV.Reverse();
						return true;
					}
				}

				used[v] = 2;
				return false;
			}

			return (cycleV.ToArray(), cycleE.ToArray());
		}
	}
}

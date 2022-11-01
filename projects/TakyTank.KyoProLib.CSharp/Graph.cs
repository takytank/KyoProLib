using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class Graph
	{
		private readonly int _n;
		private readonly List<(long d, int v, int i)>[] _tempEdges;
		private readonly (long d, int v, int i)[][] _edges;

		private int _k;
		private int[,] _parents;
		private int[] _depth;
		private int _edgeCount;

		public (long d, int v, int i)[][] Edges => _edges;

		public Graph(int n)
		{
			_n = n;
			_tempEdges = new List<(long d, int v, int i)>[n];
			for (int i = 0; i < n; i++) {
				_tempEdges[i] = new List<(long d, int v, int i)>();
			}

			_edges = new (long d, int v, int i)[n][];
			_edgeCount = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge(int u, int v, long d) => _tempEdges[u].Add((d, v, _edgeCount++));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge2W(int u, int v, long d)
		{
			_tempEdges[u].Add((d, v, _edgeCount));
			_tempEdges[v].Add((d, u, _edgeCount));
			++_edgeCount;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Build()
		{
			for (int i = 0; i < _edges.Length; i++) {
				_edges[i] = _tempEdges[i].ToArray();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int[] vertexes, int[] edges) DetectCycle()
		{
			var used = new int[_n];
			var prev = new (int v, int e)[_n];
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
				foreach (var edge in _edges[v]) {
					if (used[edge.v] == 0) {
						prev[edge.v] = (v, edge.i);
						if (Dfs(edge.v)) {
							return true;
						}
					} else if (used[edge.v] == 1) {
						int cur = v;
						cycleE.Add(edge.i);
						while (cur != edge.v) {
							cycleE.Add(prev[cur].e);
							cycleV.Add(prev[cur].v);
							cur = prev[cur].v;
						}

						cycleV.Add(edge.v);
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

			void Dfs(int v, int p, int d)
			{
				_parents[0, v] = p;
				_depth[v] = d;
				foreach (var next in _edges[v]) {
					if (next.v != p) {
						Dfs(next.v, v, d + 1);
					}
				}
			}

			Dfs(root, -1, 0);

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
		public int Lca(int u, int v)
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

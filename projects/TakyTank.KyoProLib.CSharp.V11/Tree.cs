using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V11
{
	public class Tree
	{
		private readonly int _n;
		private readonly List<(int v, long cost)>[] _tempEdges;
		private readonly (int v, long cost)[][] _edges;

		private int _k;
		private int[,] _parents;
		private int[] _depth;

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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Build()
		{
			for (int i = 0; i < _edges.Length; i++) {
				_edges[i] = _tempEdges[i].ToArray();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ReadOnlySpan<(int v, long cost)> Edges(int index) => _edges[index].AsSpan();

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
				foreach (var (next, _) in _edges[v]) {
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

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V8
{
	public class HeavyLightDecomposition
	{
		private readonly List<int>[] _graph;
		private readonly int[] _size;
		private readonly int[] _in;
		private readonly int[] _out;
		private readonly int[] _depth;
		private readonly int[] _head;
		private readonly int[] _reverseindex;
		private readonly int[] _parent;

		public HeavyLightDecomposition(int n)
		{
			_graph = new List<int>[n];
			for (int i = 0; i < n; i++) {
				_graph[i] = new List<int>();
			}

			_size = new int[n];
			_in = new int[n];
			_out = new int[n];
			_depth = new int[n];
			_head = new int[n];
			_reverseindex = new int[n];
			_parent = new int[n];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge(int p, int q)
		{
			_graph[p].Add(q);
			_graph[q].Add(p);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Build(int root = 0)
		{
			DfsSize(root, -1);
			int t = 0;
			DfsHld(0, root, -1, ref t);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void DfsSize(int index, int p)
		{
			_parent[index] = p;
			_size[index] = 1;
			if (_graph[index].Count > 0 && _graph[index][0] == p) {
				(_graph[index][0], _graph[index][^1]) = (_graph[index][^1], _graph[index][0]);
			}

			for (int i = 0; i < _graph[index].Count; i++) {
				int to = _graph[index][i];
				if (to == p) {
					continue;
				}

				DfsSize(to, index);
				_size[index] += _size[to];
				if (_size[_graph[index][0]] < _size[to]) {
					(_graph[index][0], _graph[index][i]) = (to, _graph[index][0]);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void DfsHld(int depth, int index, int p, ref int count)
		{
			_in[index] = count;
			_depth[index] = depth;
			count++;
			_reverseindex[_in[index]] = index;
			foreach (var to in _graph[index]) {
				if (to == p) {
					continue;
				}

				_head[to] = _graph[index][0] == to ? _head[index] : to;
				DfsHld(depth + 1, to, index, ref count);
			}

			_out[index] = count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Depth(int v) => _depth[v];
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Distance(int u, int v) => _depth[u] + _depth[v] - _depth[Lca(u, v)] * 2;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int LevelAncestor(int v, int k)
		{
			while (true) {
				int u = _head[v];
				if (_in[v] - k >= _in[u]) {
					return _reverseindex[_in[v] - k];
				}

				k -= _in[v] - _in[u] + 1;
				v = _parent[u];
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Lca(int u, int v)
		{
			while (true) {
				if (_in[u] > _in[v]) {
					(u, v) = (v, u);
				}

				if (_head[u] == _head[v]) {
					return u;
				}

				v = _parent[_head[v]];
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Vertex(int v) => _in[v];
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Edge(int u, int v)
		{
			if (_in[u] > _in[v]) {
				(u, v) = (v, u);
			}

			if (_head[u] == _head[v]) {
				return _in[u] + 1;
			} else {
				return _in[v];
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ForEachVertex(int u, int v, Action<int, int> action)
		{
			while (true) {
				if (_in[u] > _in[v]) {
					(u, v) = (v, u);
				}

				action(Math.Max(_in[_head[v]], _in[u]), _in[v] + 1);
				if (_head[u] != _head[v]) {
					v = _parent[_head[v]];
				} else {
					break;
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ForEachEdge(int u, int v, Action<int, int> action)
		{
			while (true) {
				if (_in[u] > _in[v]) {
					(u, v) = (v, u);
				}

				if (_head[u] != _head[v]) {
					action(_in[_head[v]], _in[v] + 1);
					v = _parent[_head[v]];
				} else {
					if (u != v) {
						action(_in[u] + 1, _in[v] + 1);
					}

					break;
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Query<T>(
			int u,
			int v,
			T unit,
			Func<int, int, T> query,
			Func<T, T, T> merge,
			bool edge = false)
		{
			T l = unit;
			T r = unit;
			for (; ; v = _parent[_head[v]]) {
				if (_in[u] > _in[v]) {
					(u, v) = (v, u);
					(l, r) = (r, l);
				}

				if (_head[u] == _head[v]) {
					break;
				}

				l = merge(query(_in[_head[v]], _in[v] + 1), l);
			}

			return merge(
				merge(query(_in[u] + (edge ? 1 : 0), _in[v] + 1), l),
				r);
		}
	}
}

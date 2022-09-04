using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V8
{
	public class LevelAncestor
	{
		private readonly List<int>[] _edges;
		private readonly int[,] _parents;
		private readonly List<List<int>> _lad;

		private readonly int[] _depth;
		private readonly int[] _next;
		private readonly int[] _length;
		private readonly int[] _path;
		private readonly int[] _order;
		private readonly int[] _hs;

		public LevelAncestor(int n)
		{
			_edges = new List<int>[n];
			for (int i = 0; i < n; i++) {
				_edges[i] = new List<int>();
			}

			_lad = new List<List<int>>();

			_depth = new int[n];
			_next = new int[n];
			_next.AsSpan().Fill(-1);
			_length = new int[n];
			_path = new int[n];
			_order = new int[n];
			_hs = new int[n + 1];

			int h = 1;
			while ((1 << h) <= n) {
				h++;
			}

			_parents = new int[h, n];
			MemoryMarshal.CreateSpan<int>(ref _parents[0, 0], _parents.Length).Fill(-1);
			for (int i = 2; i <= n; i++) {
				_hs[i] = _hs[i >> 1] + 1;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge2W(int u, int v)
		{
			_edges[u].Add(v);
			_edges[v].Add(u);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dfs(int v, int p, int d, bool isFirst)
		{
			if (_next[v] < 0) {
				_next[v] = v;
				_parents[0, v] = p;
				_length[v] = d;
				_depth[v] = d;
				foreach (int u in _edges[v]) {
					if (u == p) {
						continue;
					}

					Dfs(u, v, d + 1, false);
					if (_length[v] < _length[u]) {
						_next[v] = u;
						_length[v] = _length[u];
					}
				}
			}

			if (isFirst == false) {
				return;
			}

			_path[v] = _lad.Count;
			_lad.Add(new List<int>());
			for (int k = v; ; k = _next[k]) {
				_lad[^1].Add(k);
				_path[k] = _path[v];
				if (k == _next[k]) {
					break;
				}
			}

			for (; ; p = v, v = _next[v]) {
				foreach (var u in _edges[v]) {
					if (u != p && u != _next[v]) {
						Dfs(u, v, d + 1, true);
					}
				}

				if (v == _next[v]) {
					break;
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Build(int root = 0)
		{
			int n = _edges.Length;
			Dfs(root, -1, 0, true);
			for (int k = 0; k + 1 < _parents.GetLength(0); k++) {
				for (int v = 0; v < n; v++) {
					if (_parents[k, v] < 0) {
						_parents[k + 1, v] = -1;
					} else {
						_parents[k + 1, v] = _parents[k, _parents[k, v]];
					}
				}
			}

			for (int i = 0; i < _lad.Count; i++) {
				int v = _lad[i][0], p = _parents[0, v];
				if (~p != 0) {
					int k = _path[p];
					int l = Math.Min(_order[p] + 1, _lad[i].Count);
					_lad[i].AddRange(Enumerable.Repeat(0, l));
					for (int j = 0, m = _lad[i].Count; j + l < m; j++) {
						_lad[i][m - (j + 1)] = _lad[i][m - (j + l + 1)];
					}

					for (int j = 0; j < l; j++) {
						_lad[i][j] = _lad[k][_order[p] - l + j + 1];
					}
				}

				for (int j = 0; j < _lad[i].Count; j++) {
					if (_path[_lad[i][j]] == i) {
						_order[_lad[i][j]] = j;
					}
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Depth(int v) => _depth[v];

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Lca(int u, int v)
		{
			int h = _parents.GetLength(0);

			if (_depth[u] > _depth[v]) {
				(u, v) = (v, u);
			}

			for (int k = 0; k < h; k++) {
				if (((_depth[v] - _depth[u]) >> k & 1) != 0) {
					v = _parents[k, v];
				}
			}

			if (u == v) {
				return u;
			}

			for (int k = h - 1; k >= 0; k--) {
				if (_parents[k, u] == _parents[k, v]) {
					continue;
				}

				u = _parents[k, u];
				v = _parents[k, v];
			}

			return _parents[0, u];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Distance(int u, int v)
		{
			return _depth[u] + _depth[v] - _depth[Lca(u, v)] * 2;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Up(int v, int d)
		{
			if (d == 0) {
				return v;
			}

			v = _parents[_hs[d], v];
			d -= 1 << _hs[d];
			return _lad[_path[v]][_order[v] - d];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Next(int u, int v)
		{
			// from u to v
			if (_depth[u] >= _depth[v]) {
				return _parents[0, u];
			}

			int l = Up(v, _depth[v] - _depth[u] - 1);
			return _parents[0, l] == u ? l : _parents[0, u];
		}
	}
}

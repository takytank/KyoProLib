using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V11
{
	public class Graph
	{
		private readonly int _n;
		private readonly List<(int v, long d, int i)>[] _tempEdges;
		private readonly (int v, long d, int i)[][] _edges;
		private int _edgeCount;

		public (int v, long d, int i)[][] Edges => _edges;

		public Graph(int n)
		{
			_n = n;
			_tempEdges = new List<(int v, long d, int i)>[n];
			for (int i = 0; i < n; i++) {
				_tempEdges[i] = new List<(int v, long d, int i)>();
			}

			_edges = new (int v, long d, int i)[n][];
			_edgeCount = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge(int u, int v, long d) => _tempEdges[u].Add((v, d, _edgeCount++));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge2W(int u, int v, long d)
		{
			_tempEdges[u].Add((v, d, _edgeCount));
			_tempEdges[v].Add((u, d, _edgeCount));
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
				foreach (var edge in _edges[v]) {
					if (edge.i == prev[v].e) {
						continue;
					}

					if (edge.v == v) {
						cycleE.Add(edge.i);
						cycleV.Add(v);
						return true;
					}

					if (used[edge.v] == 0) {
						prev[edge.v] = (v, edge.i);
						if (Dfs(edge.v)) {
							return true;
						}
					} else if (used[edge.v] == 1) {
						int cur = v;
						cycleE.Add(edge.i);
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
		private readonly List<(int v, T item, int i)>[] _tempEdges;
		private readonly (int v, T item, int i)[][] _edges;
		private int _edgeCount;

		public (int v, T item, int i)[][] Edges => _edges;

		public Graph(int n)
		{
			_n = n;
			_tempEdges = new List<(int v, T item, int i)>[n];
			for (int i = 0; i < n; i++) {
				_tempEdges[i] = new List<(int v, T item, int i)>();
			}

			_edges = new (int v, T item, int i)[_n][];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge(int u, int v, T item) => _tempEdges[u].Add((v, item, _edgeCount++));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge2W(int u, int v, T item)
		{
			_tempEdges[u].Add((v, item, _edgeCount));
			_tempEdges[v].Add((u, item, _edgeCount));
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
				foreach (var edge in _edges[v]) {
					if (edge.i == prev[v].e) {
						continue;
					}

					if (edge.v == v) {
						cycleE.Add(edge.i);
						cycleV.Add(v);
						return true;
					}

					if (used[edge.v] == 0) {
						prev[edge.v] = (v, edge.i);
						if (Dfs(edge.v)) {
							return true;
						}
					} else if (used[edge.v] == 1) {
						int cur = v;
						cycleE.Add(edge.i);
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

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class MinimumSpanningTree
	{
		private readonly int _n;
		private readonly List<(int to, long d)>[] _tempEdges;
		private readonly List<(int u, int v, long d)> _tempEdgeList;
		private readonly (int to, long d)[][] _edges;
		private (int u, int v, long d)[] _edgeList;

		public MinimumSpanningTree(int n)
		{
			_n = n;
			_edges = new (int to, long d)[n][];
			_tempEdges = new List<(int v, long d)>[n];
			for (int i = 0; i < n; i++) {
				_tempEdges[i] = new List<(int v, long d)>();
			}

			_tempEdgeList = new List<(int u, int v, long d)>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge(int u, int v, long d = 1)
		{
			_tempEdges[u].Add((v, d));
			_tempEdgeList.Add((u, v, d));
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge2W(int u, int v, long d = 1)
		{
			_tempEdges[u].Add((v, d));
			_tempEdges[v].Add((u, d));
			_tempEdgeList.Add((u, v, d));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Build()
		{
			for (int i = 0; i < _n; i++) {
				_edges[i] = _tempEdges[i].ToArray();
			}

			_edgeList = _tempEdgeList.ToArray();
			Array.Sort(_edgeList, (x, y) => x.d.CompareTo(y.d));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (bool isSpanning, long length, (int u, int v, long cost)[] edges) Kruskal()
		{
			long length = 0;
			var edges = new (int u, int v, long cost)[_n - 1];
			int count = 0;

			var uf = new UnionFindTree(_n);
			foreach (var (u, v, d) in _edgeList) {
				if (uf.IsUnited(u, v)) {
					continue;
				}

				length += d;
				uf.Unite(u, v);
				edges[count] = ((u, v, d));
				++count;
			}

			bool isSpanning = uf.GetSizeOf(0) == _n;
			return (isSpanning, length, edges);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (bool isSpanning, long length, (int u, int v, long cost)[] edges) Prim()
		{
			long length = 0;
			var edges = new (int u, int v, long cost)[_n - 1];
			int count = 0;
			var done = new bool[_n];
			var que = new HeapQueue<(int u, int v, long c), long>(x => x.c, true);
			done[0] = true;
			foreach (var e in _edges[0]) {
				que.Enqueue((0, e.to, e.d));
			}

			while (que.Count > 0) {
				var tag = que.Dequeue();
				if (done[tag.v]) {
					continue;
				}

				done[tag.v] = true;
				length += tag.c;
				edges[count] = (tag.u, tag.v, tag.c);
				++count;
				foreach (var next in _edges[tag.v]) {
					if (done[next.to] == false) {
						que.Enqueue((tag.v, next.to, next.d));
					}
				}
			}

			bool isSpanning = true;
			for (int i = 0; i < _n; i++) {
				if (done[i] == false) {
					isSpanning = false;
					break;
				}
			}

			return (isSpanning, length, edges);
		}
	}
}

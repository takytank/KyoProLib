using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V8
{
	public class MaxFlow
	{
		private const long INF = long.MaxValue;
		private readonly int _n;
		private readonly List<(int v, int index)> _edgeInfos;
		private readonly List<EdgeInternal>[] _edges;
		private List<EdgeInternal>[] _flowedEdges;

		public MaxFlow(int n)
		{
			_n = n;
			_edgeInfos = new List<(int v, int index)>();
			_edges = new List<EdgeInternal>[n];
			for (int i = 0; i < n; i++) {
				_edges[i] = new List<EdgeInternal>();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge(int from, int to, long capacity)
		{
			_edgeInfos.Add((from, _edges[from].Count));
			_edges[from].Add(new EdgeInternal(to, capacity, _edges[to].Count));
			_edges[to].Add(new EdgeInternal(from, 0, _edges[from].Count - 1));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Edge GetFlowedEdge(int i)
		{
			var forward = _flowedEdges[_edgeInfos[i].v][_edgeInfos[i].index];
			var reverse = _flowedEdges[forward.To][forward.ReverseEdgeIndex];
			return new Edge(
				_edgeInfos[i].v, forward.To, (forward.Capacity + reverse.Capacity), reverse.Capacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ReadOnlySpan<Edge> GetFlowedEdges()
		{
			if (_flowedEdges is null) {
				_flowedEdges = _edges;
			}

			var result = new Edge[_edgeInfos.Count];
			for (int i = 0; i < result.Length; ++i) {
				result[i] = GetFlowedEdge(i);
			}

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long Fulkerson(int s, int t, bool keepsEdges = false)
		{
			if (keepsEdges) {
				CopyEdges();
			} else {
				_flowedEdges = _edges;
			}

			long Dfs(int s, int t, long f, bool[] done)
			{
				if (s == t) {
					return f;
				}

				done[s] = true;
				var edges = _flowedEdges[s];
				for (int i = 0; i < edges.Count; ++i) {
					var edge = edges[i];
					if (done[edge.To] == false && edge.Capacity > 0) {
						long d = Dfs(edge.To, t, Math.Min(f, edge.Capacity), done);
						if (d > 0) {
							edge.Capacity -= d;
							_flowedEdges[edge.To][edge.ReverseEdgeIndex].Capacity += d;
							return d;
						}
					}
				}

				return 0;
			}

			long flow = 0;
			while (true) {
				var done = new bool[_n];
				long f = Dfs(s, t, INF, done);
				if (f == 0 || f == INF) {
					break;
				}

				flow += f;
			}

			return flow;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long Dinic(int s, int t, bool keepsEdges = false)
		{
			if (keepsEdges) {
				CopyEdges();
			} else {
				_flowedEdges = _edges;
			}

			long[] Bfs(int s)
			{
				var d = new long[_n];
				d.AsSpan().Fill(-1);
				d[s] = 0;
				var q = new Queue<int>();
				q.Enqueue(s);
				while (q.Count > 0) {
					int v = q.Dequeue();
					var edges = _flowedEdges[v];
					foreach (var edge in edges) {
						if (edge.Capacity > 0 && d[edge.To] < 0) {
							d[edge.To] = d[v] + 1;
							q.Enqueue(edge.To);
						}
					}
				}

				return d;
			}

			long Dfs(int s, int t, long f, int[] done, long[] distance)
			{
				if (s == t) {
					return f;
				}

				var edges = _flowedEdges[s];
				for (; done[s] < edges.Count; done[s]++) {
					var edge = edges[done[s]];
					if (edge.Capacity > 0 && distance[s] < distance[edge.To]) {
						long d = Dfs(edge.To, t, Math.Min(f, edge.Capacity), done, distance);
						if (d > 0) {
							edge.Capacity -= d;
							_flowedEdges[edge.To][edge.ReverseEdgeIndex].Capacity += d;
							return d;
						}
					}
				}

				return 0;
			}

			long flow = 0;
			while (true) {
				var distance = Bfs(s);
				if (distance[t] < 0) {
					break;
				}

				var done = new int[_n];
				while (true) {
					long f = Dfs(s, t, INF, done, distance);
					if (f == 0 || f == INF) {
						break;
					}

					flow += f;
				}
			}

			return flow;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (bool[] canReach, (int u, int v, long flow)[] minCutEdges) MinCut(int s)
		{
			var visited = new bool[_n];
			var que = new Queue<int>();
			que.Enqueue(s);
			while (que.Count > 0) {
				int p = que.Dequeue();
				visited[p] = true;
				foreach (var e in _edges[p]) {
					if (e.Capacity != 0 && visited[e.To] == false) {
						visited[e.To] = true;
						que.Enqueue(e.To);
					}
				}
			}

			if (_flowedEdges is null) {
				_flowedEdges = _edges;
			}

			var minCutEdges = new List<(int u, int v, long flow)>();
			for (int i = 0; i < _edgeInfos.Count; ++i) {
				var forward = _flowedEdges[_edgeInfos[i].v][_edgeInfos[i].index];
				int from = _edgeInfos[i].v;
				int to = forward.To;
				long flow = _flowedEdges[forward.To][forward.ReverseEdgeIndex].Capacity;
				if (visited[from] != visited[to]) {
					minCutEdges.Add((from, to, flow));
				}
			}

			return (visited, minCutEdges.ToArray());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void CopyEdges()
		{
			_flowedEdges = new List<EdgeInternal>[_n];
			for (int i = 0; i < _n; ++i) {
				_flowedEdges[i] = new List<EdgeInternal>();
				var edges = _edges[i];
				foreach (var edge in edges) {
					_flowedEdges[i].Add(edge);
				}
			}
		}

		private class EdgeInternal
		{
			public int To { get; set; }
			public int ReverseEdgeIndex { get; set; }
			public long Capacity { get; set; }
			public EdgeInternal(int to, long capacity, int reverse)
			{
				To = to;
				Capacity = capacity;
				ReverseEdgeIndex = reverse;
			}
		}

		public struct Edge
		{
			public int From { get; set; }
			public int To { get; set; }
			public long Capacity { get; set; }
			public long Flow { get; set; }
			public Edge(int from, int to, long capacity, long flow)
			{
				From = from;
				To = to;
				Capacity = capacity;
				Flow = flow;
			}
		};
	}
}

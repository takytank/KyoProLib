using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V8
{
	public class MaxFlow
	{
		private const long INF = long.MaxValue;
		private readonly int n_;
		private readonly List<(int v, int index)> edgeInfos_;
		private readonly JagList2<EdgeInternal> edges_;
		private JagList2<EdgeInternal> flowedEdges_;

		public MaxFlow(int n)
		{
			n_ = n;
			edgeInfos_ = new List<(int v, int index)>();
			edges_ = new JagList2<EdgeInternal>(n);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge(int from, int to, long capacity)
		{
			edgeInfos_.Add((from, edges_.Raw[from].Count));
			edges_.Add(from, new EdgeInternal(to, capacity, edges_.Raw[to].Count));
			edges_.Add(to, new EdgeInternal(from, 0, edges_.Raw[from].Count - 1));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Build()
		{
			edges_.Build();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Edge GetFlowedEdge(int i)
		{
			var to = flowedEdges_[edgeInfos_[i].v][edgeInfos_[i].index];
			var from = flowedEdges_[to.To][to.ReverseEdgeIndex];
			return new Edge(
				edgeInfos_[i].v, to.To, (to.Capacity + from.Capacity), from.Capacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ReadOnlySpan<Edge> GetFlowedEdges()
		{
			if (flowedEdges_ is null) {
				flowedEdges_ = edges_;
			}

			var result = new Edge[edgeInfos_.Count];
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
				flowedEdges_ = edges_;
			}

			long Dfs(int s, int t, long f, bool[] done)
			{
				if (s == t) {
					return f;
				}

				done[s] = true;
				var edges = flowedEdges_[s];
				for (int i = 0; i < edges.Length; ++i) {
					ref var edge = ref edges[i];
					if (done[edge.To] == false && edge.Capacity > 0) {
						long d = Dfs(edge.To, t, Math.Min(f, edge.Capacity), done);
						if (d > 0) {
							edge.Capacity -= d;
							flowedEdges_[edge.To][edge.ReverseEdgeIndex].Capacity += d;
							return d;
						}
					}
				}

				return 0;
			}

			long flow = 0;
			while (true) {
				var done = new bool[n_];
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
				flowedEdges_ = edges_;
			}

			long[] Bfs(int s)
			{
				var d = new long[n_];
				d.AsSpan().Fill(-1);
				d[s] = 0;
				var q = new Queue<int>();
				q.Enqueue(s);
				while (q.Count > 0) {
					int v = q.Dequeue();
					var edges = flowedEdges_[v];
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

				var edges = flowedEdges_[s];
				for (; done[s] < edges.Length; done[s]++) {
					ref var edge = ref edges[done[s]];
					if (edge.Capacity > 0 && distance[s] < distance[edge.To]) {
						long d = Dfs(edge.To, t, Math.Min(f, edge.Capacity), done, distance);
						if (d > 0) {
							edge.Capacity -= d;
							flowedEdges_[edge.To][edge.ReverseEdgeIndex].Capacity += d;
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

				var done = new int[n_];
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
		private void CopyEdges()
		{
			flowedEdges_ = new JagList2<EdgeInternal>(n_);
			for (int i = 0; i < n_; ++i) {
				var edges = edges_[i];
				foreach (var edge in edges) {
					flowedEdges_.Add(i, edge);
				}
			}

			flowedEdges_.Build();
		}

		private struct EdgeInternal
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

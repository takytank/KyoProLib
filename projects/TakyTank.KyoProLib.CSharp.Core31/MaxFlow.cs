using System;
using System.Collections.Generic;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.Core31
{
	public class MaxFlow
	{
		private const long INF = long.MaxValue;
		private readonly int n_;
		private readonly List<(int first, int second)> position_;
		private readonly List<EdgeInternal>[] edges_;
		private List<EdgeInternal>[] flowedEdges_;

		public MaxFlow(int n)
		{
			n_ = n;
			position_ = new List<(int first, int second)>();
			edges_ = new List<EdgeInternal>[n];
			for (int i = 0; i < n; i++) {
				edges_[i] = new List<EdgeInternal>();
			}
		}

		public void AddEdge(int from, int to, long capacity)
		{
			position_.Add((from, edges_[from].Count));
			edges_[from].Add(new EdgeInternal(to, capacity, edges_[to].Count));
			edges_[to].Add(new EdgeInternal(from, 0, edges_[from].Count - 1));
		}

		private Edge GetFlowedEdge(int i)
		{
			var to = flowedEdges_[position_[i].first][position_[i].second];
			var from = flowedEdges_[to.To][to.ReverseEdgeIndex];
			return new Edge(
				position_[i].first, to.To, (to.Capacity + from.Capacity), from.Capacity);
		}

		public IReadOnlyList<Edge> GetFlowedEdges()
		{
			if (flowedEdges_ is null) {
				flowedEdges_ = edges_;
			}

			var result = new List<Edge>();
			for (int i = 0; i < position_.Count; i++) {
				result.Add(GetFlowedEdge(i));
			}

			return result;
		}

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
				var edgess = flowedEdges_.AsSpan();
				var edges = edgess[s].AsSpan();
				for (int i = 0; i < edges.Length; i++) {
					ref var edge = ref edges[i];
					if (done[edge.To] == false && edge.Capacity > 0) {
						long d = Dfs(edge.To, t, Math.Min(f, edge.Capacity), done);
						if (d > 0) {
							edge.Capacity -= d;
							edgess[edge.To].AsSpan()[edge.ReverseEdgeIndex].Capacity += d;
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
				var edgess = flowedEdges_.AsSpan();
				while (q.Count > 0) {
					int v = q.Dequeue();
					for (int i = 0; i < edgess[v].Count; i++) {
						var edge = edgess[v][i];
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

				var edgess = flowedEdges_.AsSpan();
				var edges = edgess[s].AsSpan();
				for (; done[s] < edges.Length; done[s]++) {
					ref var edge = ref edges[done[s]];
					if (edge.Capacity > 0 && distance[s] < distance[edge.To]) {
						long d = Dfs(edge.To, t, Math.Min(f, edge.Capacity), done, distance);
						if (d > 0) {
							edge.Capacity -= d;
							edgess[edge.To].AsSpan()[edge.ReverseEdgeIndex].Capacity += d;
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

		private void CopyEdges()
		{
			flowedEdges_ = new List<EdgeInternal>[n_];
			for (int i = 0; i < n_; i++) {
				flowedEdges_[i] = new List<EdgeInternal>();
			}

			for (int i = 0; i < n_; i++) {
				var span = edges_[i].AsSpan();
				for (int j = 0; j < span.Length; j++) {
					flowedEdges_[i].Add(span[j]);
				}
			}
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

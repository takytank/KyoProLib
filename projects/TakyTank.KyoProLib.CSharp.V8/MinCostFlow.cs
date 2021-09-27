using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V8
{
	public class MinCostFlow
	{
		private const long INF = long.MaxValue;
		private readonly int n_;
		private readonly List<(int v, int index)> edgeInfos_;
		private readonly JagList2<EdgeInternal> edges_;
		private JagList2<EdgeInternal> flowedEdges_;

		public MinCostFlow(int n)
		{
			n_ = n;
			edgeInfos_ = new List<(int v, int index)>();
			edges_ = new JagList2<EdgeInternal>(n);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge(int from, int to, long capacity, long cost = 1)
		{
			edgeInfos_.Add((from, edges_.Raw[from].Count));
			edges_.Add(from, new EdgeInternal(to, capacity, cost, edges_.Raw[to].Count));
			edges_.Add(to, new EdgeInternal(from, 0, -1 * cost, edges_.Raw[from].Count - 1));
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
		public (long flow, long cost) BellmanFord(
			int s, int t, long flowLimit, bool keepsEdges = false)
		{
			if (keepsEdges) {
				CopyEdges();
			} else {
				flowedEdges_ = edges_;
			}

			var prevVertexes = new int[n_];
			var prevEdgeIndexes = new int[n_];
			var distances = new long[n_];

			long minCost = 0;
			long f = flowLimit;
			while (f > 0) {
				distances.AsSpan().Fill(INF);
				distances[s] = 0;
				for (int i = 0; i < n_; ++i) {
					bool changes = false;
					for (int v = 0; v < n_; ++v) {
						if (distances[v] == INF) {
							continue;
						}

						var edges = flowedEdges_[v];
						for (int j = 0; j < edges.Length; ++j) {
							var edge = edges[j];
							long newDistance = distances[v] + edge.Cost;
							if (edge.Capacity > 0 && distances[edge.To] > newDistance) {
								distances[edge.To] = newDistance;
								prevVertexes[edge.To] = v;
								prevEdgeIndexes[edge.To] = j;
								changes = true;
							}
						}
					}

					if (i == n_ - 1 && changes) {
						return (-1, 0);
					}

					if (changes == false) {
						break;
					}
				}

				if (distances[t] == INF) {
					return (-1, 0);
				}

				long d = f;
				for (int v = t; v != s; v = prevVertexes[v]) {
					d = Math.Min(d, flowedEdges_[prevVertexes[v]][prevEdgeIndexes[v]].Capacity);
				}

				f -= d;
				minCost += d * distances[t];
				for (int v = t; v != s; v = prevVertexes[v]) {
					ref var e = ref flowedEdges_[prevVertexes[v]][prevEdgeIndexes[v]];
					e.Capacity -= d;
					flowedEdges_[v][e.ReverseEdgeIndex].Capacity += d;
				}
			}

			return (flowLimit - f, minCost);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long flow, long cost) Fpsa(
			int s, int t, long flowLimit, bool keepsEdges = false)
		{
			if (keepsEdges) {
				CopyEdges();
			} else {
				flowedEdges_ = edges_;
			}

			var prevVertexes = new int[n_];
			var prevEdgeIndexes = new int[n_];
			var distances = new long[n_];
			var pending = new bool[n_];
			var times = new int[n_];

			long minCost = 0;
			long f = flowLimit;
			while (f > 0) {
				var que = new Queue<int>();
				que.Enqueue(s);

				distances.AsSpan().Fill(INF);
				distances[s] = 0;
				pending.AsSpan().Fill(false);
				pending[s] = true;
				times.AsSpan().Fill(0);
				times[s]++;
				while (que.Count > 0) {
					var v = que.Dequeue();
					pending[v] = false;
					var edges = flowedEdges_[v];
					for (int j = 0; j < edges.Length; ++j) {
						var edge = edges[j];
						var newDistance = distances[v] + edge.Cost;
						if (edge.Capacity <= 0 || newDistance >= distances[edge.To]) {
							continue;
						}

						distances[edge.To] = newDistance;
						prevVertexes[edge.To] = v;
						prevEdgeIndexes[edge.To] = j;
						if (pending[edge.To] == false) {
							++times[edge.To];
							if (times[edge.To] >= n_) {
								return (-1, 0);
							}

							pending[edge.To] = true;
							que.Enqueue(edge.To);
						}
					}
				}

				if (distances[t] == INF) {
					return (-1, 0);
				}

				long d = f;
				for (int v = t; v != s; v = prevVertexes[v]) {
					d = Math.Min(d, flowedEdges_[prevVertexes[v]][prevEdgeIndexes[v]].Capacity);
				}

				f -= d;
				minCost += d * distances[t];
				for (int v = t; v != s; v = prevVertexes[v]) {
					ref var e = ref flowedEdges_[prevVertexes[v]][prevEdgeIndexes[v]];
					e.Capacity -= d;
					flowedEdges_[v][e.ReverseEdgeIndex].Capacity += d;
				}
			}

			return (flowLimit - f, minCost);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long flow, long cost) Dijkstra(
			int s, int t, long flowLimit, bool hasMinusCost = false, bool keepsEdges = false)
		{
			if (keepsEdges) {
				CopyEdges();
			} else {
				flowedEdges_ = edges_;
			}

			var potentials = new long[n_];
			var prevVertexes = new int[n_];
			var prevEdgeIndexes = new int[n_];
			var distances = new long[n_];

			bool first = true;
			long minCost = 0;
			long f = flowLimit;
			var que = new DijkstraQ();
			while (f > 0) {
				distances.AsSpan().Fill(INF);
				distances[s] = 0;

				if (first && hasMinusCost) {
					for (int i = 0; i < n_; ++i) {
						bool changes = false;
						for (int v = 0; v < n_; ++v) {
							if (distances[v] == INF) {
								continue;
							}

							var edges = flowedEdges_[v];
							for (int j = 0; j < edges.Length; ++j) {
								var edge = edges[j];
								long newDistance = distances[v] + edge.Cost;
								if (edge.Capacity > 0 && distances[edge.To] > newDistance) {
									distances[edge.To] = newDistance;
									prevVertexes[edge.To] = v;
									prevEdgeIndexes[edge.To] = j;
									changes = true;
								}
							}
						}

						if (i == n_ - 1 && changes) {
							return (-1, 0);
						}

						if (changes == false) {
							break;
						}
					}
				} else {
					que.Enqueue(0, s);
					while (que.Count > 0) {
						var current = que.Dequeue();
						int v = current.v;
						if (distances[v] < current.distance) {
							continue;
						}

						var edges = flowedEdges_[v];
						for (int j = 0; j < edges.Length; ++j) {
							var edge = edges[j];
							long newDistance = distances[v] + edge.Cost + potentials[v] - potentials[edge.To];
							if (edge.Capacity > 0 && distances[edge.To] > newDistance) {
								distances[edge.To] = newDistance;
								prevVertexes[edge.To] = v;
								prevEdgeIndexes[edge.To] = j;
								que.Enqueue(newDistance, edge.To);
							}
						}
					}
				}

				if (distances[t] == INF) {
					return (-1, 0);
				}

				for (int v = 0; v < n_; ++v) {
					potentials[v] += distances[v];
				}

				long d = f;
				for (int v = t; v != s; v = prevVertexes[v]) {
					d = Math.Min(d, flowedEdges_[prevVertexes[v]][prevEdgeIndexes[v]].Capacity);
				}

				f -= d;
				minCost += d * potentials[t];
				for (int v = t; v != s; v = prevVertexes[v]) {
					ref var e = ref flowedEdges_[prevVertexes[v]][prevEdgeIndexes[v]];
					e.Capacity -= d;
					flowedEdges_[v][e.ReverseEdgeIndex].Capacity += d;
				}

				first = false;
			}

			return (flowLimit - f, minCost);
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
			public long Cost { get; set; }
			public EdgeInternal(int to, long capacity, long cost, int reverse)
			{
				To = to;
				Capacity = capacity;
				Cost = cost;
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

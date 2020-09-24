using System;
using System.Collections.Generic;
using System.Text;

namespace TakyTank.KyoProLib.CS8
{
	public class Flow
	{
		private const long INF = long.MaxValue;
		private readonly int n_;
		private readonly List<(int first, int second)> position_;
		private readonly List<EdgeInternal>[] edges_;

		public Flow(int n)
		{
			n_ = n;
			position_ = new List<(int first, int second)>();
			edges_ = new List<EdgeInternal>[n];
			for (int i = 0; i < n; i++) {
				edges_[i] = new List<EdgeInternal>();
			}
		}

		public void AddEdge(int from, int to, long capacity, long cost = 1)
		{
			position_.Add((from, edges_[from].Count));
			edges_[from].Add(new EdgeInternal(to, capacity, cost, edges_[to].Count));
			edges_[to].Add(new EdgeInternal(from, 0, -1 * cost, edges_[from].Count - 1));
		}

		public Edge GetEdge(int i)
		{
			var to = edges_[position_[i].first][position_[i].second];
			var from = edges_[to.To][to.ReverseEdgeIndex];
			return new Edge(
				position_[i].first, to.To, (to.Capacity + from.Capacity), from.Capacity);
		}

		public IReadOnlyList<Edge> GetEdges()
		{
			var result = new List<Edge>();
			for (int i = 0; i < position_.Count; i++) {
				result.Add(GetEdge(i));
			}
			return result;
		}

		public long CalculateMaxFlowByFordFulkerson(int s, int t)
		{
			long Dfs(int s, int t, long f, bool[] done)
			{
				if (s == t) {
					return f;
				}

				done[s] = true;
				foreach (var edge in edges_[s]) {
					if (done[edge.To] == false && edge.Capacity > 0) {
						long d = Dfs(edge.To, t, Math.Min(f, edge.Capacity), done);
						if (d > 0) {
							edge.Capacity -= d;
							edges_[edge.To][edge.ReverseEdgeIndex].Capacity += d;
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

		public long CalculateMaxFlowByDinic(int s, int t)
		{
			long[] Bfs(int s)
			{
				var d = new long[n_];
				d.AsSpan().Fill(-1);
				d[s] = 0;
				var q = new Queue<int>();
				q.Enqueue(s);
				while (q.Count > 0) {
					int v = q.Dequeue();
					foreach (var edge in edges_[v]) {
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

				for (; done[s] < edges_[s].Count; done[s]++) {
					var edge = edges_[s][done[s]];
					if (edge.Capacity > 0 && distance[s] < distance[edge.To]) {
						long d = Dfs(edge.To, t, Math.Min(f, edge.Capacity), done, distance);
						if (d > 0) {
							edge.Capacity -= d;
							edges_[edge.To][edge.ReverseEdgeIndex].Capacity += d;
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

		public (long flow, long cost) CalculateMinCostFlowByBellmanFord(int s, int t, long flowLimit)
		{
			var prevVertexes = new int[n_];
			var prevEdgeIndexes = new int[n_];
			var distances = new long[n_];

			long minCost = 0;
			long f = flowLimit;
			while (f > 0) {
				distances.AsSpan().Fill(INF);
				distances[s] = 0;
				for (int i = 0; i < n_; i++) {
					bool changes = false;
					for (int v = 0; v < n_; v++) {
						if (distances[v] == INF) {
							continue;
						}

						for (int j = 0; j < edges_[v].Count; j++) {
							var edge = edges_[v][j];
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
					d = Math.Min(d, edges_[prevVertexes[v]][prevEdgeIndexes[v]].Capacity);
				}

				f -= d;
				minCost += d * distances[t];
				for (int v = t; v != s; v = prevVertexes[v]) {
					edges_[prevVertexes[v]][prevEdgeIndexes[v]].Capacity -= d;
					edges_[v][edges_[prevVertexes[v]][prevEdgeIndexes[v]].ReverseEdgeIndex].Capacity += d;
				}
			}

			return (flowLimit - f, minCost);
		}

		public (long flow, long cost) CalculateMinCostFlowByDijkstra(
			int s, int t, long flowLimit, bool hasMinusCost = false)
		{
			var potentials = new long[n_];
			var prevVertexes = new int[n_];
			var prevEdgeIndexes = new int[n_];
			var distances = new long[n_];

			bool first = true;
			long minCost = 0;
			long f = flowLimit;
			while (f > 0) {
				distances.AsSpan().Fill(INF);
				distances[s] = 0;

				if (first && hasMinusCost) {
					for (int i = 0; i < n_; i++) {
						bool changes = false;
						for (int v = 0; v < n_; v++) {
							if (distances[v] == INF) {
								continue;
							}

							for (int j = 0; j < edges_[v].Count; j++) {
								var edge = edges_[v][j];
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
					var pq = new HeapQ();
					pq.Enqueue(0, s);
					while (pq.Count > 0) {
						var current = pq.Dequeue();
						int v = current.v;
						if (distances[v] < current.distance) {
							continue;
						}

						for (int j = 0; j < edges_[v].Count; j++) {
							var edge = edges_[v][j];
							long newDistance = distances[v] + edge.Cost + potentials[v] - potentials[edge.To];
							if (edge.Capacity > 0 && distances[edge.To] > newDistance) {
								distances[edge.To] = newDistance;
								prevVertexes[edge.To] = v;
								prevEdgeIndexes[edge.To] = j;
								pq.Enqueue(newDistance, edge.To);
							}
						}
					}
				}

				if (distances[t] == INF) {
					return (-1, 0);
				}

				for (int v = 0; v < n_; v++) {
					potentials[v] += distances[v];
				}

				long d = f;
				for (int v = t; v != s; v = prevVertexes[v]) {
					d = Math.Min(d, edges_[prevVertexes[v]][prevEdgeIndexes[v]].Capacity);
				}

				f -= d;
				minCost += d * potentials[t];
				for (int v = t; v != s; v = prevVertexes[v]) {
					edges_[prevVertexes[v]][prevEdgeIndexes[v]].Capacity -= d;
					edges_[v][edges_[prevVertexes[v]][prevEdgeIndexes[v]].ReverseEdgeIndex].Capacity += d;
				}

				first = false;
			}

			return (flowLimit - f, minCost);
		}

		private class EdgeInternal
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

		private class HeapQ
		{
			private (long distance, int v)[] heap_;

			public int Count { get; private set; } = 0;
			public HeapQ()
			{
				heap_ = new (long distance, int v)[1024];
			}

			public void Enqueue(long distance, int v)
			{
				var pair = (distance, v);
				if (heap_.Length == Count) {
					var newHeap = new (long distance, int v)[heap_.Length * 2];
					heap_.CopyTo(newHeap, 0);
					heap_ = newHeap;
				}

				heap_[Count] = pair;
				++Count;

				int c = Count - 1;
				while (c > 0) {
					int p = (c - 1) >> 1;
					if (heap_[p].distance > distance) {
						heap_[c] = heap_[p];
						c = p;
					} else {
						break;
					}
				}

				heap_[c] = pair;
			}

			public (long distance, int v) Dequeue()
			{
				(long distance, int v) ret = heap_[0];
				int n = Count - 1;

				var item = heap_[n];
				int p = 0;
				int c = (p << 1) + 1;
				while (c < n) {
					if (c != n - 1 && heap_[c + 1].distance < heap_[c].distance) {
						++c;
					}

					if (item.distance > heap_[c].distance) {
						heap_[p] = heap_[c];
						p = c;
						c = (p << 1) + 1;
					} else {
						break;
					}
				}

				heap_[p] = item;
				Count--;

				return ret;
			}
		}
	}
}

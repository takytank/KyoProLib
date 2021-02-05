using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.Core31
{
	public class MinCostFlow
	{
		private const long INF = long.MaxValue;
		private readonly int n_;
		private readonly LightList<(int first, int second)> position_;
		private readonly LightList<EdgeInternal>[] edges_;
		private LightList<EdgeInternal>[] flowedEdges_;

		public MinCostFlow(int n)
		{
			n_ = n;
			position_ = new LightList<(int first, int second)>();
			edges_ = new LightList<EdgeInternal>[n];
			for (int i = 0; i < n; i++) {
				edges_[i] = new LightList<EdgeInternal>();
			}
		}

		public void AddEdge(int from, int to, long capacity, long cost = 1)
		{
			position_.Add((from, edges_[from].Count));
			edges_[from].Add(new EdgeInternal(to, capacity, cost, edges_[to].Count));
			edges_[to].Add(new EdgeInternal(from, 0, -1 * cost, edges_[from].Count - 1));
		}

		private Edge GetFlowedEdge(int i)
		{
			var to = flowedEdges_[position_[i].first][position_[i].second];
			var from = flowedEdges_[to.To][to.ReverseEdgeIndex];
			return new Edge(
				position_[i].first, to.To, (to.Capacity + from.Capacity), from.Capacity);
		}

		public ReadOnlySpan<Edge> GetFlowedEdges()
		{
			if (flowedEdges_ is null) {
				flowedEdges_ = edges_;
			}

			var result = new Edge[position_.Count];
			for (int i = 0; i < result.Length; i++) {
				result[i] = GetFlowedEdge(i);
			}

			return result;
		}

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
			var edgess = flowedEdges_.AsSpan();
			while (f > 0) {
				distances.AsSpan().Fill(INF);
				distances[s] = 0;
				for (int i = 0; i < n_; i++) {
					bool changes = false;
					for (int v = 0; v < n_; v++) {
						if (distances[v] == INF) {
							continue;
						}

						var edges = edgess[v].AsSpan();
						for (int j = 0; j < edges.Length; j++) {
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
					d = Math.Min(d, edgess[prevVertexes[v]][prevEdgeIndexes[v]].Capacity);
				}

				f -= d;
				minCost += d * distances[t];
				for (int v = t; v != s; v = prevVertexes[v]) {
					ref var e = ref edgess[prevVertexes[v]].AsSpan()[prevEdgeIndexes[v]];
					e.Capacity -= d;
					edgess[v].AsSpan()[e.ReverseEdgeIndex].Capacity += d;
				}
			}

			return (flowLimit - f, minCost);
		}

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
			var edgess = flowedEdges_.AsSpan();
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
					var edges = edgess[v].AsSpan();
					for (int j = 0; j < edges.Length; j++) {
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
					d = Math.Min(d, edgess[prevVertexes[v]][prevEdgeIndexes[v]].Capacity);
				}

				f -= d;
				minCost += d * distances[t];
				for (int v = t; v != s; v = prevVertexes[v]) {
					ref var e = ref edgess[prevVertexes[v]].AsSpan()[prevEdgeIndexes[v]];
					e.Capacity -= d;
					edgess[v].AsSpan()[e.ReverseEdgeIndex].Capacity += d;
				}
			}

			return (flowLimit - f, minCost);
		}

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
			var edgess = flowedEdges_.AsSpan();
			var que = new RadixHeap();
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

							var edges = edgess[v].AsSpan();
							for (int j = 0; j < edges.Length; j++) {
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
					que.Push(0, s);
					while (que.Count > 0) {
						var current = que.Pop();
						int v = current.v;
						if (distances[v] < current.distance) {
							continue;
						}

						var edges = edgess[v].AsSpan();
						for (int j = 0; j < edges.Length; j++) {
							var edge = edges[j];
							long newDistance = distances[v] + edge.Cost + potentials[v] - potentials[edge.To];
							if (edge.Capacity > 0 && distances[edge.To] > newDistance) {
								distances[edge.To] = newDistance;
								prevVertexes[edge.To] = v;
								prevEdgeIndexes[edge.To] = j;
								que.Push(newDistance, edge.To);
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
					d = Math.Min(d, edgess[prevVertexes[v]][prevEdgeIndexes[v]].Capacity);
				}

				f -= d;
				minCost += d * potentials[t];
				for (int v = t; v != s; v = prevVertexes[v]) {
					ref var e = ref edgess[prevVertexes[v]].AsSpan()[prevEdgeIndexes[v]];
					e.Capacity -= d;
					edgess[v].AsSpan()[e.ReverseEdgeIndex].Capacity += d;
				}

				first = false;
			}

			return (flowLimit - f, minCost);
		}

		private void CopyEdges()
		{
			flowedEdges_ = new LightList<EdgeInternal>[n_];
			for (int i = 0; i < n_; i++) {
				flowedEdges_[i] = new LightList<EdgeInternal>();
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

		private class RadixHeap
		{
			private const int MAX_BIT = 64;
			private readonly LightList<(long distance, int v)>[] buckets_;
			private readonly long[] mins_;
			private long lastDistance_;

			public int Count { get; private set; }

			public RadixHeap()
			{
				buckets_ = new LightList<(long distance, int v)>[MAX_BIT + 1];
				for (int i = 0; i < MAX_BIT + 1; i++) {
					buckets_[i] = new LightList<(long distance, int vertex)>();
				}

				mins_ = new long[MAX_BIT + 1];
				mins_.AsSpan().Fill(long.MaxValue);
			}

			public void Push(long distance, int v)
			{
				++Count;
				int bit = GetBit(distance ^ lastDistance_);
				buckets_[bit].Add((distance, v));
				mins_[bit] = Math.Min(mins_[bit], distance);
			}

			public (long distance, int v) Pop()
			{
				if (buckets_[0].Count == 0) {
					int index = 1;
					while (buckets_[index].Count == 0) {
						++index;
					}

					lastDistance_ = mins_[index];
					foreach (var item in buckets_[index].AsSpan()) {
						int bit = GetBit(item.distance ^ lastDistance_);
						buckets_[bit].Add(item);
						mins_[bit] = Math.Min(mins_[bit], item.distance);
					}

					buckets_[index].Clear();
					mins_[index] = long.MaxValue;
				}

				--Count;
				var ret = buckets_[0][^1];
				buckets_[0].Remove();
				return ret;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private int GetBit(long a)
				=> a != 0 ? MAX_BIT - BitOperations.LeadingZeroCount((ulong)a) : 0;
		}
	}
}

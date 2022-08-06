using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V8
{
	public class MinCostFlow
	{
		private const long INF = long.MaxValue;
		private readonly int _n;
		private readonly List<(int v, int index)> _edgeInfos;
		private readonly List<EdgeInternal>[] _edges;
		private List<EdgeInternal>[] _flowedEdges;

		public MinCostFlow(int n)
		{
			_n = n;
			_edgeInfos = new List<(int v, int index)>();
			_edges = new List<EdgeInternal>[n];
			for (int i = 0; i < n; i++) {
				_edges[i] = new List<EdgeInternal>();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge(int from, int to, long capacity, long cost = 1)
		{
			_edgeInfos.Add((from, _edges[from].Count));
			_edges[from].Add(new EdgeInternal(to, capacity, cost, _edges[to].Count));
			_edges[to].Add(new EdgeInternal(from, 0, -1 * cost, _edges[from].Count - 1));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Edge GetFlowedEdge(int i)
		{
			var to = _flowedEdges[_edgeInfos[i].v][_edgeInfos[i].index];
			var from = _flowedEdges[to.To][to.ReverseEdgeIndex];
			return new Edge(
				_edgeInfos[i].v, to.To, (to.Capacity + from.Capacity), from.Capacity);
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
		public (long flow, long cost) BellmanFord(
			int s, int t, long flowLimit, bool keepsEdges = false)
			=> BellmanFordSlopes(s, t, flowLimit, keepsEdges).Last();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public List<(long flow, long cost)> BellmanFordSlopes(
			int s, int t, long flowLimit, bool keepsEdges = false)
		{
			if (keepsEdges) {
				CopyEdges();
			} else {
				_flowedEdges = _edges;
			}

			var prevVertexes = new int[_n];
			var prevEdgeIndexes = new int[_n];
			var distances = new long[_n];

			var result = new List<(long flow, long cost)>();
			result.Add((0, 0));
			long prevCostPerFlow = -1;
			long minCost = 0;
			long f = flowLimit;
			while (f > 0) {
				distances.AsSpan().Fill(INF);
				distances[s] = 0;
				for (int i = 0; i < _n; ++i) {
					bool changes = false;
					for (int v = 0; v < _n; ++v) {
						if (distances[v] == INF) {
							continue;
						}

						var edges = _flowedEdges[v];
						for (int j = 0; j < edges.Count; ++j) {
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

					if (i == _n - 1 && changes) {
						return result;
					}

					if (changes == false) {
						break;
					}
				}

				if (distances[t] == INF) {
					return result;
				}

				long d = f;
				for (int v = t; v != s; v = prevVertexes[v]) {
					d = Math.Min(d, _flowedEdges[prevVertexes[v]][prevEdgeIndexes[v]].Capacity);
				}

				f -= d;
				minCost += d * distances[t];
				for (int v = t; v != s; v = prevVertexes[v]) {
					var e = _flowedEdges[prevVertexes[v]][prevEdgeIndexes[v]];
					e.Capacity -= d;
					_flowedEdges[v][e.ReverseEdgeIndex].Capacity += d;
				}

				if (prevCostPerFlow == distances[t]) {
					result.RemoveAt(result.Count - 1);
				}

				result.Add((flowLimit - f, minCost));
				prevCostPerFlow = distances[t];
			}

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long flow, long cost) Fpsa(
			int s, int t, long flowLimit, bool keepsEdges = false)
			=> FpsaSlopes(s, t, flowLimit, keepsEdges).Last();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public List<(long flow, long cost)> FpsaSlopes(
			int s, int t, long flowLimit, bool keepsEdges = false)
		{
			if (keepsEdges) {
				CopyEdges();
			} else {
				_flowedEdges = _edges;
			}

			var prevVertexes = new int[_n];
			var prevEdgeIndexes = new int[_n];
			var distances = new long[_n];
			var pending = new bool[_n];
			var times = new int[_n];

			var result = new List<(long flow, long cost)>();
			result.Add((0, 0));
			long prevCostPerFlow = -1;
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
					var edges = _flowedEdges[v];
					for (int j = 0; j < edges.Count; ++j) {
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
							if (times[edge.To] >= _n) {
								return result;
							}

							pending[edge.To] = true;
							que.Enqueue(edge.To);
						}
					}
				}

				if (distances[t] == INF) {
					return result;
				}

				long d = f;
				for (int v = t; v != s; v = prevVertexes[v]) {
					d = Math.Min(d, _flowedEdges[prevVertexes[v]][prevEdgeIndexes[v]].Capacity);
				}

				f -= d;
				minCost += d * distances[t];
				for (int v = t; v != s; v = prevVertexes[v]) {
					var e = _flowedEdges[prevVertexes[v]][prevEdgeIndexes[v]];
					e.Capacity -= d;
					_flowedEdges[v][e.ReverseEdgeIndex].Capacity += d;
				}

				if (prevCostPerFlow == distances[t]) {
					result.RemoveAt(result.Count - 1);
				}

				result.Add((flowLimit - f, minCost));
				prevCostPerFlow = distances[t];
			}

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long flow, long cost) Dijkstra(
			int s, int t, long flowLimit, bool hasMinusCost = false, bool keepsEdges = false)
			=> DijkstraSlopes(s, t, flowLimit, hasMinusCost, keepsEdges).Last();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public List<(long flow, long cost)> DijkstraSlopes(
			int s, int t, long flowLimit, bool hasMinusCost = false, bool keepsEdges = false)
		{
			if (keepsEdges) {
				CopyEdges();
			} else {
				_flowedEdges = _edges;
			}

			var potentials = new long[_n];
			var prevVertexes = new int[_n];
			var prevEdgeIndexes = new int[_n];
			var distances = new long[_n];

			var result = new List<(long flow, long cost)>();
			result.Add((0, 0));
			long prevCostPerFlow = -1;
			bool first = true;
			long minCost = 0;
			long f = flowLimit;
			var que = new DijkstraQ();
			while (f > 0) {
				distances.AsSpan().Fill(INF);
				distances[s] = 0;

				if (first && hasMinusCost) {
					for (int i = 0; i < _n; ++i) {
						bool changes = false;
						for (int v = 0; v < _n; ++v) {
							if (distances[v] == INF) {
								continue;
							}

							var edges = _flowedEdges[v];
							for (int j = 0; j < edges.Count; ++j) {
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

						if (i == _n - 1 && changes) {
							return result;
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

						var edges = _flowedEdges[v];
						for (int j = 0; j < edges.Count; ++j) {
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
					return result;
				}

				for (int v = 0; v < _n; ++v) {
					potentials[v] += distances[v];
				}

				long d = f;
				for (int v = t; v != s; v = prevVertexes[v]) {
					d = Math.Min(d, _flowedEdges[prevVertexes[v]][prevEdgeIndexes[v]].Capacity);
				}

				f -= d;
				minCost += d * potentials[t];
				for (int v = t; v != s; v = prevVertexes[v]) {
					var e = _flowedEdges[prevVertexes[v]][prevEdgeIndexes[v]];
					e.Capacity -= d;
					_flowedEdges[v][e.ReverseEdgeIndex].Capacity += d;
				}

				if (prevCostPerFlow == distances[t]) {
					result.RemoveAt(result.Count - 1);
				}

				result.Add((flowLimit - f, minCost));
				prevCostPerFlow = distances[t];

				first = false;
			}

			return result;
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

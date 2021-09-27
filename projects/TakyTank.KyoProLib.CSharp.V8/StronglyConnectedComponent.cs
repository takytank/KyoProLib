using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V8
{
	public class StronglyConnectedComponent
	{
		private readonly int n_;
		private readonly List<Edge> edges_;

		private readonly int[] components_;
		private int groupCount_;

		public int[] Component => components_;
		public int GroupCount => groupCount_;
		public int VertexCount => n_;

		public StronglyConnectedComponent(int n, int m = 100000)
		{
			n_ = n;
			edges_ = new List<Edge>(m);
			components_ = new int[n];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge(int from, int to) => edges_.Add(new Edge(from, to));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Build()
		{
			var g = new EdgeInfo(n_, edges_);
			int currentOrder = 0;
			groupCount_ = 0;
			var visited = new Stack<int>(n_);
			var lowestOrdersInGroup = new int[n_];
			var orders = new int[n_];
			orders.AsSpan().Fill(-1);

			void Dfs(int v)
			{
				lowestOrdersInGroup[v] = currentOrder;
				orders[v] = currentOrder;
				currentOrder++;
				visited.Push(v);

				for (int i = g.CumulatedFromCount[v]; i < g.CumulatedFromCount[v + 1]; ++i) {
					int to = g.To[i];
					if (orders[to] == -1) {
						Dfs(to);
						lowestOrdersInGroup[v] = Math.Min(lowestOrdersInGroup[v], lowestOrdersInGroup[to]);
					} else {
						lowestOrdersInGroup[v] = Math.Min(lowestOrdersInGroup[v], orders[to]);
					}
				}

				if (lowestOrdersInGroup[v] == orders[v]) {
					while (true) {
						int u = visited.Pop();
						orders[u] = n_;
						components_[u] = groupCount_;

						if (u == v) {
							break;
						}
					}

					++groupCount_;
				}
			}

			for (int i = 0; i < orders.Length; ++i) {
				if (orders[i] == -1) {
					Dfs(i);
				}
			}

			foreach (ref var x in components_.AsSpan()) {
				x = groupCount_ - 1 - x;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int[][] Decompose()
		{
			var counts = new int[groupCount_];
			foreach (var x in components_) {
				++counts[x];
			}

			var temp = new List<int>[groupCount_];
			for (int i = 0; i < groupCount_; ++i) {
				temp[i] = new List<int>(counts[i]);
			}

			for (int i = 0; i < components_.Length; ++i) {
				temp[components_[i]].Add(i);
			}

			var group = new int[groupCount_][];
			for (int i = 0; i < groupCount_; ++i) {
				group[i] = temp[i].ToArray();
			}

			return group;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int[][] Contract()
		{
			var temp = new List<int>[groupCount_];
			for (int i = 0; i < groupCount_; ++i) {
				temp[i] = new List<int>();
			}

			foreach (var e in edges_) {
				int x = components_[e.From];
				int y = components_[e.To];
				if (x == y) {
					continue;
				}

				temp[x].Add(y);
			}

			var graph = new int[groupCount_][];
			for (int i = 0; i < groupCount_; ++i) {
				graph[i] = temp[i].ToArray();
			}

			return graph;
		}

		private class EdgeInfo
		{
			public int[] CumulatedFromCount { get; }
			public int[] To { get; }

			public EdgeInfo(int n, List<Edge> edges)
			{
				CumulatedFromCount = new int[n + 1];
				To = new int[edges.Count];

				foreach (var e in edges) {
					++CumulatedFromCount[e.From + 1];
				}

				for (int i = 1; i <= n; i++) {
					CumulatedFromCount[i] += CumulatedFromCount[i - 1];
				}

				var counter = new int[CumulatedFromCount.Length];
				CumulatedFromCount.CopyTo(counter, 0);
				foreach (var e in edges) {
					To[counter[e.From]] = e.To;
					++counter[e.From];
				}
			}
		}

		private readonly struct Edge
		{
			public int From { get; }
			public int To { get; }

			public Edge(int from, int to)
			{
				From = from;
				To = to;
			}
		}
	}
}

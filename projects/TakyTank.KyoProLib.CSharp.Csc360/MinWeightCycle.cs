using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class MinWeightCycle
	{
		private readonly int n_;
		private readonly bool isDirected_;
		private readonly (int u, int v)[] edges_;
		private readonly Dictionary<int, long>[] to_;
		private int edgeCount_ = 0;

		public MinWeightCycle(int n, int m, bool isDirected)
		{
			n_ = n;
			isDirected_ = isDirected;
			edges_ = new (int u, int v)[m];
			to_ = new Dictionary<int, long>[n];
			for (int i = 0; i < n; i++) {
				to_[i] = new Dictionary<int, long>();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge(int u, int v, long w = 1)
		{
			edges_[edgeCount_] = (u, v);
			++edgeCount_;
			if (isDirected_) {
				if (to_[v].ContainsKey(u) == false) {
					to_[v].Add(u, w);
				} else {
					to_[v][u] = Math.Min(to_[v][u], w);
				}
			} else {
				if (to_[v].ContainsKey(u) == false) {
					to_[v].Add(u, w);
					to_[u].Add(v, w);
				} else {
					to_[v][u] = Math.Min(to_[v][u], w);
					to_[u][v] = Math.Min(to_[u][v], w);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long length, LightList<int> path) Bfs()
		{
			const long inf = long.MaxValue;
			long ans = inf;
			var path = new LightList<int>();
			for (int i = 0; i < edges_.Length; i++) {
				var ret = Bfs(i, ans);
				if (ret.length >= 0) {
					path = ret.path;
					ans = ret.length;
				}
			}

			if (ans == inf) {
				ans = -1;
			}

			return (ans, path);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long length, LightList<int> path) Bfs(int edgeIndex, long limit = long.MaxValue)
		{
			const long inf = long.MaxValue;
			var (u, v) = edges_[edgeIndex];
			long w = to_[v][u];
			if (isDirected_) {
				to_[v].Remove(u);
			} else {
				to_[u].Remove(v);
				to_[v].Remove(u);
			}

			var que = new Queue<int>();
			var distances = new long[n_].Fill(inf);
			var prevs = new int[n_].Fill(-1);
			distances[u] = 0;
			que.Enqueue(u);
			while (que.Count > 0) {
				var cur = que.Dequeue();
				foreach (var next in to_[cur].Keys) {
					if (distances[next] != inf) {
						continue;
					}

					long nd = distances[cur] + w;
					if (nd < distances[next]) {
						distances[next] = nd;
						prevs[next] = cur;
						que.Enqueue(next);
					}
				}
			}

			var path = new LightList<int>();
			long ans = -1;
			if (distances[v] != inf) {
				long temp = distances[v] + 1;
				if (temp < limit) {
					ans = temp;
					path.Add(u);
					for (int cur = v; cur != u; cur = prevs[cur]) {
						path.Add(cur);
					}
				}
			}

			if (isDirected_) {
				to_[v].Add(u, w);
			} else {
				to_[v].Add(u, w);
				to_[u].Add(v, w);
			}

			return (ans, path);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long length, LightList<int> path) Dijkstra()
		{
			const long inf = long.MaxValue;
			long ans = inf;
			var path = new LightList<int>();
			for (int i = 0; i < edges_.Length; i++) {
				var ret = Dijkstra(i, ans);
				if (ret.length >= 0) {
					path = ret.path;
					ans = ret.length;
				}
			}

			if (ans == inf) {
				ans = -1;
			}

			return (ans, path);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long length, LightList<int> path) Dijkstra(int edgeIndex, long limit = long.MaxValue)
		{
			const long inf = long.MaxValue;
			var (u, v) = edges_[edgeIndex];
			long w = to_[v][u];
			if (isDirected_) {
				to_[v].Remove(u);
			} else {
				to_[u].Remove(v);
				to_[v].Remove(u);
			}

			var que = new DijkstraQ();
			var distances = new long[n_].Fill(long.MaxValue);
			var prevs = new int[n_].Fill(-1);
			distances[u] = 0;
			que.Enqueue(0, u);
			while (que.Count > 0) {
				var cur = que.Dequeue();
				if (cur.distance > distances[cur.v]) {
					continue;
				}

				foreach (var next in to_[cur.v]) {
					long nd = cur.distance + next.Value;
					if (nd < distances[next.Key]) {
						distances[next.Key] = nd;
						prevs[next.Key] = cur.v;
						que.Enqueue(nd, next.Key);
					}
				}
			}

			var path = new LightList<int>();
			long ans = -1;
			if (distances[v] != inf) {
				long temp = distances[v] + w;
				if (temp < limit) {
					ans = temp;
					path.Add(u);
					for (int cur = v; cur != u; cur = prevs[cur]) {
						path.Add(cur);
					}
				}
			}

			if (isDirected_) {
				to_[v].Add(u, w);
			} else {
				to_[v].Add(u, w);
				to_[u].Add(v, w);
			}

			return (ans, path);
		}
	}
}

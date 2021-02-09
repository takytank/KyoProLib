using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class ShortestPath
	{
		private readonly int n_;
		private readonly LightList<(int to, long d)>[] edges_;

		public ShortestPath(int n)
		{
			n_ = n;
			edges_ = new LightList<(int v, long d)>[n];
			for (int i = 0; i < n; i++) {
				edges_[i] = new LightList<(int v, long d)>();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge(int p, int q, long d = 1) => edges_[p].Add((q, d));

		public ReadOnlySpan<int> GetPath(int s, int t, int[] prevs)
		{
			var path = new LightList<int>();
			path.Add(t);
			int cur = t;
			while (cur != s) {
				cur = prevs[cur];
				path.Add(cur);
			}

			path.Reverse();
			return path.AsSpan();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long[] Bfs(int start, long inf = long.MaxValue)
		{
			var distances = new long[n_];
			distances.AsSpan().Fill(inf);
			distances[start] = 0;
			var que = new Queue<int>();
			que.Enqueue(start);
			while (que.Count > 0) {
				var cur = que.Dequeue();
				foreach (var (to, d) in edges_[cur].AsSpan()) {
					long nextDistance = distances[cur] + d;
					if (nextDistance < distances[to]) {
						distances[to] = nextDistance;
						que.Enqueue(to);
					}
				}
			}

			return distances;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long[] distances, int[] prevs) BfsWithPath(int start, long inf = long.MaxValue)
		{
			var distances = new long[n_];
			distances.AsSpan().Fill(inf);
			distances[start] = 0;
			var prevs = new int[n_].Fill(-1);
			var que = new Queue<int>();
			que.Enqueue(start);
			while (que.Count > 0) {
				var cur = que.Dequeue();
				foreach (var (to, d) in edges_[cur].AsSpan()) {
					long nextDistance = distances[cur] + d;
					if (nextDistance < distances[to]) {
						distances[to] = nextDistance;
						prevs[to] = cur;
						que.Enqueue(to);
					}
				}
			}

			return (distances, prevs);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long[] Bfs01(int start, long inf = long.MaxValue)
		{
			var distances = new long[n_];
			distances.AsSpan().Fill(inf);
			distances[start] = 0;
			var que0 = new Queue<(int v, long distance)>();
			var que1 = new Queue<(int v, long distance)>();
			que0.Enqueue((start, 0));
			while (que0.Count > 0 || que1.Count > 0) {
				var (v, distance) = que0.Count > 0
					 ? que0.Dequeue()
					 : que1.Dequeue();
				if (distance > distances[v]) {
					continue;
				}

				foreach (var (to, d) in edges_[v].AsSpan()) {
					long nextDistance = distances[v] + d;
					if (nextDistance < distances[to]) {
						distances[to] = nextDistance;
						if (d == 0) {
							que0.Enqueue((to, nextDistance));
						} else {
							que1.Enqueue((to, nextDistance));
						}
					}
				}
			}

			return distances;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long[] distances, int[] prevs) Bfs01WithPath(int start, long inf = long.MaxValue)
		{
			var distances = new long[n_];
			distances.AsSpan().Fill(inf);
			distances[start] = 0;
			var prevs = new int[n_].Fill(-1);
			var que0 = new Queue<(int v, long distance)>();
			var que1 = new Queue<(int v, long distance)>();
			que0.Enqueue((start, 0));
			while (que0.Count > 0 || que1.Count > 0) {
				var (v, distance) = que0.Count > 0
					 ? que0.Dequeue()
					 : que1.Dequeue();
				if (distance > distances[v]) {
					continue;
				}

				foreach (var (to, d) in edges_[v].AsSpan()) {
					long nextDistance = distances[v] + d;
					if (nextDistance < distances[to]) {
						distances[to] = nextDistance;
						prevs[to] = v;
						if (d == 0) {
							que0.Enqueue((to, nextDistance));
						} else {
							que1.Enqueue((to, nextDistance));
						}
					}
				}
			}

			return (distances, prevs);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long[] distances, bool existsNegativeCycle) BellmanFord(
			int start, long inf = long.MaxValue)
		{
			var distances = new long[n_];
			distances.AsSpan().Fill(inf);
			distances[start] = 0;

			bool existsNegativeCycle = false;
			for (int i = 0; i < n_; i++) {
				bool changes = false;
				for (int v = 0; v < n_; v++) {
					if (distances[v] == inf) {
						continue;
					}

					foreach (var (to, d) in edges_[v].AsSpan()) {
						long newDistance = distances[v] + d;
						if (newDistance < distances[to]) {
							changes = true;
							distances[to] = newDistance;
						}
					}
				}

				if (i == n_ - 1) {
					existsNegativeCycle = changes;
				}

				if (changes == false) {
					break;
				}
			}

			return (distances, existsNegativeCycle);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long[] distances, bool existsNegativeCycle, int[] prevs) BellmanFordWithPath(
			int start, long inf = long.MaxValue)
		{
			var distances = new long[n_];
			distances.AsSpan().Fill(inf);
			distances[start] = 0;
			var prevs = new int[n_].Fill(-1);

			bool existsNegativeCycle = false;
			for (int i = 0; i < n_; i++) {
				bool changes = false;
				for (int v = 0; v < n_; v++) {
					if (distances[v] == inf) {
						continue;
					}

					foreach (var (to, d) in edges_[v].AsSpan()) {
						long newDistance = distances[v] + d;
						if (newDistance < distances[to]) {
							changes = true;
							distances[to] = newDistance;
							prevs[to] = v;
						}
					}
				}

				if (i == n_ - 1) {
					existsNegativeCycle = changes;
				}

				if (changes == false) {
					break;
				}
			}

			return (distances, existsNegativeCycle, prevs);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long[] Dijkstra(int start, long inf = long.MaxValue)
		{
			var distances = new long[n_];
			distances.AsSpan().Fill(inf);
			distances[start] = 0;
			var que = new DijkstraQ();
			que.Enqueue(0, start);
			while (que.Count > 0) {
				var (distance, v) = que.Dequeue();
				if (distance > distances[v]) {
					continue;
				}

				foreach (var (to, d) in edges_[v].AsSpan()) {
					long nextDistance = distances[v] + d;
					if (nextDistance < distances[to]) {
						distances[to] = nextDistance;
						que.Enqueue(nextDistance, to);
					}
				}
			}

			return distances;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long[] distances, int[] prevs) DijkstraWithPath(int start, long inf = long.MaxValue)
		{
			var distances = new long[n_];
			distances.AsSpan().Fill(inf);
			distances[start] = 0;
			var prevs = new int[n_].Fill(-1);
			var que = new DijkstraQ();
			que.Enqueue(0, start);
			while (que.Count > 0) {
				var (distance, v) = que.Dequeue();
				if (distance > distances[v]) {
					continue;
				}

				foreach (var (to, d) in edges_[v].AsSpan()) {
					long nextDistance = distances[v] + d;
					if (nextDistance < distances[to]) {
						distances[to] = nextDistance;
						prevs[to] = v;
						que.Enqueue(nextDistance, to);
					}
				}
			}

			return (distances, prevs);
		}

		public long[,] WarshallFloyd(long inf = long.MaxValue / 2)
		{
			var distances = new long[n_, n_];
			for (int i = 0; i < n_; ++i) {
				for (int j = 0; j < n_; ++j) {
					if (i != j) {
						distances[i, j] = inf;
					}
				}
			}

			for (int i = 0; i < n_; i++) {
				foreach (var (j, d) in edges_[i].AsSpan()) {
					distances[i, j] = Math.Min(distances[i, j], d);
				}
			}

			for (int k = 0; k < n_; k++) {
				for (int i = 0; i < n_; i++) {
					for (int j = 0; j < n_; j++) {
						distances[i, j] = Math.Min(
							distances[i, j],
							distances[i, k] + distances[k ,j]);
					}
				}
			}

			return distances;
		}
	}
}

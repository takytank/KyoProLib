using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class GraphBfs
	{
		private readonly int n_;
		private readonly List<int>[] edges_;

		public GraphBfs(int n)
		{
			n_ = n;
			edges_ = new List<int>[n];
			for (int i = 0; i < n; i++) {
				edges_[i] = new List<int>();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge(int p, int q) => edges_[p].Add(q);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long[] MinDistance(int v, long inf = long.MaxValue)
		{
			var distance = new long[n_];
			distance.AsSpan().Fill(inf);
			distance[v] = 0;
			var que = new Queue<int>();
			que.Enqueue(v);
			while (que.Count > 0) {
				var cur = que.Dequeue();
				for (int i = 0; i < edges_[v].Count; i++) {
					var next = edges_[v][i];
					long nextDistance = distance[cur] + 1;
					if (nextDistance < distance[next]) {
						distance[next] = nextDistance;
						que.Enqueue(next);
					}
				}
			}

			return distance;
		}
	}
}

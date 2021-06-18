using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class TopologicalSort
	{
		private readonly int n_;
		private readonly List<int>[] tempEdges_;
		private readonly int[][] edges_;
		private readonly int[] inCounts_;

		public int[][] Edges => edges_;

		public TopologicalSort(int n)
		{
			n_ = n;
			edges_ = new int[n][];
			tempEdges_ = new List<int>[n];
			for (int i = 0; i < n; i++) {
				tempEdges_[i] = new List<int>();
			}

			inCounts_ = new int[n];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge(int p, int q)
		{
			tempEdges_[p].Add(q);
			inCounts_[q]++;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Build()
		{
			for (int i = 0; i < n_; i++) {
				edges_[i] = tempEdges_[i].ToArray();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int[] Sort()
		{
			var counts = new int[n_];
			Array.Copy(inCounts_, counts, n_);

			var done = new bool[n_];
			var list = new List<int>(n_);
			var que = new Queue<int>();
			for (int i = 0; i < n_; i++) {
				if (counts[i] == 0) {
					que.Enqueue(i);
					list.Add(i);
					done[i] = true;
				}
			}

			while (que.Count > 0) {
				int cur = que.Dequeue();
				foreach (var next in edges_[cur]) {
					if (done[next]) {
						return null;
					}

					counts[next]--;
					if (counts[next] == 0) {
						que.Enqueue(next);
						list.Add(next);
						done[next] = true;
					}
				}
			}

			if (list.Count != n_) {
				return null;
			}

			var sorted = list.ToArray();
			return sorted;
		}
	}
}

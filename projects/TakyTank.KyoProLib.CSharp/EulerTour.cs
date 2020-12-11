using System;
using System.Collections.Generic;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class EulerTour
	{
		private readonly int vertexCount_;
		private readonly int tourCount_;
		private readonly int[] tour_;
		private readonly int[] discovery_;
		private readonly int[] finish_;
		private readonly long[] costAccum_;
		private readonly (int depth, int index)[] depthTree_;

		public EulerTour(List<(int v, long cost)>[] g, int root = 0)
		{
			vertexCount_ = g.Length;
			tourCount_ = (vertexCount_ << 1) - 1;
			tour_ = new int[tourCount_];
			discovery_ = new int[vertexCount_];
			finish_ = new int[vertexCount_];
			costAccum_ = new long[tourCount_];
			depthTree_ = new (int depth, int index)[tourCount_ << 1];

			void Dfs(int v, int parent, int depth, ref int index)
			{
				discovery_[v] = index;
				tour_[index] = v;
				depthTree_[index + tourCount_] = (depth, index);
				++index;
				for (int i = 0; i < g[v].Count; i++) {
					if (g[v][i].v == parent) {
						continue;
					}

					costAccum_[index] = g[v][i].cost;
					Dfs(g[v][i].v, v, depth + 1, ref index);

					tour_[index] = v;
					depthTree_[index + tourCount_] = (depth, index);
					costAccum_[index] = -g[v][i].cost;
					++index;
				}

				finish_[v] = index;
			}

			int index = 0;
			Dfs(root, -1, 0, ref index);

			for (int i = 0; i < tourCount_ - 1; i++) {
				costAccum_[i + 1] += costAccum_[i];
			}

			for (int i = tourCount_ - 1; i > 0; i--) {
				var l = depthTree_[i << 1];
				var r = depthTree_[(i << 1) + 1];
				depthTree_[i] = l.depth < r.depth ? l : r;
			}
		}

		public int Lca(int u, int v)
		{
			int left = Math.Min(discovery_[u], discovery_[v]);
			int right = Math.Max(discovery_[u], discovery_[v]) + 1;

			int l = left + tourCount_;
			int r = right + tourCount_;
			(int depth, int index) valL = (int.MaxValue, 0);
			(int depth, int index) valR = (int.MaxValue, 0);
			while (l < r) {
				if ((l & 1) != 0) {
					if (depthTree_[l].depth < valL.depth) {
						valL = depthTree_[l];
					}

					++l;
				}

				if ((r & 1) != 0) {
					--r;
					if (depthTree_[r].depth < valR.depth) {
						valR = depthTree_[r];
					}
				}

				l >>= 1;
				r >>= 1;
			}

			return tour_[valL.depth < valR.depth ? valL.index : valR.index];
		}

		public long Cost(int u, int v)
		{
			int lca = Lca(u, v);
			return costAccum_[u + 1] + costAccum_[v + 1] - (costAccum_[lca + 1] * 2);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

	public class EulerTourSubTreeE<TEdge>
	{
		private readonly int vertexCount_;
		private readonly int tourCount_;

		private readonly int[] tour_;
		private readonly int[] discovery_;
		private readonly int[] finish_;
		private readonly TEdge[] edgeTree_;
		private readonly Func<TEdge, TEdge, TEdge> mergeEdge_;
		private readonly TEdge edgeUnit_;
		private readonly Dictionary<(int p, int q), int> edgeMap_;
		
		public EulerTourSubTreeE(
			List<(int v, TEdge cost)>[] g,
			TEdge edgeUnit,
			Func<TEdge, TEdge, TEdge> mergeEdge,
			int root = 0)
		{
			vertexCount_ = g.Length;
			tourCount_ = vertexCount_ << 1;
			tour_ = new int[tourCount_];
			discovery_ = new int[vertexCount_];
			finish_ = new int[vertexCount_];
			edgeTree_ = new TEdge[tourCount_ << 1];
			mergeEdge_ = mergeEdge;
			edgeUnit_ = edgeUnit;
			edgeMap_ = new Dictionary<(int p, int q), int>();

			void Dfs(int v, int parent, int depth, ref int index)
			{
				discovery_[v] = index;
				tour_[index] = v;
				++index;
				for (int i = 0; i < g[v].Count; i++) {
					if (g[v][i].v == parent) {
						continue;
					}

					edgeTree_[index + tourCount_] = g[v][i].cost;
					edgeMap_[(v, g[v][i].v)] = g[v][i].v;
					Dfs(g[v][i].v, v, depth + 1, ref index);

					++index;
				}

				tour_[index] = -v;
				edgeTree_[index + tourCount_] = edgeUnit;
				finish_[v] = index;
			}

			int index = 0;
			Dfs(root, -1, 0, ref index);

			for (int i = tourCount_ - 1; i > 0; i--) {
				var l = edgeTree_[i << 1];
				var r = edgeTree_[(i << 1) + 1];
				edgeTree_[i] = mergeEdge(l, r);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TEdge QueryEdge(int v)
		{
			int left = discovery_[v];
			int right = finish_[v];
			if (left > right || right < 0 || left >= tourCount_) {
				return edgeUnit_;
			}

			int l = left + tourCount_;
			int r = right + tourCount_;
			TEdge valL = edgeUnit_;
			TEdge valR = edgeUnit_;
			while (l < r) {
				if ((l & 1) != 0) {
					valL = mergeEdge_(valL, edgeTree_[l]);
					++l;
				}
				if ((r & 1) != 0) {
					--r;
					valR = mergeEdge_(edgeTree_[r], valR);
				}

				l >>= 1;
				r >>= 1;
			}

			return mergeEdge_(valL, valR);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void UpdateEdge(int p, int q, TEdge value)
		{
			if (edgeMap_.ContainsKey((p, q))) {
				UpdateEdge(edgeMap_[(p, q)], value);
			} else if (edgeMap_.ContainsKey((q, p))) {
				UpdateEdge(edgeMap_[(q, p)], value);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void UpdateEdge(int v, TEdge value)
		{
			if (v >= tourCount_) {
				return;
			}

			int index = discovery_[v] + tourCount_;
			edgeTree_[index] = value;
			index >>= 1;
			while (index != 0) {
				edgeTree_[index] = mergeEdge_(edgeTree_[index << 1], edgeTree_[(index << 1) + 1]);
				index >>= 1;
			}
		}
	}

	public class EulerTourSubTreeV<TVertex>
	{
		private readonly int vertexCount_;
		private readonly int tourCount_;

		private readonly int[] tour_;
		private readonly int[] discovery_;
		private readonly int[] finish_;
		private readonly TVertex[] vertexTree_;
		private readonly Func<TVertex, TVertex, TVertex> mergeVertex_;
		private readonly TVertex vertexUnit_;

		public EulerTourSubTreeV(
			List<int>[] g,
			TVertex[] vertexes,
			TVertex vertexUnit,
			Func<TVertex, TVertex, TVertex> mergeVertex,
			int root = 0)
		{
			vertexCount_ = g.Length;
			tourCount_ = vertexCount_ << 1;
			tour_ = new int[tourCount_];
			discovery_ = new int[vertexCount_];
			finish_ = new int[vertexCount_];
			vertexTree_ = new TVertex[tourCount_ << 1];
			mergeVertex_ = mergeVertex;
			vertexUnit_ = vertexUnit;

			void Dfs(int v, int parent, int depth, ref int index)
			{
				vertexTree_[index + tourCount_] = vertexes[v];
				discovery_[v] = index;
				tour_[index] = v;
				++index;
				for (int i = 0; i < g[v].Count; i++) {
					if (g[v][i] == parent) {
						continue;
					}

					
					Dfs(g[v][i], v, depth + 1, ref index);

					++index;
				}

				tour_[index] = -v;
				vertexTree_[index + tourCount_] = vertexUnit;
				finish_[v] = index;
			}

			int index = 0;
			Dfs(root, -1, 0, ref index);

			for (int i = tourCount_ - 1; i > 0; i--) {
				var l = vertexTree_[i << 1];
				var r = vertexTree_[(i << 1) + 1];
				vertexTree_[i] = mergeVertex(l, r);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TVertex QueryEdge(int v)
		{
			int left = discovery_[v];
			int right = finish_[v];
			if (left > right || right < 0 || left >= tourCount_) {
				return vertexUnit_;
			}

			int l = left + tourCount_;
			int r = right + tourCount_;
			TVertex valL = vertexUnit_;
			TVertex valR = vertexUnit_;
			while (l < r) {
				if ((l & 1) != 0) {
					valL = mergeVertex_(valL, vertexTree_[l]);
					++l;
				}
				if ((r & 1) != 0) {
					--r;
					valR = mergeVertex_(vertexTree_[r], valR);
				}

				l >>= 1;
				r >>= 1;
			}

			return mergeVertex_(valL, valR);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void UpdateEdge(int v, TVertex value)
		{
			if (v >= tourCount_) {
				return;
			}

			int index = discovery_[v] + tourCount_;
			vertexTree_[index] = value;
			index >>= 1;
			while (index != 0) {
				vertexTree_[index] = mergeVertex_(vertexTree_[index << 1], vertexTree_[(index << 1) + 1]);
				index >>= 1;
			}
		}
	}
}

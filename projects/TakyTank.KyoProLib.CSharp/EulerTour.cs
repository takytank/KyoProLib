using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class EulerTourPath
	{
		private readonly int vertexCount_;
		private readonly int tourCount_;
		private readonly int depthCount_;

		private readonly int[] tour_;
		private readonly int[] discovery_;
		private readonly int[] finish_;

		private readonly long[] edgeTree_;
		private readonly Dictionary<(int p, int q), int> edgeMap_;

		private readonly long[] vertexTree_;

		private readonly (int depth, int v)[] depthTree_;

		public EulerTourPath(
			int n,
			(int p, int q, long cost)[] edges,
			long[] vertexes,
			int root = 0)
		{
			vertexCount_ = n;
			tourCount_ = vertexCount_ << 1;
			depthCount_ = (vertexCount_ << 1) - 1;

			tour_ = new int[tourCount_];
			discovery_ = new int[vertexCount_];
			finish_ = new int[vertexCount_];

			edgeTree_ = new long[tourCount_ << 1];
			edgeMap_ = new Dictionary<(int p, int q), int>();
			vertexTree_ = new long[tourCount_ << 1];
			depthTree_ = new (int depth, int index)[(depthCount_ << 1)];

			var g = new List<(int v, long cost)>[vertexCount_];
			for (int i = 0; i < vertexCount_; i++) {
				g[i] = new List<(int v, long cost)>();
			}

			foreach (var (p, q, cost) in edges) {
				g[p].Add((q, cost));
				g[q].Add((p, cost));
			}

			void Dfs(int v, int parent, int depth, ref int index)
			{
				discovery_[v] = index;
				tour_[index] = v;
				vertexTree_[index + tourCount_] = vertexes is null ? 0 : vertexes[v];
				depthTree_[index + depthCount_] = (depth, v);
				++index;
				for (int i = 0; i < g[v].Count; i++) {
					if (g[v][i].v == parent) {
						continue;
					}

					edgeTree_[index + tourCount_] = g[v][i].cost;
					edgeMap_[(v, g[v][i].v)] = g[v][i].v;
					Dfs(g[v][i].v, v, depth + 1, ref index);

					depthTree_[index + depthCount_] = (depth, v);
					++index;
				}

				tour_[index] = -v;
				finish_[v] = index;
				vertexTree_[index + tourCount_] = -(vertexTree_[discovery_[v] + tourCount_]);
				edgeTree_[index + tourCount_] = -(edgeTree_[discovery_[v] + tourCount_]);
			}

			int index = 0;
			Dfs(root, -1, 0, ref index);

			for (int i = tourCount_ - 1; i > 0; i--) {
				var l = edgeTree_[i << 1];
				var r = edgeTree_[(i << 1) + 1];
				edgeTree_[i] = l + r;
			}

			for (int i = tourCount_ - 1; i > 0; i--) {
				var l = vertexTree_[i << 1];
				var r = vertexTree_[(i << 1) + 1];
				vertexTree_[i] = l + r;
			}

			for (int i = depthCount_ - 1; i > 0; i--) {
				var l = depthTree_[i << 1];
				var r = depthTree_[(i << 1) + 1];
				depthTree_[i] = l.depth < r.depth ? l : r;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long Vertex(int v) => vertexTree_[v + tourCount_];

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long Edge(int p, int q)
		{
			if (edgeMap_.ContainsKey((p, q))) {
				return edgeTree_[edgeMap_[(p, q)] + tourCount_];
			} else if (edgeMap_.ContainsKey((q, p))) {
				return edgeTree_[edgeMap_[(q, p)] + tourCount_];
			} else {
				return 0;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Lca(int u, int v)
		{
			int left = Math.Min(discovery_[u], discovery_[v]);
			int right = Math.Max(discovery_[u], discovery_[v]) + 1;

			int l = left + depthCount_;
			int r = right + depthCount_;
			(int depth, int v) valL = (int.MaxValue, 0);
			(int depth, int v) valR = (int.MaxValue, 0);
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

			return valL.depth < valR.depth ? valL.v : valR.v;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long QueryVertex(int p, int q)
		{
			int left = Math.Min(discovery_[p], discovery_[q]);
			int right = Math.Max(discovery_[p], discovery_[q]) + 1;
			if (left > right || right < 0 || left >= tourCount_) {
				return 0;
			}

			int l = left + tourCount_;
			int r = right + tourCount_;
			long valL = 0;
			long valR = 0;
			while (l < r) {
				if ((l & 1) != 0) {
					valL += vertexTree_[l];
					++l;
				}
				if ((r & 1) != 0) {
					--r;
					valR += vertexTree_[r];
				}

				l >>= 1;
				r >>= 1;
			}

			return valL + valR;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void UpdateVertex(int v, long value)
		{
			if (v >= tourCount_) {
				return;
			}

			int index = discovery_[v] + tourCount_;
			vertexTree_[index] = value;
			index >>= 1;
			while (index != 0) {
				vertexTree_[index] = vertexTree_[index << 1] + vertexTree_[(index << 1) + 1];
				index >>= 1;
			}

			index = finish_[v] + tourCount_;
			vertexTree_[index] = -value;
			index >>= 1;
			while (index != 0) {
				vertexTree_[index] = vertexTree_[index << 1] + vertexTree_[(index << 1) + 1];
				index >>= 1;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long QueryEdge(int p, int q)
		{
			int left = Math.Min(discovery_[p], discovery_[q]) + 1;
			int right = Math.Max(discovery_[p], discovery_[q]) + 1;
			if (left > right || right < 0 || left >= tourCount_) {
				return 0;
			}

			int l = left + tourCount_;
			int r = right + tourCount_;
			long valL = 0;
			long valR = 0;
			while (l < r) {
				if ((l & 1) != 0) {
					valL += edgeTree_[l];
					++l;
				}
				if ((r & 1) != 0) {
					--r;
					valR += edgeTree_[r];
				}

				l >>= 1;
				r >>= 1;
			}

			return valL + valR;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void UpdateEdge(int p, int q, long value)
		{
			if (edgeMap_.ContainsKey((p, q))) {
				UpdateEdge(edgeMap_[(p, q)], value);
			} else if (edgeMap_.ContainsKey((q, p))) {
				UpdateEdge(edgeMap_[(q, p)], value);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void UpdateEdge(int v, long value)
		{
			if (v >= tourCount_) {
				return;
			}

			int index = discovery_[v] + tourCount_;
			edgeTree_[index] = value;
			index >>= 1;
			while (index != 0) {
				edgeTree_[index] = edgeTree_[index << 1] + edgeTree_[(index << 1) + 1];
				index >>= 1;
			}

			index = finish_[v] + tourCount_;
			edgeTree_[index] = -value;
			index >>= 1;
			while (index != 0) {
				edgeTree_[index] = edgeTree_[index << 1] + edgeTree_[(index << 1) + 1];
				index >>= 1;
			}
		}
	}

	public class EulerTourPath<TVertex, TEdge>
	{
		private readonly int vertexCount_;
		private readonly int tourCount_;
		private readonly int depthCount_;

		private readonly int[] tour_;
		private readonly int[] discovery_;
		private readonly int[] finish_;

		private readonly TEdge[] edgeTree_;
		private readonly TEdge edgeUnit_;
		private readonly Func<TEdge, TEdge, TEdge> mergeEdge_;
		private readonly Func<TEdge, TEdge> inverseEdge_;
		private readonly Dictionary<(int p, int q), int> edgeMap_;

		private readonly TVertex[] vertexTree_;
		private readonly TVertex vertexUnit_;
		private readonly Func<TVertex, TVertex, TVertex> mergeVertex_;
		private readonly Func<TVertex, TVertex> inverseVertex_;

		private readonly (int depth, int v)[] depthTree_;

		public EulerTourPath(
			int n,
			(int p, int q, TEdge cost)[] edges,
			TEdge edgeUnit,
			Func<TEdge, TEdge, TEdge> mergeEdge,
			Func<TEdge, TEdge> inverseEdge,
			TVertex[] vertexes,
			TVertex vertexUnit,
			Func<TVertex, TVertex, TVertex> mergeVertex,
			Func<TVertex, TVertex> inverseVertex,
			int root = 0)
		{
			vertexCount_ = n;
			tourCount_ = vertexCount_ << 1;
			depthCount_ = (vertexCount_ << 1) - 1;

			tour_ = new int[tourCount_];
			discovery_ = new int[vertexCount_];
			finish_ = new int[vertexCount_];

			edgeTree_ = new TEdge[tourCount_ << 1];
			edgeUnit_ = edgeUnit;
			mergeEdge_ = mergeEdge;
			inverseEdge_ = inverseEdge;
			edgeMap_ = new Dictionary<(int p, int q), int>();

			vertexTree_ = new TVertex[tourCount_ << 1];
			vertexUnit_ = vertexUnit;
			mergeVertex_ = mergeVertex;
			inverseVertex_ = inverseVertex;

			depthTree_ = new (int depth, int index)[(depthCount_ << 1)];

			var g = new List<(int v, TEdge cost)>[vertexCount_];
			for (int i = 0; i < vertexCount_; i++) {
				g[i] = new List<(int v, TEdge cost)>();
			}

			foreach (var (p, q, cost) in edges) {
				g[p].Add((q, cost));
				g[q].Add((p, cost));
			}

			void Dfs(int v, int parent, int depth, ref int index)
			{
				discovery_[v] = index;
				tour_[index] = v;
				vertexTree_[index + tourCount_] = vertexes is null ? vertexUnit : vertexes[v];
				depthTree_[index + depthCount_] = (depth, v);
				++index;
				for (int i = 0; i < g[v].Count; i++) {
					if (g[v][i].v == parent) {
						continue;
					}

					edgeTree_[index + tourCount_] = g[v][i].cost;
					edgeMap_[(v, g[v][i].v)] = g[v][i].v;
					Dfs(g[v][i].v, v, depth + 1, ref index);

					depthTree_[index + depthCount_] = (depth, v);
					++index;
				}

				tour_[index] = -v;
				finish_[v] = index;
				vertexTree_[index + tourCount_]
					= inverseVertex_(vertexTree_[discovery_[v] + tourCount_]);
				edgeTree_[index + tourCount_] = inverseEdge_(edgeTree_[discovery_[v] + tourCount_]);
			}

			int index = 0;
			Dfs(root, -1, 0, ref index);

			for (int i = tourCount_ - 1; i > 0; i--) {
				var l = edgeTree_[i << 1];
				var r = edgeTree_[(i << 1) + 1];
				edgeTree_[i] = mergeEdge(l, r);
			}

			for (int i = tourCount_ - 1; i > 0; i--) {
				var l = vertexTree_[i << 1];
				var r = vertexTree_[(i << 1) + 1];
				vertexTree_[i] = mergeVertex(l, r);
			}

			for (int i = depthCount_ - 1; i > 0; i--) {
				var l = depthTree_[i << 1];
				var r = depthTree_[(i << 1) + 1];
				depthTree_[i] = l.depth < r.depth ? l : r;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TVertex Vertex(int v) => vertexTree_[v + tourCount_];

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TEdge Edge(int p, int q)
		{
			if (edgeMap_.ContainsKey((p, q))) {
				return edgeTree_[edgeMap_[(p, q)] + tourCount_];
			} else if (edgeMap_.ContainsKey((q, p))) {
				return edgeTree_[edgeMap_[(q, p)] + tourCount_];
			} else {
				return edgeUnit_;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Lca(int u, int v)
		{
			int left = Math.Min(discovery_[u], discovery_[v]);
			int right = Math.Max(discovery_[u], discovery_[v]) + 1;

			int l = left + depthCount_;
			int r = right + depthCount_;
			(int depth, int v) valL = (int.MaxValue, 0);
			(int depth, int v) valR = (int.MaxValue, 0);
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

			return valL.depth < valR.depth ? valL.v : valR.v;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TVertex QueryVertex(int p, int q)
		{
			int left = Math.Min(discovery_[p], discovery_[q]);
			int right = Math.Max(discovery_[p], discovery_[q]) + 1;
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
		public void UpdateVertex(int v, TVertex value)
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

			index = finish_[v] + tourCount_;
			vertexTree_[index] = inverseVertex_(value);
			index >>= 1;
			while (index != 0) {
				vertexTree_[index] = mergeVertex_(vertexTree_[index << 1], vertexTree_[(index << 1) + 1]);
				index >>= 1;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TEdge QueryEdge(int p, int q)
		{
			int left = Math.Min(discovery_[p], discovery_[q]) + 1;
			int right = Math.Max(discovery_[p], discovery_[q]) + 1;
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

			index = finish_[v] + tourCount_;
			edgeTree_[index] = inverseEdge_(value);
			index >>= 1;
			while (index != 0) {
				edgeTree_[index] = mergeEdge_(edgeTree_[index << 1], edgeTree_[(index << 1) + 1]);
				index >>= 1;
			}
		}
	}

	public class EulerTourSubTree
	{
		private readonly int vertexCount_;
		private readonly int tourCount_;

		private readonly int[] tour_;
		private readonly int[] discovery_;
		private readonly int[] finish_;

		private readonly long[] edgeTree_;
		private readonly Dictionary<(int p, int q), int> edgeMap_;
		private readonly long[] vertexTree_;

		public EulerTourSubTree(
			int n,
			(int p, int q, long cost)[] edges,
			long[] vertexes,
			int root = 0)
		{
			vertexCount_ = n;
			tourCount_ = vertexCount_ << 1;

			tour_ = new int[tourCount_];
			discovery_ = new int[vertexCount_];
			finish_ = new int[vertexCount_];

			edgeTree_ = new long[tourCount_ << 1];
			edgeMap_ = new Dictionary<(int p, int q), int>();
			vertexTree_ = new long[tourCount_ << 1];

			var g = new List<(int v, long cost)>[vertexCount_];
			for (int i = 0; i < vertexCount_; i++) {
				g[i] = new List<(int v, long cost)>();
			}

			foreach (var (p, q, cost) in edges) {
				g[p].Add((q, cost));
				g[q].Add((p, cost));
			}

			void Dfs(int v, int parent, int depth, ref int index)
			{
				discovery_[v] = index;
				tour_[index] = v;
				vertexTree_[index + tourCount_] = vertexes is null ? 0 : vertexes[v];
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
				finish_[v] = index;
				vertexTree_[index + tourCount_] = 0;
				edgeTree_[index + tourCount_] = 0;
			}

			int index = 0;
			Dfs(root, -1, 0, ref index);

			for (int i = tourCount_ - 1; i > 0; i--) {
				var l = edgeTree_[i << 1];
				var r = edgeTree_[(i << 1) + 1];
				edgeTree_[i] = l + r;
			}

			for (int i = tourCount_ - 1; i > 0; i--) {
				var l = vertexTree_[i << 1];
				var r = vertexTree_[(i << 1) + 1];
				vertexTree_[i] = l + r;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long Vertex(int v) => vertexTree_[v + tourCount_];

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long Edge(int p, int q)
		{
			if (edgeMap_.ContainsKey((p, q))) {
				return edgeTree_[edgeMap_[(p, q)] + tourCount_];
			} else if (edgeMap_.ContainsKey((q, p))) {
				return edgeTree_[edgeMap_[(q, p)] + tourCount_];
			} else {
				return 0;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long QueryVertex(int v)
		{
			int left = discovery_[v];
			int right = finish_[v];
			if (left > right || right < 0 || left >= tourCount_) {
				return 0;
			}

			int l = left + tourCount_;
			int r = right + tourCount_;
			long valL = 0;
			long valR = 0;
			while (l < r) {
				if ((l & 1) != 0) {
					valL += vertexTree_[l];
					++l;
				}
				if ((r & 1) != 0) {
					--r;
					valR += vertexTree_[r];
				}

				l >>= 1;
				r >>= 1;
			}

			return valL + valR;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void UpdateVertex(int v, long value)
		{
			if (v >= tourCount_) {
				return;
			}

			int index = discovery_[v] + tourCount_;
			vertexTree_[index] = value;
			index >>= 1;
			while (index != 0) {
				vertexTree_[index] = vertexTree_[index << 1] + vertexTree_[(index << 1) + 1];
				index >>= 1;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long QueryEdge(int v)
		{
			int left = discovery_[v] + 1;
			int right = finish_[v];
			if (left > right || right < 0 || left >= tourCount_) {
				return 0;
			}

			int l = left + tourCount_;
			int r = right + tourCount_;
			long valL = 0;
			long valR = 0;
			while (l < r) {
				if ((l & 1) != 0) {
					valL += edgeTree_[l];
					++l;
				}
				if ((r & 1) != 0) {
					--r;
					valR += edgeTree_[r];
				}

				l >>= 1;
				r >>= 1;
			}

			return valL + valR;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void UpdateEdge(int p, int q, long value)
		{
			if (edgeMap_.ContainsKey((p, q))) {
				UpdateEdge(edgeMap_[(p, q)], value);
			} else if (edgeMap_.ContainsKey((q, p))) {
				UpdateEdge(edgeMap_[(q, p)], value);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void UpdateEdge(int v, long value)
		{
			if (v >= tourCount_) {
				return;
			}

			int index = discovery_[v] + tourCount_;
			edgeTree_[index] = value;
			index >>= 1;
			while (index != 0) {
				edgeTree_[index] = edgeTree_[index << 1] + edgeTree_[(index << 1) + 1];
				index >>= 1;
			}
		}
	}

	public class EulerTourSubTree<TVertex, TEdge>
	{
		private readonly int vertexCount_;
		private readonly int tourCount_;

		private readonly int[] tour_;
		private readonly int[] discovery_;
		private readonly int[] finish_;

		private readonly TEdge[] edgeTree_;
		private readonly TEdge edgeUnit_;
		private readonly Func<TEdge, TEdge, TEdge> mergeEdge_;
		private readonly Dictionary<(int p, int q), int> edgeMap_;

		private readonly TVertex[] vertexTree_;
		private readonly TVertex vertexUnit_;
		private readonly Func<TVertex, TVertex, TVertex> mergeVertex_;

		public EulerTourSubTree(
			int n,
			(int p, int q, TEdge cost)[] edges,
			TEdge edgeUnit,
			Func<TEdge, TEdge, TEdge> mergeEdge,
			TVertex[] vertexes,
			TVertex vertexUnit,
			Func<TVertex, TVertex, TVertex> mergeVertex,
			int root = 0)
		{
			vertexCount_ = n;
			tourCount_ = vertexCount_ << 1;

			tour_ = new int[tourCount_];
			discovery_ = new int[vertexCount_];
			finish_ = new int[vertexCount_];

			edgeTree_ = new TEdge[tourCount_ << 1];
			edgeUnit_ = edgeUnit;
			mergeEdge_ = mergeEdge;
			edgeMap_ = new Dictionary<(int p, int q), int>();

			vertexTree_ = new TVertex[tourCount_ << 1];
			vertexUnit_ = vertexUnit;
			mergeVertex_ = mergeVertex;

			var g = new List<(int v, TEdge cost)>[vertexCount_];
			for (int i = 0; i < vertexCount_; i++) {
				g[i] = new List<(int v, TEdge cost)>();
			}

			foreach (var (p, q, cost) in edges) {
				g[p].Add((q, cost));
				g[q].Add((p, cost));
			}

			void Dfs(int v, int parent, int depth, ref int index)
			{
				discovery_[v] = index;
				tour_[index] = v;
				vertexTree_[index + tourCount_] = vertexes is null ? vertexUnit : vertexes[v];
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
				finish_[v] = index;
				vertexTree_[index + tourCount_] = vertexUnit;
				edgeTree_[index + tourCount_] = edgeUnit;
			}

			int index = 0;
			Dfs(root, -1, 0, ref index);

			for (int i = tourCount_ - 1; i > 0; i--) {
				var l = edgeTree_[i << 1];
				var r = edgeTree_[(i << 1) + 1];
				edgeTree_[i] = mergeEdge(l, r);
			}

			for (int i = tourCount_ - 1; i > 0; i--) {
				var l = vertexTree_[i << 1];
				var r = vertexTree_[(i << 1) + 1];
				vertexTree_[i] = mergeVertex(l, r);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TVertex Vertex(int v) => vertexTree_[v + tourCount_];

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TEdge Edge(int p, int q)
		{
			if (edgeMap_.ContainsKey((p, q))) {
				return edgeTree_[edgeMap_[(p, q)] + tourCount_];
			} else if (edgeMap_.ContainsKey((q, p))) {
				return edgeTree_[edgeMap_[(q, p)] + tourCount_];
			} else {
				return edgeUnit_;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TVertex QueryVertex(int v)
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
		public void UpdateVertex(int v, TVertex value)
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TEdge QueryEdge(int v)
		{
			int left = discovery_[v] + 1;
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
}

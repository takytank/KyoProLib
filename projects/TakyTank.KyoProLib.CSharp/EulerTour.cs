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
		private readonly int depthCount_;

		private readonly int[] tour_;
		private readonly int[] discovery_;
		private readonly int[] finish_;
		private readonly int[] depth_;
		private readonly List<int>[] graph_;
		private readonly Dictionary<(int p, int q), int> edgeMap_;
		private readonly (int depth, int v)[] depthTree_;

		public int Count => tourCount_;
		public int[] Discovery => discovery_;
		public int[] Finish => finish_;

		public EulerTour(int n)
		{
			vertexCount_ = n;
			tourCount_ = vertexCount_ << 1;
			depthCount_ = (vertexCount_ << 1) - 1;

			tour_ = new int[tourCount_];
			discovery_ = new int[vertexCount_];
			finish_ = new int[vertexCount_];
			depth_ = new int[vertexCount_];
			graph_ = new List<int>[vertexCount_];
			for (int i = 0; i < n; i++) {
				graph_[i] = new List<int>();
			}

			edgeMap_ = new Dictionary<(int p, int q), int>();
			depthTree_ = new (int depth, int index)[(depthCount_ << 1)];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge(int p, int q)
		{
			graph_[p].Add(q);
			graph_[q].Add(p);
		}

		public void Build(int root = 0)
		{
			void Dfs(int v, int parent, int depth, ref int index)
			{
				discovery_[v] = index;
				tour_[index] = v;
				depthTree_[index + depthCount_] = (depth, v);
				depth_[v] = depth;
				++index;
				for (int i = 0; i < graph_[v].Count; i++) {
					if (graph_[v][i] == parent) {
						continue;
					}

					edgeMap_[(v, graph_[v][i])] = graph_[v][i];
					Dfs(graph_[v][i], v, depth + 1, ref index);
					depthTree_[index + depthCount_] = (depth, v);

					++index;
				}

				tour_[index] = -v;
				finish_[v] = index;
			}

			int index2 = 0;
			Dfs(root, -1, 0, ref index2);

			for (int i = depthCount_ - 1; i > 0; i--) {
				var l = depthTree_[i << 1];
				var r = depthTree_[(i << 1) + 1];
				depthTree_[i] = l.depth < r.depth ? l : r;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int IndexOfVertex(int v) => discovery_[v];
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Vertex(int v, Action<int> action)
			=> action.Invoke(IndexOfVertex(v));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int discovery, int finish) IndexOfEdge(int p, int q)
		{
			int v = 0;
			if (edgeMap_.ContainsKey((p, q))) {
				v = edgeMap_[(p, q)];
			} else if (edgeMap_.ContainsKey((q, p))) {
				v = edgeMap_[(q, p)];
			}

			return (discovery_[v], finish_[v]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Edge(int p, int q, Action<(int discovery, int finish)> action)
			=> action.Invoke(IndexOfEdge(p, q));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int l, int r) IndexOfSubTreeVertex(int v)
			=> (discovery_[v], finish_[v]);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SubTreeVertex(int v, Action<(int l, int r)> action)
			=> action.Invoke(IndexOfSubTreeVertex(v));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int l, int r) IndexOfSubTreeEdge(int v)
			=> (discovery_[v] + 1, finish_[v]);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SubTreeEdge(int v, Action<(int l, int r)> action)
			=> action.Invoke(IndexOfSubTreeEdge(v));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int l1, int r1, int l2, int r2) IndexOfPathVertex(int p, int q)
		{
			var lca = LeastCommonAnsestor(p, q);
			return (discovery_[lca], discovery_[p], discovery_[lca] + 1, discovery_[q]);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void PathVertex(int p, int q, Action<(int l1, int r1, int l2, int r2)> action)
			=> action.Invoke(IndexOfPathVertex(p, q));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int l1, int r1, int l2, int r2) IndexOfPathEdge(int p, int q)
		{
			var lca = LeastCommonAnsestor(p, q);
			return (discovery_[lca] + 1, discovery_[p], discovery_[lca] + 1, discovery_[q]);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void PathEdge(int p, int q, Action<(int l1, int r1, int l2, int r2)> action)
			=> action.Invoke(IndexOfPathEdge(p, q));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Lca(int u, int v) => LeastCommonAnsestor(u, v);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int LeastCommonAnsestor(int u, int v)
		{
			int left = Math.Min(discovery_[u], discovery_[v]);
			int right = Math.Max(discovery_[u], discovery_[v]) + 1;

			int l = left + depthCount_;
			int r = right + depthCount_;
			(int depth, int v) valL = (int.MaxValue, u);
			(int depth, int v) valR = (int.MaxValue, u);
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
		public int Distance(int u, int v)
			=> depth_[u] + depth_[v] - depth_[Lca(u, v)] * 2;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int root, Dictionary<int, List<(int v, int d)>> tree) AuxiliaryTree(int[] vertices)
		{
			int n = vertices.Length;
			Array.Sort(vertices, (x, y) => discovery_[x].CompareTo(discovery_[y]));

			var stack = new Stack<int>();
			stack.Push(vertices[0]);
			var tree = new Dictionary<int, List<(int v, int d)>> {
				[vertices[0]] = new List<(int v, int d)>()
			};

			void AddEdge(int p, int q)
			{
				int d = Math.Abs(depth_[p] - depth_[q]);
				tree[p].Add((q, d));
				tree[q].Add((p, d));
			}

			for (int i = 0; i < n - 1; ++i) {
				int lca = Lca(vertices[i], vertices[i + 1]);
				if (tree.ContainsKey(lca) == false) {
					tree[lca] = new List<(int v, int d)>();
				}

				if (lca != vertices[i]) {
					int last = stack.Pop();
					while (stack.Count > 0 && depth_[lca] < depth_[stack.Peek()]) {
						int top = stack.Peek();
						if (tree.ContainsKey(top) == false) {
							tree[top] = new List<(int v, int d)>();
						}

						AddEdge(top, last);
						last = stack.Pop();
					}

					if (stack.Count == 0 || stack.Peek() != lca) {
						stack.Push(lca);
					}

					AddEdge(lca, last);
				}

				stack.Push(vertices[i + 1]);
				if (tree.ContainsKey(vertices[i + 1]) == false) {
					tree[vertices[i + 1]] = new List<(int v, int d)>();
				}
			}

			while (stack.Count > 1) {
				int v = stack.Pop();
				if (stack.Count > 0) {
					AddEdge(v, stack.Peek());
				}
			}

			return (stack.Peek(), tree);
		}
	}
}

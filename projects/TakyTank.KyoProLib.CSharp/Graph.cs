using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class Graph
	{
		private readonly int n_;
		private readonly List<(long d, int v)>[] tempEdges_;
		private readonly (long d, int v)[][] edges_;

		private int k_;
		private int[,] parents_;
		private int[] depth_;

		public (long d, int v)[][] Edges => edges_;

		public Graph(int n)
		{
			n_ = n;
			tempEdges_ = new List<(long d, int v)>[n];
			for (int i = 0; i < n; i++) {
				tempEdges_[i] = new List<(long d, int v)>();
			}

			edges_ = new (long d, int v)[n][];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge(int u, int v, long d) => tempEdges_[u].Add((d, v));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge2(int u, int v, long d)
		{
			tempEdges_[u].Add((d, v));
			tempEdges_[v].Add((d, u));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Build()
		{
			for (int i = 0; i < edges_.Length; i++) {
				edges_[i] = tempEdges_[i].ToArray();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void InitializeLca(int root)
		{
			int m = n_ - 1;
			k_ = 0;
			while (m > 0) {
				++k_;
				m >>= 1;
			}

			parents_ = new int[k_, n_];
			depth_ = new int[n_];

			void Dfs(int v, int p, int d)
			{
				parents_[0, v] = p;
				depth_[v] = d;
				foreach (var next in edges_[v]) {
					if (next.v != p) {
						Dfs(next.v, v, d + 1);
					}
				}
			}

			Dfs(root, -1, 0);

			for (int i = 0; i < k_ - 1; i++) {
				for (int j = 0; j < n_; j++) {
					if (parents_[i, j] < 0) {
						parents_[i + 1, j] = -1;
					} else {
						parents_[i + 1, j] = parents_[i, parents_[i, j]];
					}
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Lca(int u, int v)
		{
			if (depth_[u] > depth_[v]) {
				(u, v) = (v, u);
			}

			for (int i = 0; i < k_; i++) {
				if (((depth_[v] - depth_[u]) & (1 << i)) != 0) {
					v = parents_[i, v];
				}
			}

			if (u == v) {
				return u;
			}

			for (int i = k_ - 1; i >= 0; i--) {
				if (parents_[i, u] == parents_[i, v]) {
					continue;
				}

				u = parents_[i, u];
				v = parents_[i, v];
			}

			return parents_[0, u];
		}
	}

	public class Graph<T>
	{
		private readonly int n_;
		private readonly List<(int v, T item)>[] tempEdges_;
		private readonly (int v, T item)[][] edges_;

		private int k_;
		private int[,] parents_;
		private int[] depth_;

		public (int v, T item)[][] Edges => edges_;

		public Graph(int n)
		{
			n_ = n;
			tempEdges_ = new List<(int v, T item)>[n];
			for (int i = 0; i < n; i++) {
				tempEdges_[i] = new List<(int v, T item)>();
			}

			edges_ = new (int v, T item)[n_][];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge(int u, int v, T item) => tempEdges_[u].Add((v, item));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge2(int u, int v, T item)
		{
			tempEdges_[u].Add((v, item));
			tempEdges_[v].Add((u, item));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Build()
		{
			for (int i = 0; i < edges_.Length; i++) {
				edges_[i] = tempEdges_[i].ToArray();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void InitializeLca(int root)
		{
			int m = n_ - 1;
			k_ = 0;
			while (m > 0) {
				++k_;
				m >>= 1;
			}

			parents_ = new int[k_, n_];
			depth_ = new int[n_];

			void Dfs(int v, int p, int d)
			{
				parents_[0, v] = p;
				depth_[v] = d;
				foreach (var next in edges_[v]) {
					if (next.v != p) {
						Dfs(next.v, v, d + 1);
					}
				}
			}

			Dfs(root, -1, 0);

			for (int i = 0; i < k_ - 1; i++) {
				for (int j = 0; j < n_; j++) {
					if (parents_[i, j] < 0) {
						parents_[i + 1, j] = -1;
					} else {
						parents_[i + 1, j] = parents_[i, parents_[i, j]];
					}
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Lca(int u, int v)
		{
			if (depth_[u] > depth_[v]) {
				(u, v) = (v, u);
			}

			for (int i = 0; i < k_; i++) {
				if (((depth_[v] - depth_[u]) & (1 << i)) != 0) {
					v = parents_[i, v];
				}
			}

			if (u == v) {
				return u;
			}

			for (int i = k_ - 1; i >= 0; i--) {
				if (parents_[i, u] == parents_[i, v]) {
					continue;
				}

				u = parents_[i, u];
				v = parents_[i, v];
			}

			return parents_[0, u];
		}
	}
}

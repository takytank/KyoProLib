using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class LevelAncestor
	{
		private readonly List<int>[] g_;
		private readonly int[,] parents_;
		private readonly List<List<int>> lad_;

		private readonly int[] depth_;
		private readonly int[] next_;
		private readonly int[] length_;
		private readonly int[] path_;
		private readonly int[] order_;
		private readonly int[] hs_;

		public LevelAncestor(int n)
		{
			g_ = new List<int>[n];
			for (int i = 0; i < n; i++) {
				g_[i] = new List<int>();
			}

			lad_ = new List<List<int>>();

			depth_ = new int[n];
			next_ = new int[n];
			next_.AsSpan().Fill(-1);
			length_ = new int[n];
			path_ = new int[n];
			order_ = new int[n];
			hs_ = new int[n + 1];

			int h = 1;
			while ((1 << h) <= n) {
				h++;
			}

			parents_ = new int[h, n];
			MemoryMarshal.CreateSpan<int>(ref parents_[0, 0], parents_.Length).Fill(-1);
			for (int i = 2; i <= n; i++) {
				hs_[i] = hs_[i >> 1] + 1;
			}
		}

		public void AddEdge(int u, int v)
		{
			g_[u].Add(v);
			g_[v].Add(u);
		}

		public void Dfs(int v, int p, int d, bool isFirst)
		{
			if (next_[v] < 0) {
				next_[v] = v;
				parents_[0, v] = p;
				length_[v] = d;
				depth_[v] = d;
				foreach (int u in g_[v]) {
					if (u == p) {
						continue;
					}

					Dfs(u, v, d + 1, false);
					if (length_[v] < length_[u]) {
						next_[v] = u;
						length_[v] = length_[u];
					}
				}
			}

			if (isFirst == false) {
				return;
			}

			path_[v] = lad_.Count;
			lad_.Add(new List<int>());
			for (int k = v; ; k = next_[k]) {
				lad_[^1].Add(k);
				path_[k] = path_[v];
				if (k == next_[k]) {
					break;
				}
			}

			for (; ; p = v, v = next_[v]) {
				foreach (var u in g_[v]) {
					if (u != p && u != next_[v]) {
						Dfs(u, v, d + 1, true);
					}
				}

				if (v == next_[v]) {
					break;
				}
			}
		}

		public void Build(int root = 0)
		{
			int n = g_.Length;
			Dfs(root, -1, 0, true);
			for (int k = 0; k + 1 < parents_.GetLength(0); k++) {
				for (int v = 0; v < n; v++) {
					if (parents_[k, v] < 0) {
						parents_[k + 1, v] = -1;
					} else {
						parents_[k + 1, v] = parents_[k, parents_[k, v]];
					}
				}
			}

			for (int i = 0; i < lad_.Count; i++) {
				int v = lad_[i][0], p = parents_[0, v];
				if (~p != 0) {
					int k = path_[p];
					int l = Math.Min(order_[p] + 1, lad_[i].Count);
					lad_[i].AddRange(Enumerable.Repeat(0, l));
					for (int j = 0, m = lad_[i].Count; j + l < m; j++) {
						lad_[i][m - (j + 1)] = lad_[i][m - (j + l + 1)];
					}

					for (int j = 0; j < l; j++) {
						lad_[i][j] = lad_[k][order_[p] - l + j + 1];
					}
				}

				for (int j = 0; j < lad_[i].Count; j++) {
					if (path_[lad_[i][j]] == i) {
						order_[lad_[i][j]] = j;
					}
				}
			}
		}

		public int Lca(int u, int v)
		{
			int h = parents_.GetLength(0);

			if (depth_[u] > depth_[v]) {
				(u, v) = (v, u);
			}

			for (int k = 0; k < h; k++) {
				if (((depth_[v] - depth_[u]) >> k & 1) != 0) {
					v = parents_[k, v];
				}
			}

			if (u == v) {
				return u;
			}

			for (int k = h - 1; k >= 0; k--) {
				if (parents_[k, u] == parents_[k, v]) {
					continue;
				}

				u = parents_[k, u];
				v = parents_[k, v];
			}

			return parents_[0, u];
		}

		public int Distance(int u, int v)
		{
			return depth_[u] + depth_[v] - depth_[Lca(u, v)] * 2;
		}

		public int Up(int v, int d)
		{
			if (d == 0) {
				return v;
			}

			v = parents_[hs_[d], v];
			d -= 1 << hs_[d];
			return lad_[path_[v]][order_[v] - d];
		}

		// from u to v
		public int Next(int u, int v)
		{
			if (depth_[u] >= depth_[v]) {
				return parents_[0, u];
			}

			int l = Up(v, depth_[v] - depth_[u] - 1);
			return parents_[0, l] == u ? l : parents_[0, u];
		}
	}
}

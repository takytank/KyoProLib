using System;
using System.Collections.Generic;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class HeavyLightDecomposition
	{
		private readonly List<int>[] g_;
		private readonly int[] size_;
		private readonly int[] in_;
		private readonly int[] out_;
		private readonly int[] head_;
		private readonly int[] reverseindex_;
		private readonly int[] parent_;

		public HeavyLightDecomposition(int n)
		{
			g_ = new List<int>[n];
			for (int i = 0; i < n; i++) {
				g_[i] = new List<int>();
			}

			size_ = new int[n];
			in_ = new int[n];
			out_ = new int[n];
			head_ = new int[n];
			reverseindex_ = new int[n];
			parent_ = new int[n];
		}

		public void AddEdge(int p, int q)
		{
			g_[p].Add(q);
			g_[q].Add(p);
		}

		public void Build(int root = 0)
		{
			DfsSZ(root, -1);
			int t = 0;
			DfsHld(root, -1, ref t);
		}

		private void DfsSZ(int index, int p)
		{
			parent_[index] = p;
			size_[index] = 1;
			if (g_[index].Count > 0 && g_[index][0] == p) {
				(g_[index][0], g_[index][^1]) = (g_[index][^1], g_[index][0]);
			}

			for (int i = 0; i < g_[index].Count; i++) {
				int to = g_[index][i];
				if (to == p) {
					continue;
				}

				DfsSZ(to, index);
				size_[index] += size_[to];
				if (size_[g_[index][0]] < size_[to]) {
					(g_[index][0], g_[index][i]) = (to, g_[index][0]);
				}
			}
		}

		private void DfsHld(int index, int p, ref int count)
		{
			in_[index] = count;
			count++;
			reverseindex_[in_[index]] = index;
			foreach (var to in g_[index]) {
				if (to == p) {
					continue;
				}

				head_[to] = g_[index][0] == to ? head_[index] : to;
				DfsHld(to, index, ref count);
			}

			out_[index] = count;
		}

		public int LevelAncestor(int v, int k)
		{
			while (true) {
				int u = head_[v];
				if (in_[v] - k >= in_[u]) {
					return reverseindex_[in_[v] - k];
				}

				k -= in_[v] - in_[u] + 1;
				v = parent_[u];
			}
		}

		public int LCA(int u, int v)
		{
			while (true) {
				if (in_[u] > in_[v]) {
					(u, v) = (v, u);
				}

				if (head_[u] == head_[v]) {
					return u;
				}

				v = parent_[head_[v]];
			}
		}

		public int Vertex(int v) => in_[v];
		public int OneEdge(int u, int v)
		{
			if (in_[u] > in_[v]) {
				(u, v) = (v, u);
			}

			if (head_[u] == head_[v]) {
				return in_[u] + 1;
			} else {
				return in_[v];
			}
		}

		public void ForEachVertex(int u, int v, Action<int, int> action)
		{
			while (true) {
				if (in_[u] > in_[v]) {
					(u, v) = (v, u);
				}

				action(Math.Max(in_[head_[v]], in_[u]), in_[v] + 1);
				if (head_[u] != head_[v]) {
					v = parent_[head_[v]];
				} else {
					break;
				}
			}
		}

		public void ForEachEdge(int u, int v, Action<int, int> action)
		{
			while (true) {
				if (in_[u] > in_[v]) {
					(u, v) = (v, u);
				}

				if (head_[u] != head_[v]) {
					action(in_[head_[v]], in_[v] + 1);
					v = parent_[head_[v]];
				} else {
					if (u != v) {
						action(in_[u] + 1, in_[v] + 1);
					}

					break;
				}
			}
		}

		public T Query<T>(
			int u, int v, T unit, Func<int, int, T> query, Func<T, T, T> merge, bool edge = false)
		{
			T l = unit;
			T r = unit;
			for (; ; v = parent_[head_[v]]) {
				if (in_[u] > in_[v]) {
					(u, v) = (v, u);
					(l, r) = (r, l);
				}

				if (head_[u] == head_[v]) {
					break;
				}

				l = merge(query(in_[head_[v]], in_[v] + 1), l);
			}

			return merge(
				merge(query(in_[u] + (edge ? 1 : 0), in_[v] + 1), l),
				r);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class DynamicConnectivity
	{
		private readonly UndoUnionFindTree uf_;
		private readonly int queryCount_;
		private readonly int size_;
		private readonly List<(int p, int q)>[] edges_;
		private readonly List<((int left, int right) range, (int p, int q) edge)> pendings_
			= new List<((int left, int right) range, (int p, int q) edge)>();
		private readonly Dictionary<(int p, int q), int> counts_
			= new Dictionary<(int p, int q), int>();
		private readonly Dictionary<(int p, int q), int> appears_
			= new Dictionary<(int p, int q), int>();

		public UndoUnionFindTree UnionFind => uf_;

		public DynamicConnectivity(int virtexCount, int queryCount)
		{
			queryCount_ = queryCount;
			uf_ = new UndoUnionFindTree(virtexCount);
			size_ = 1;
			while (size_ < queryCount) {
				size_ <<= 1;
			}

			int m = 2 * size_;
			edges_ = new List<(int p, int q)>[m];
			for (int i = 0; i < m; i++) {
				edges_[i] = new List<(int p, int q)>();
			}
		}

		public void Insert(int index, (int p, int q) edge)
		{
			if (edge.p > edge.q) {
				edge = (edge.q, edge.p);
			}

			if (counts_.ContainsKey(edge) == false) {
				counts_[edge] = 0;
			}

			if (counts_[edge] == 0) {
				appears_[edge] = index;
			}

			counts_[edge]++;
		}

		public void Erase(int index, (int p, int q) edge)
		{
			if (edge.p > edge.q) {
				edge = (edge.q, edge.p);
			}

			--counts_[edge];
			if (counts_[edge] == 0) {
				pendings_.Add(((appears_[edge], index), edge));
			}
		}

		public void Build()
		{
			foreach (var p in counts_) {
				if (p.Value > 0) {
					pendings_.Add(((appears_[p.Key], size_), p.Key));
				}
			}

			foreach (var (range, edge) in pendings_) {
				Add(range.left, range.right, edge);
			}
		}

		public void Add(int left, int right, (int p, int q) edge)
		{
			Add(left, right, edge, 1, 0, size_);
		}

		private void Add(int left, int right, (int p, int q) edge, int v, int l, int r)
		{
			if (r <= left || right <= l) {
				return;
			}

			if (left <= l && r <= right) {
				edges_[v].Add(edge);
				return;
			}

			Add(left, right, edge, v << 1, l, (l + r) >> 1);
			Add(left, right, edge, (v << 1) + 1, (l + r) >> 1, r);
		}

		public void Execute(Action<int> action, int v = 1)
		{
			foreach (var (p, q) in edges_[v]) {
				uf_.Unite(p, q);
			}

			if (v < size_) {
				Execute(action, v << 1);
				Execute(action, (v << 1) + 1);
			} else if (v - size_ < queryCount_) {
				int query_index = v - size_;
				action(query_index);
			}

			for (int i = 0; i < edges_[v].Count; i++) {
				uf_.Undo();
			}
		}
	}
}

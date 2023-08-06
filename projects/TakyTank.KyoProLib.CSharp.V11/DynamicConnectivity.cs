using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V11
{
	public class DynamicConnectivity
	{
		private readonly UndoUnionFindTree uf_;
		private readonly int queryCount_;
		private readonly int n_;
		private readonly LightList<(int p, int q)>[] edges_;
		private readonly LightList<((int left, int right) range, (int p, int q) edge)> pendings_
			= new LightList<((int left, int right) range, (int p, int q) edge)>();
		private readonly Dictionary<(int p, int q), int> counts_
			= new Dictionary<(int p, int q), int>();
		private readonly Dictionary<(int p, int q), int> appears_
			= new Dictionary<(int p, int q), int>();

		public UndoUnionFindTree UnionFind => uf_;

		public DynamicConnectivity(int virtexCount, int queryCount)
		{
			queryCount_ = queryCount;
			uf_ = new UndoUnionFindTree(virtexCount);
			n_ = 1;
			while (n_ < queryCount) {
				n_ <<= 1;
			}

			int m = 2 * n_;
			edges_ = new LightList<(int p, int q)>[m];
			for (int i = 0; i < m; i++) {
				edges_[i] = new LightList<(int p, int q)>();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void PlanConnect(int index, (int p, int q) edge)
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void PlanDisconnect(int index, (int p, int q) edge)
		{
			if (edge.p > edge.q) {
				edge = (edge.q, edge.p);
			}

			--counts_[edge];
			if (counts_[edge] == 0) {
				pendings_.Add(((appears_[edge], index), edge));
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void BuildPlans()
		{
			foreach (var p in counts_) {
				if (p.Value > 0) {
					pendings_.Add(((appears_[p.Key], n_), p.Key));
				}
			}

			foreach (var (range, edge) in pendings_.AsSpan()) {
				AddConnectionSpan(range.left, range.right, edge);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddConnectionSpan(Range range, (int p, int q) edge)
			=> AddConnectionSpan(range.Start.Value, range.End.Value, edge);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddConnectionSpan(int left, int right, (int p, int q) edge)
		{
			if (left > right || right < 0 || left >= queryCount_) {
				return;
			}

			int l = left + n_;
			int r = right + n_;
			while (l < r) {
				if ((l & 1) != 0) {
					edges_[l].Add(edge);
					++l;
				}

				if ((r & 1) != 0) {
					--r;
					edges_[r].Add(edge);
				}

				l >>= 1;
				r >>= 1;
			}
		}

		public void ExecuteQueries(Action<int> action, int v = 1)
		{
			foreach (var (p, q) in edges_[v].AsSpan()) {
				uf_.Unite(p, q);
			}

			if (v < n_) {
				ExecuteQueries(action, v << 1);
				ExecuteQueries(action, (v << 1) + 1);
			} else if (v - n_ < queryCount_) {
				action(v - n_);
			}

			for (int i = 0; i < edges_[v].Count; i++) {
				uf_.Undo();
			}
		}
	}
}

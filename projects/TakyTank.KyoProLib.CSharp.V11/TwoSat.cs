using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V11
{
	public class TwoSat
	{
		readonly int n_;
		readonly private bool[] answer_;
		readonly private StronglyConnectedComponent scc_;
		public TwoSat(int n, int m = 100000)
		{
			n_ = n;
			answer_ = new bool[n];
			scc_ = new StronglyConnectedComponent(2 * n, m);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddClause(int x, bool truthX, int y, bool truthY)
		{
			scc_.AddEdge(2 * x + (truthX ? 0 : 1), 2 * y + (truthY ? 1 : 0));
			scc_.AddEdge(2 * y + (truthY ? 0 : 1), 2 * x + (truthX ? 1 : 0));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Build() => scc_.Build();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (bool satisfied, IReadOnlyList<bool> answer) JudgeSatisfiable()
		{
			var sccs = scc_.Decompose();
			var id = new int[2 * n_];

			for (int i = 0; i < sccs.Length; ++i) {
				foreach (var v in sccs[i]) {
					id[v] = i;
				}
			}

			for (int i = 0; i < n_; ++i) {
				if (id[2 * i] == id[2 * i + 1]) {
					return (false, answer_);
				} else {
					answer_[i] = id[2 * i] < id[2 * i + 1];
				}
			}

			return (true, answer_);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.Core31
{
	public class IntervalSet
	{
		private readonly RedBlackTree<(long l, long r)> set_;
		private readonly bool mergesAdjacentInterval_;

		public IntervalSet(long inf, bool mergesAdjacentInterval)
		{
			mergesAdjacentInterval_ = mergesAdjacentInterval;
			set_ = new RedBlackTree<(long l, long r)>((x, y) => x.l.CompareTo(y.l)) {
				(-inf, -inf),
				(inf, inf),
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long l, long r) GetInterval(long p)
		{
			var (index, value) = set_.LowerBound((p, p + 1));
			if (index < 0 || value.r <= p) {
				return set_[^1];
			} else {
				return value;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IncludesSameInterval(long p, long q)
		{
			var (l, r) = GetInterval(p);
			return l <= q && q < r;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(long l, long r)
		{
			if (l > r) {
				(l, r) = (r, l);
			}

			var it = set_.LowerBound((l, r));
			var t = set_[it.index - 1];
			if (t.l <= l && l <= t.r) {
				l = Math.Min(l, t.l);
				r = Math.Max(r, t.r);
				set_.RemoveAt(it.index - 1);
			}

			it = set_.LowerBound((l, r));
			while (true) {
				if (mergesAdjacentInterval_) {
					if (l <= it.value.l && it.value.l <= r) {
						r = Math.Max(r, it.value.r);
						set_.RemoveAt(it.index);
						it = (it.index, set_[it.index]);
					} else {
						break;
					}
				} else {
					if (l < it.value.l && it.value.l < r) {
						r = Math.Max(r, it.value.r);
						set_.RemoveAt(it.index);
						it = (it.index, set_[it.index]);
					} else {
						break;
					}
				}
			}

			set_.Add((l, r));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Remove(long l, long r)
		{
			if (l > r) {
				(l, r) = (r, l);
			}

			var itL = set_.LowerBound((l, l + 1));
			var itR = set_.UpperBound((r, r + 1));
			itL = set_.Prev(itL);
			itR = set_.Prev(itR);
			for (int i = itR.index; i >= itL.index; i--) {
				set_.RemoveAt(i);
			}

			if (itL.value.l < l) {
				set_.Add((itL.value.l, Math.Min(itL.value.r, l)));
			}

			if (r < itR.value.r) {
				set_.Add((Math.Max(itR.value.l, r), itR.value.r));
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int index, long l, long r) LowerBound(long p)
		{
			var lower = set_.LowerBound((p, p));
			if (lower.value.l == p) {
				return (lower.index, lower.value.l, lower.value.r);
			}

			var (index, value) = set_.Prev(lower);
			if (value.r > p) {
				return (index, value.l, value.r);
			} else {
				return (lower.index, lower.value.l, lower.value.r);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int index, long l, long r) UpperBound(long p)
		{
			var (index, value) = set_.UpperBound((p, p));
			return (index, value.l, value.r);
		}
	}
}

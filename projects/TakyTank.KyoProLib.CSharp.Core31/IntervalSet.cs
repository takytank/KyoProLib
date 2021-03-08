using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.Core31
{
	public class IntervalSet
	{
		private readonly RedBlackTree<LR> set_;
		private readonly bool mergesAdjacentInterval_;

		public IntervalSet(long inf = long.MaxValue, bool mergesAdjacentInterval = true)
		{
			mergesAdjacentInterval_ = mergesAdjacentInterval;
			set_ = new RedBlackTree<LR>() {
				new LR(-inf, -inf),
				new LR(inf, inf),
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long l, long r) GetInterval(long p)
		{
			var (index, value) = set_.LowerBound(new LR(p, p));
			if (index < 0 || value.R <= p) {
				return set_[^1].ToTuple();
			} else {
				return value.ToTuple();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IncludesSameInterval(long p, long q)
		{
			var (l, r) = GetInterval(p);
			return l <= q && q < r;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(long p) => Add(p, p + 1);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(long l, long r)
		{
			if (l > r) {
				(l, r) = (r, l);
			}

			var it = set_.LowerBound(new LR(l, r));
			var t = set_[it.index - 1];
			if (t.L <= l && l <= t.R) {
				l = Math.Min(l, t.L);
				r = Math.Max(r, t.R);
				set_.RemoveAt(it.index - 1);
			}

			it = set_.LowerBound(new LR(l, r));
			while (true) {
				if (mergesAdjacentInterval_) {
					if (l <= it.value.L && it.value.L <= r) {
						r = Math.Max(r, it.value.R);
						set_.RemoveAt(it.index);
						it = (it.index, set_[it.index]);
					} else {
						break;
					}
				} else {
					if (l < it.value.L && it.value.L < r) {
						r = Math.Max(r, it.value.R);
						set_.RemoveAt(it.index);
						it = (it.index, set_[it.index]);
					} else {
						break;
					}
				}
			}

			set_.Add(new LR(l, r));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Remove(long p) => Remove(p, p + 1);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Remove(long l, long r)
		{
			if (l > r) {
				(l, r) = (r, l);
			}

			var itL = set_.LowerBound(new LR(l, l));
			var itR = set_.UpperBound(new LR(r, r));
			itL = set_.Prev(itL);
			itR = set_.Prev(itR);
			for (int i = itR.index; i >= itL.index; i--) {
				set_.RemoveAt(i);
			}

			if (itL.value.L < l) {
				set_.Add(new LR(itL.value.L, Math.Min(itL.value.R, l)));
			}

			if (r < itR.value.R) {
				set_.Add(new LR(Math.Max(itR.value.L, r), itR.value.R));
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long Mex(long p = 0)
		{
			var (index, value) = set_.LowerBound(new LR(p, p));
			if (index < 0 || value.L > p) {
				return p;
			} else {
				return value.R;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int index, long l, long r) LowerBound(long p)
		{
			var lower = set_.LowerBound(new LR(p, p));
			if (lower.value.L == p) {
				return (lower.index, lower.value.L, lower.value.R);
			}

			var (index, value) = set_.Prev(lower);
			if (value.R > p) {
				return (index, value.L, value.R);
			} else {
				return (lower.index, lower.value.L, lower.value.R);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int index, long l, long r) UpperBound(long p)
		{
			var (index, value) = set_.UpperBound(new LR(p, p));
			return (index, value.L, value.R);
		}

		private struct LR : IComparable<LR>
		{
			public long L { get; set; }
			public long R { get; set; }
			public LR(long l, long r)
			{
				L = l;
				R = r;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public (long l, long r) ToTuple() => (L, R);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public int CompareTo(LR other) => L.CompareTo(other.L);
		}
	}

	public class IntervalSet<T> where T : struct, IComparable<T>
	{
		private readonly RedBlackTree<LR> set_;
		private readonly bool mergesAdjacentInterval_;

		public IntervalSet(T inf, T minf, bool mergesAdjacentInterval)
		{
			mergesAdjacentInterval_ = mergesAdjacentInterval;
			set_ = new RedBlackTree<LR>() {
				new LR(minf, minf),
				new LR(inf, inf),
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (T l, T r) GetInterval(T p)
		{
			var (index, value) = set_.LowerBound(new LR(p, p));
			if (index < 0 || value.R.CompareTo(p) <= 0) {
				return set_[^1].ToTuple();
			} else {
				return value.ToTuple();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IncludesSameInterval(T p, T q)
		{
			var (l, r) = GetInterval(p);
			return l.CompareTo(q) <= 0 && q.CompareTo(r) < 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(T l, T r)
		{
			if (l.CompareTo(r) > 0) {
				(l, r) = (r, l);
			}

			var it = set_.LowerBound(new LR(l, r));
			var t = set_[it.index - 1];
			if (t.L.CompareTo(l) <= 0 && l.CompareTo(t.R) <= 0) {
				if (l.CompareTo(t.L) > 0) {
					l = t.L;
				}

				if (r.CompareTo(t.R) < 0) {
					r = t.R;
				}

				set_.RemoveAt(it.index - 1);
			}

			it = set_.LowerBound(new LR(l, r));
			while (true) {
				if (mergesAdjacentInterval_) {
					if (l.CompareTo(it.value.L) <= 0 && it.value.L.CompareTo(r) <= 0) {
						if (r.CompareTo(it.value.R) < 0) {
							r = it.value.R;
						}

						set_.RemoveAt(it.index);
						it = (it.index, set_[it.index]);
					} else {
						break;
					}
				} else {
					if (l.CompareTo(it.value.L) < 0 && it.value.L.CompareTo(r) < 0) {
						if (r.CompareTo(it.value.R) < 0) {
							r = it.value.R;
						}

						set_.RemoveAt(it.index);
						it = (it.index, set_[it.index]);
					} else {
						break;
					}
				}
			}

			set_.Add(new LR(l, r));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Remove(T l, T r)
		{
			if (l.CompareTo(r) > 0) {
				(l, r) = (r, l);
			}

			var itL = set_.LowerBound(new LR(l, l));
			var itR = set_.UpperBound(new LR(r, r));
			itL = set_.Prev(itL);
			itR = set_.Prev(itR);
			for (int i = itR.index; i >= itL.index; i--) {
				set_.RemoveAt(i);
			}

			if (itL.value.L.CompareTo(l) < 0) {
				T min = itL.value.R.CompareTo(l) <= 0 ? itL.value.R : l;
				set_.Add(new LR(itL.value.L, min));
			}

			if (r.CompareTo(itR.value.R) < 0) {
				T max = itR.value.L.CompareTo(r) >= 0 ? itR.value.L : r;
				set_.Add(new LR(max, itR.value.R));
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Mex(T p)
		{
			var (index, value) = set_.LowerBound(new LR(p, p));
			if (index < 0 || value.L.CompareTo(p) > 0) {
				return p;
			} else {
				return value.R;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int index, T l, T r) LowerBound(T p)
		{
			var lower = set_.LowerBound(new LR(p, p));
			if (lower.value.L.CompareTo(p) == 0) {
				return (lower.index, lower.value.L, lower.value.R);
			}

			var (index, value) = set_.Prev(lower);
			if (value.R.CompareTo(p) > 0) {
				return (index, value.L, value.R);
			} else {
				return (lower.index, lower.value.L, lower.value.R);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int index, T l, T r) UpperBound(T p)
		{
			var (index, value) = set_.UpperBound(new LR(p, p));
			return (index, value.L, value.R);
		}

		private struct LR : IComparable<LR>
		{
			public T L { get; set; }
			public T R { get; set; }
			public LR(T l, T r)
			{
				L = l;
				R = r;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public (T l, T r) ToTuple() => (L, R);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public int CompareTo(LR other) => L.CompareTo(other.L);
		}
	}
}

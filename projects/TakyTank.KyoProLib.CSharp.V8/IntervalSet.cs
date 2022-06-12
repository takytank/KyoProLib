using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V8
{
	public class IntervalSet32
	{
		private readonly RedBlackTree<LR> set_;
		private readonly bool mergesAdjacentInterval_;

		public IntervalSet32(int inf = int.MaxValue, bool mergesAdjacentInterval = true)
		{
			mergesAdjacentInterval_ = mergesAdjacentInterval;
			set_ = new RedBlackTree<LR>(false) {
				new LR(-inf, -inf),
				new LR(inf, inf),
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int l, int r) GetInterval(int p)
		{
			var lower = set_.LowerBound(new LR(p, p));
			if (lower.value.L != p) {
				lower = set_.Prev(lower);
			}

			var (index, value) = lower;
			if (index < 0 || value.R <= p) {
				return set_[^1].ToTuple();
			} else {
				return value.ToTuple();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IncludesSameInterval(int p, int q)
		{
			var (l, r) = GetInterval(p);
			return l <= q && q < r;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(int p)
		{
			var right = set_.UpperBound(new LR(p, p));
			var (_, left) = set_.Prev(right);
			if (left.L <= p && p < left.R) {
				return;
			}

			bool willMerge = mergesAdjacentInterval_
				? right.value.L == p + 1
				: right.value.L == p;
			if (left.R == p) {
				if (willMerge == false) {
					set_.Remove(left);
					set_.Add(new LR(left.L, p + 1));
				} else {
					set_.Remove(left);
					set_.Remove(right.value);
					set_.Add(new LR(left.L, right.value.R));
				}
			} else {
				if (willMerge == false) {
					set_.Add(new LR(p, p + 1));
				} else {
					set_.Remove(right.value);
					set_.Add(new LR(p, right.value.R));
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(int l, int r)
		{
			if (l > r) {
				(l, r) = (r, l);
			}

			var it = set_.LowerBound(new LR(l, r));
			var t = set_[it.index - 1];
			bool willMerge = mergesAdjacentInterval_
				? t.L <= l && l <= t.R
				: t.L < l && l < t.R;
			if (willMerge) {
				l = Math.Min(l, t.L);
				r = Math.Max(r, t.R);
				set_.RemoveAt(it.index - 1);
			}

			it = set_.LowerBound(new LR(l, r));
			while (true) {
				willMerge = mergesAdjacentInterval_
					 ? l <= it.value.L && it.value.L <= r
					 : l < it.value.L && it.value.L < r;
				if (willMerge) {
					r = Math.Max(r, it.value.R);
					set_.RemoveAt(it.index);
					it = (it.index, set_[it.index]);
				} else {
					break;
				}
			}

			set_.Add(new LR(l, r));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Remove(int p)
		{
			var (_, value) = set_.Prev(set_.UpperBound(new LR(p, p)));
			if (value.R <= p) {
				return false;
			}

			set_.Remove(value);
			if (value.L < p) {
				set_.Add(new LR(value.L, p));
			}

			if (p + 1 < value.R) {
				set_.Add(new LR(p + 1, value.R));
			}

			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IReadOnlyList<(int l, int r)> Remove(int l, int r)
		{
			if (l > r) {
				(l, r) = (r, l);
			}

			var itL = set_.LowerBound(new LR(l, l));
			var itR = set_.UpperBound(new LR(r, r));
			itL = set_.Prev(itL);
			itR = set_.Prev(itR);
			var removes = new List<(int l, int r)>();
			for (int i = itR.index; i >= itL.index; i--) {
				bool isEdge = i == 0 || i == set_.Count - 1;
				var (found, removed) = set_.RemoveAt(i);
				if (isEdge == false && found) {
					removes.Add((Math.Max(l, removed.L), Math.Min(r, removed.R)));
				}
			}

			if (itL.value.L < l) {
				set_.Add(new LR(itL.value.L, Math.Min(itL.value.R, l)));
			}

			if (r < itR.value.R) {
				set_.Add(new LR(Math.Max(itR.value.L, r), itR.value.R));
			}

			return removes;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Mex(int p = 0)
		{
			var lower = set_.LowerBound(new LR(p, p));
			if (lower.value.L != p) {
				lower = set_.Prev(lower);
			}

			var (index, value) = lower;
			if (index < 0 || value.R <= p) {
				return p;
			} else {
				return value.R;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int index, int l, int r) LowerBound(int p)
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
		public (int index, int l, int r) UpperBound(int p)
		{
			var (index, value) = set_.UpperBound(new LR(p, p));
			return (index, value.L, value.R);
		}

		private struct LR : IComparable<LR>
		{
			public int L { get; set; }
			public int R { get; set; }
			public LR(int l, int r)
			{
				L = l;
				R = r;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public (int l, int r) ToTuple() => (L, R);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public int CompareTo(LR other) => L.CompareTo(other.L);
		}
	}

	public class IntervalSet32<T>
	{
		private readonly RedBlackTree<LR> set_;
		private readonly bool mergesAdjacentInterval_;

		public IntervalSet32(int inf = int.MaxValue, bool mergesAdjacentInterval = true)
		{
			mergesAdjacentInterval_ = mergesAdjacentInterval;
			set_ = new RedBlackTree<LR>(false) {
				new LR(-inf, -inf, default),
				new LR(inf, inf, default),
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int l, int r) GetInterval(int p)
		{
			var lower = set_.LowerBound(new LR(p, p, default));
			if (lower.value.L != p) {
				lower = set_.Prev(lower);
			}

			var (index, value) = lower;
			if (index < 0 || value.R <= p) {
				return set_[^1].ToTuple();
			} else {
				return value.ToTuple();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IncludesSameInterval(int p, int q)
		{
			var (l, r) = GetInterval(p);
			return l <= q && q < r;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(int l, int r, T value)
		{
			if (l > r) {
				(l, r) = (r, l);
			}

			Remove(l, r);
			set_.Add(new LR(l, r, value));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IReadOnlyList<(int l, int r, T value)> Remove(int l, int r)
		{
			if (l > r) {
				(l, r) = (r, l);
			}

			var itL = set_.LowerBound(new LR(l, l, default));
			var itR = set_.UpperBound(new LR(r, r, default));
			itL = set_.Prev(itL);
			itR = set_.Prev(itR);
			var removes = new List<(int l, int r, T value)>();
			for (int i = itR.index; i >= itL.index; i--) {
				bool isEdge = i == 0 || i == set_.Count - 1;
				var (found, removed) = set_.RemoveAt(i);
				if (isEdge == false && found) {
					removes.Add((Math.Max(l, removed.L), Math.Min(r, removed.R), removed.Value));
				}
			}

			if (itL.value.L < l) {
				set_.Add(new LR(itL.value.L, Math.Min(itL.value.R, l), itL.value.Value));
			}

			if (r < itR.value.R) {
				set_.Add(new LR(Math.Max(itR.value.L, r), itR.value.R, itR.value.Value));
			}

			return removes;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Mex(int p = 0)
		{
			var lower = set_.LowerBound(new LR(p, p, default));
			if (lower.value.L != p) {
				lower = set_.Prev(lower);
			}

			var (index, value) = lower;
			if (index < 0 || value.R <= p) {
				return p;
			} else {
				return value.R;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int index, int l, int r) LowerBound(int p)
		{
			var lower = set_.LowerBound(new LR(p, p, default));
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
		public (int index, int l, int r) UpperBound(int p)
		{
			var (index, value) = set_.UpperBound(new LR(p, p, default));
			return (index, value.L, value.R);
		}

		private struct LR : IComparable<LR>
		{
			public int L { get; set; }
			public int R { get; set; }
			public T Value { get; set; }
			public LR(int l, int r, T value)
			{
				L = l;
				R = r;
				Value = value;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public (int l, int r) ToTuple() => (L, R);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public int CompareTo(LR other) => L.CompareTo(other.L);
		}
	}

	public class IntervalSet64
	{
		private readonly RedBlackTree<LR> set_;
		private readonly bool mergesAdjacentInterval_;

		public IntervalSet64(long inf = long.MaxValue, bool mergesAdjacentInterval = true)
		{
			mergesAdjacentInterval_ = mergesAdjacentInterval;
			set_ = new RedBlackTree<LR>(false) {
				new LR(-inf, -inf),
				new LR(inf, inf),
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long l, long r) GetInterval(long p)
		{
			var lower = set_.LowerBound(new LR(p, p));
			if (lower.value.L != p) {
				lower = set_.Prev(lower);
			}

			var (index, value) = lower;
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
		public void Add(long p)
		{
			var right = set_.UpperBound(new LR(p, p));
			var (_, left) = set_.Prev(right);
			if (left.L <= p && p < left.R) {
				return;
			}

			bool willMerge = mergesAdjacentInterval_
				? right.value.L == p + 1
				: right.value.L == p;
			if (left.R == p) {
				if (willMerge == false) {
					set_.Remove(left);
					set_.Add(new LR(left.L, p + 1));
				} else {
					set_.Remove(left);
					set_.Remove(right.value);
					set_.Add(new LR(left.L, right.value.R));
				}
			} else {
				if (willMerge == false) {
					set_.Add(new LR(p, p + 1));
				} else {
					set_.Remove(right.value);
					set_.Add(new LR(p, right.value.R));
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(long l, long r)
		{
			if (l > r) {
				(l, r) = (r, l);
			}

			var it = set_.LowerBound(new LR(l, r));
			var t = set_[it.index - 1];
			bool willMerge = mergesAdjacentInterval_
				? t.L <= l && l <= t.R
				: t.L < l && l < t.R;
			if (willMerge) {
				l = Math.Min(l, t.L);
				r = Math.Max(r, t.R);
				set_.RemoveAt(it.index - 1);
			}

			it = set_.LowerBound(new LR(l, r));
			while (true) {
				willMerge = mergesAdjacentInterval_
					 ? l <= it.value.L && it.value.L <= r
					 : l < it.value.L && it.value.L < r;
				if (willMerge) {
					r = Math.Max(r, it.value.R);
					set_.RemoveAt(it.index);
					it = (it.index, set_[it.index]);
				} else {
					break;
				}
			}

			set_.Add(new LR(l, r));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Remove(long p)
		{
			var (_, value) = set_.Prev(set_.UpperBound(new LR(p, p)));
			if (value.R <= p) {
				return false;
			}

			set_.Remove(value);
			if (value.L < p) {
				set_.Add(new LR(value.L, p));
			}

			if (p + 1 < value.R) {
				set_.Add(new LR(p + 1, value.R));
			}

			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IReadOnlyList<(long l, long r)> Remove(long l, long r)
		{
			if (l > r) {
				(l, r) = (r, l);
			}

			var itL = set_.LowerBound(new LR(l, l));
			var itR = set_.UpperBound(new LR(r, r));
			itL = set_.Prev(itL);
			itR = set_.Prev(itR);
			var removes = new List<(long l, long r)>();
			for (int i = itR.index; i >= itL.index; i--) {
				bool isEdge = i == 0 || i == set_.Count - 1;
				var (found, removed) = set_.RemoveAt(i);
				if (isEdge == false && found) {
					removes.Add((Math.Max(l, removed.L), Math.Min(r, removed.R)));
				}
			}

			if (itL.value.L < l) {
				set_.Add(new LR(itL.value.L, Math.Min(itL.value.R, l)));
			}

			if (r < itR.value.R) {
				set_.Add(new LR(Math.Max(itR.value.L, r), itR.value.R));
			}

			return removes;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long Mex(long p = 0)
		{
			var lower = set_.LowerBound(new LR(p, p));
			if (lower.value.L != p) {
				lower = set_.Prev(lower);
			}

			var (index, value) = lower;
			if (index < 0 || value.R <= p) {
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

	public class IntervalSet64<T>
	{
		private readonly RedBlackTree<LR> set_;
		private readonly bool mergesAdjacentInterval_;

		public IntervalSet64(long inf = long.MaxValue, bool mergesAdjacentInterval = true)
		{
			mergesAdjacentInterval_ = mergesAdjacentInterval;
			set_ = new RedBlackTree<LR>(false) {
				new LR(-inf, -inf, default),
				new LR(inf, inf, default),
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long l, long r) GetInterval(long p)
		{
			var lower = set_.LowerBound(new LR(p, p, default));
			if (lower.value.L != p) {
				lower = set_.Prev(lower);
			}

			var (index, value) = lower;
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
		public void Add(long l, long r, T value)
		{
			if (l > r) {
				(l, r) = (r, l);
			}

			Remove(l, r);
			set_.Add(new LR(l, r, value));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IReadOnlyList<(long l, long r, T value)> Remove(long l, long r)
		{
			if (l > r) {
				(l, r) = (r, l);
			}

			var itL = set_.LowerBound(new LR(l, l, default));
			var itR = set_.UpperBound(new LR(r, r, default));
			itL = set_.Prev(itL);
			itR = set_.Prev(itR);
			var removes = new List<(long l, long r, T value)>();
			for (int i = itR.index; i >= itL.index; i--) {
				bool isEdge = i == 0 || i == set_.Count - 1;
				var (found, removed) = set_.RemoveAt(i);
				if (isEdge == false && found) {
					removes.Add((Math.Max(l, removed.L), Math.Min(r, removed.R), removed.Value));
				}
			}

			if (itL.value.L < l) {
				set_.Add(new LR(itL.value.L, Math.Min(itL.value.R, l), itL.value.Value));
			}

			if (r < itR.value.R) {
				set_.Add(new LR(Math.Max(itR.value.L, r), itR.value.R, itR.value.Value));
			}

			return removes;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long Mex(long p = 0)
		{
			var lower = set_.LowerBound(new LR(p, p, default));
			if (lower.value.L != p) {
				lower = set_.Prev(lower);
			}

			var (index, value) = lower;
			if (index < 0 || value.R <= p) {
				return p;
			} else {
				return value.R;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int index, long l, long r) LowerBound(long p)
		{
			var lower = set_.LowerBound(new LR(p, p, default));
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
			var (index, value) = set_.UpperBound(new LR(p, p, default));
			return (index, value.L, value.R);
		}

		private struct LR : IComparable<LR>
		{
			public long L { get; set; }
			public long R { get; set; }
			public T Value { get; set; }
			public LR(long l, long r, T value)
			{
				L = l;
				R = r;
				Value = value;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public (long l, long r) ToTuple() => (L, R);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public int CompareTo(LR other) => L.CompareTo(other.L);
		}
	}

	public class IntervalSetAny<TRange> where TRange : struct, IComparable<TRange>
	{
		private readonly RedBlackTree<LR> set_;
		private readonly bool mergesAdjacentInterval_;

		public IntervalSetAny(TRange inf, TRange minf, bool mergesAdjacentInterval)
		{
			mergesAdjacentInterval_ = mergesAdjacentInterval;
			set_ = new RedBlackTree<LR>(false) {
				new LR(minf, minf),
				new LR(inf, inf),
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (TRange l, TRange r) GetInterval(TRange p)
		{
			var lower = set_.LowerBound(new LR(p, p));
			if (lower.value.L.CompareTo(p) != 0) {
				lower = set_.Prev(lower);
			}

			var (index, value) = lower;
			if (index < 0 || value.R.CompareTo(p) <= 0) {
				return set_[^1].ToTuple();
			} else {
				return value.ToTuple();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IncludesSameInterval(TRange p, TRange q)
		{
			var (l, r) = GetInterval(p);
			return l.CompareTo(q) <= 0 && q.CompareTo(r) < 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(TRange l, TRange r)
		{
			if (l.CompareTo(r) > 0) {
				(l, r) = (r, l);
			}

			var it = set_.LowerBound(new LR(l, r));
			var t = set_[it.index - 1];
			bool willMerge = mergesAdjacentInterval_
				? t.L.CompareTo(l) <= 0 && l.CompareTo(t.R) <= 0
				: t.L.CompareTo(l) < 0 && l.CompareTo(t.R) < 0;
			if (willMerge) {
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
				willMerge = mergesAdjacentInterval_
					 ? l.CompareTo(it.value.L) <= 0 && it.value.L.CompareTo(r) <= 0
					 : l.CompareTo(it.value.L) < 0 && it.value.L.CompareTo(r) < 0;

				if (willMerge) {
					if (r.CompareTo(it.value.R) < 0) {
						r = it.value.R;
					}

					set_.RemoveAt(it.index);
					it = (it.index, set_[it.index]);
				} else {
					break;
				}
			}

			set_.Add(new LR(l, r));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IReadOnlyList<(TRange l, TRange r)> Remove(TRange l, TRange r)
		{
			if (l.CompareTo(r) > 0) {
				(l, r) = (r, l);
			}

			var itL = set_.LowerBound(new LR(l, l));
			var itR = set_.UpperBound(new LR(r, r));
			itL = set_.Prev(itL);
			itR = set_.Prev(itR);
			var removes = new List<(TRange l, TRange r)>();
			for (int i = itR.index; i >= itL.index; i--) {
				bool isEdge = i == 0 || i == set_.Count - 1;
				var (found, removed) = set_.RemoveAt(i);
				if (isEdge == false && found) {
					removes.Add((
						l.CompareTo(removed.L) >= 0 ? l : removed.L,
						r.CompareTo(removed.R) <= 0 ? r : removed.R));
				}

				set_.RemoveAt(i);
			}

			if (itL.value.L.CompareTo(l) < 0) {
				TRange min = itL.value.R.CompareTo(l) <= 0 ? itL.value.R : l;
				set_.Add(new LR(itL.value.L, min));
			}

			if (r.CompareTo(itR.value.R) < 0) {
				TRange max = itR.value.L.CompareTo(r) >= 0 ? itR.value.L : r;
				set_.Add(new LR(max, itR.value.R));
			}

			return removes;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TRange Mex(TRange p)
		{
			var lower = set_.LowerBound(new LR(p, p));
			if (lower.value.L.CompareTo(p) != 0) {
				lower = set_.Prev(lower);
			}

			var (index, value) = lower;
			if (index < 0 || value.R.CompareTo(p) <= 0) {
				return p;
			} else {
				return value.R;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int index, TRange l, TRange r) LowerBound(TRange p)
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
		public (int index, TRange l, TRange r) UpperBound(TRange p)
		{
			var (index, value) = set_.UpperBound(new LR(p, p));
			return (index, value.L, value.R);
		}

		private struct LR : IComparable<LR>
		{
			public TRange L { get; set; }
			public TRange R { get; set; }
			public LR(TRange l, TRange r)
			{
				L = l;
				R = r;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public (TRange l, TRange r) ToTuple() => (L, R);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public int CompareTo(LR other) => L.CompareTo(other.L);
		}
	}

	public class IntervalSetAny<TRange, TValue> where TRange : struct, IComparable<TRange>
	{
		private readonly RedBlackTree<LR> set_;
		private readonly bool mergesAdjacentInterval_;

		public IntervalSetAny(TRange inf, TRange minf, bool mergesAdjacentInterval)
		{
			mergesAdjacentInterval_ = mergesAdjacentInterval;
			set_ = new RedBlackTree<LR>(false) {
				new LR(minf, minf, default),
				new LR(inf, inf, default),
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (TRange l, TRange r) GetInterval(TRange p)
		{
			var lower = set_.LowerBound(new LR(p, p, default));
			if (lower.value.L.CompareTo(p) != 0) {
				lower = set_.Prev(lower);
			}

			var (index, value) = lower;
			if (index < 0 || value.R.CompareTo(p) <= 0) {
				return set_[^1].ToTuple();
			} else {
				return value.ToTuple();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IncludesSameInterval(TRange p, TRange q)
		{
			var (l, r) = GetInterval(p);
			return l.CompareTo(q) <= 0 && q.CompareTo(r) < 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(TRange l, TRange r, TValue value)
		{
			if (l.CompareTo(r) > 0) {
				(l, r) = (r, l);
			}

			Remove(l, r);
			set_.Add(new LR(l, r, value));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IReadOnlyList<(TRange l, TRange r)> Remove(TRange l, TRange r)
		{
			if (l.CompareTo(r) > 0) {
				(l, r) = (r, l);
			}

			var itL = set_.LowerBound(new LR(l, l, default));
			var itR = set_.UpperBound(new LR(r, r, default));
			itL = set_.Prev(itL);
			itR = set_.Prev(itR);
			var removes = new List<(TRange l, TRange r)>();
			for (int i = itR.index; i >= itL.index; i--) {
				bool isEdge = i == 0 || i == set_.Count - 1;
				var (found, removed) = set_.RemoveAt(i);
				if (isEdge == false && found) {
					removes.Add((
						l.CompareTo(removed.L) >= 0 ? l : removed.L,
						r.CompareTo(removed.R) <= 0 ? r : removed.R));
				}

				set_.RemoveAt(i);
			}

			if (itL.value.L.CompareTo(l) < 0) {
				TRange min = itL.value.R.CompareTo(l) <= 0 ? itL.value.R : l;
				set_.Add(new LR(itL.value.L, min, itL.value.Value));
			}

			if (r.CompareTo(itR.value.R) < 0) {
				TRange max = itR.value.L.CompareTo(r) >= 0 ? itR.value.L : r;
				set_.Add(new LR(max, itR.value.R, itR.value.Value));
			}

			return removes;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TRange Mex(TRange p)
		{
			var lower = set_.LowerBound(new LR(p, p, default));
			if (lower.value.L.CompareTo(p) != 0) {
				lower = set_.Prev(lower);
			}

			var (index, value) = lower;
			if (index < 0 || value.R.CompareTo(p) <= 0) {
				return p;
			} else {
				return value.R;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int index, TRange l, TRange r) LowerBound(TRange p)
		{
			var lower = set_.LowerBound(new LR(p, p, default));
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
		public (int index, TRange l, TRange r) UpperBound(TRange p)
		{
			var (index, value) = set_.UpperBound(new LR(p, p, default));
			return (index, value.L, value.R);
		}

		private struct LR : IComparable<LR>
		{
			public TRange L { get; set; }
			public TRange R { get; set; }
			public TValue Value { get; set; }
			public LR(TRange l, TRange r, TValue value)
			{
				L = l;
				R = r;
				Value = value;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public (TRange l, TRange r) ToTuple() => (L, R);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public int CompareTo(LR other) => L.CompareTo(other.L);
		}
	}
}
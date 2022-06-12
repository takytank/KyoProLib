using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V8
{
	public class IntervalSet32
	{
		private readonly RedBlackTree<LR> _set;
		private readonly bool _mergesAdjacentInterval;

		public IntervalSet32(int inf = int.MaxValue, bool mergesAdjacentInterval = true)
		{
			_mergesAdjacentInterval = mergesAdjacentInterval;
			_set = new RedBlackTree<LR>(false) {
				new LR(-inf, -inf),
				new LR(inf, inf),
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int l, int r) GetInterval(int p)
		{
			var lower = _set.LowerBound(new LR(p, p));
			if (lower.value.L != p) {
				lower = _set.Prev(lower);
			}

			var (index, value) = lower;
			if (index < 0 || value.R <= p) {
				return _set[^1].ToTuple();
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
			var right = _set.UpperBound(new LR(p, p));
			var (_, left) = _set.Prev(right);
			if (left.L <= p && p < left.R) {
				return;
			}

			bool willMerge = _mergesAdjacentInterval
				? right.value.L == p + 1
				: right.value.L == p;
			if (left.R == p) {
				if (willMerge == false) {
					_set.Remove(left);
					_set.Add(new LR(left.L, p + 1));
				} else {
					_set.Remove(left);
					_set.Remove(right.value);
					_set.Add(new LR(left.L, right.value.R));
				}
			} else {
				if (willMerge == false) {
					_set.Add(new LR(p, p + 1));
				} else {
					_set.Remove(right.value);
					_set.Add(new LR(p, right.value.R));
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(int l, int r)
		{
			if (l > r) {
				(l, r) = (r, l);
			}

			var it = _set.LowerBound(new LR(l, r));
			var t = _set[it.index - 1];
			bool willMerge = _mergesAdjacentInterval
				? t.L <= l && l <= t.R
				: t.L < l && l < t.R;
			if (willMerge) {
				l = Math.Min(l, t.L);
				r = Math.Max(r, t.R);
				_set.RemoveAt(it.index - 1);
			}

			it = _set.LowerBound(new LR(l, r));
			while (true) {
				willMerge = _mergesAdjacentInterval
					 ? l <= it.value.L && it.value.L <= r
					 : l < it.value.L && it.value.L < r;
				if (willMerge) {
					r = Math.Max(r, it.value.R);
					_set.RemoveAt(it.index);
					it = (it.index, _set[it.index]);
				} else {
					break;
				}
			}

			_set.Add(new LR(l, r));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Remove(int p)
		{
			var (_, value) = _set.Prev(_set.UpperBound(new LR(p, p)));
			if (value.R <= p) {
				return false;
			}

			_set.Remove(value);
			if (value.L < p) {
				_set.Add(new LR(value.L, p));
			}

			if (p + 1 < value.R) {
				_set.Add(new LR(p + 1, value.R));
			}

			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IReadOnlyList<(int l, int r)> Remove(int l, int r)
		{
			if (l > r) {
				(l, r) = (r, l);
			}

			var itL = _set.LowerBound(new LR(l, l));
			var itR = _set.UpperBound(new LR(r, r));
			itL = _set.Prev(itL);
			itR = _set.Prev(itR);
			var removes = new List<(int l, int r)>();
			for (int i = itR.index; i >= itL.index; i--) {
				bool isEdge = i == 0 || i == _set.Count - 1;
				var (found, removed) = _set.RemoveAt(i);
				if (isEdge == false && found) {
					removes.Add((Math.Max(l, removed.L), Math.Min(r, removed.R)));
				}
			}

			if (itL.value.L < l) {
				_set.Add(new LR(itL.value.L, Math.Min(itL.value.R, l)));
			}

			if (r < itR.value.R) {
				_set.Add(new LR(Math.Max(itR.value.L, r), itR.value.R));
			}

			return removes;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Mex(int p = 0)
		{
			var lower = _set.LowerBound(new LR(p, p));
			if (lower.value.L != p) {
				lower = _set.Prev(lower);
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
			var lower = _set.LowerBound(new LR(p, p));
			if (lower.value.L == p) {
				return (lower.index, lower.value.L, lower.value.R);
			}

			var (index, value) = _set.Prev(lower);
			if (value.R > p) {
				return (index, value.L, value.R);
			} else {
				return (lower.index, lower.value.L, lower.value.R);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int index, int l, int r) UpperBound(int p)
		{
			var (index, value) = _set.UpperBound(new LR(p, p));
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
		private readonly RedBlackTree<LR> _set;

		public IntervalSet32(int inf = int.MaxValue)
		{
			_set = new RedBlackTree<LR>(false) {
				new LR(-inf, -inf, default),
				new LR(inf, inf, default),
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int l, int r) GetInterval(int p)
		{
			var lower = _set.LowerBound(new LR(p, p, default));
			if (lower.value.L != p) {
				lower = _set.Prev(lower);
			}

			var (index, value) = lower;
			if (index < 0 || value.R <= p) {
				return _set[^1].ToTuple();
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
			_set.Add(new LR(l, r, value));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IReadOnlyList<(int l, int r, T value)> Remove(int l, int r)
		{
			if (l > r) {
				(l, r) = (r, l);
			}

			var itL = _set.LowerBound(new LR(l, l, default));
			var itR = _set.UpperBound(new LR(r, r, default));
			itL = _set.Prev(itL);
			itR = _set.Prev(itR);
			var removes = new List<(int l, int r, T value)>();
			for (int i = itR.index; i >= itL.index; i--) {
				bool isEdge = i == 0 || i == _set.Count - 1;
				var (found, removed) = _set.RemoveAt(i);
				if (isEdge == false && found) {
					removes.Add((Math.Max(l, removed.L), Math.Min(r, removed.R), removed.Value));
				}
			}

			if (itL.value.L < l) {
				_set.Add(new LR(itL.value.L, Math.Min(itL.value.R, l), itL.value.Value));
			}

			if (r < itR.value.R) {
				_set.Add(new LR(Math.Max(itR.value.L, r), itR.value.R, itR.value.Value));
			}

			return removes;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Mex(int p = 0)
		{
			var lower = _set.LowerBound(new LR(p, p, default));
			if (lower.value.L != p) {
				lower = _set.Prev(lower);
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
			var lower = _set.LowerBound(new LR(p, p, default));
			if (lower.value.L == p) {
				return (lower.index, lower.value.L, lower.value.R);
			}

			var (index, value) = _set.Prev(lower);
			if (value.R > p) {
				return (index, value.L, value.R);
			} else {
				return (lower.index, lower.value.L, lower.value.R);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int index, int l, int r) UpperBound(int p)
		{
			var (index, value) = _set.UpperBound(new LR(p, p, default));
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
		private readonly RedBlackTree<LR> _set;
		private readonly bool _mergesAdjacentInterval;

		public IntervalSet64(long inf = long.MaxValue, bool mergesAdjacentInterval = true)
		{
			_mergesAdjacentInterval = mergesAdjacentInterval;
			_set = new RedBlackTree<LR>(false) {
				new LR(-inf, -inf),
				new LR(inf, inf),
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long l, long r) GetInterval(long p)
		{
			var lower = _set.LowerBound(new LR(p, p));
			if (lower.value.L != p) {
				lower = _set.Prev(lower);
			}

			var (index, value) = lower;
			if (index < 0 || value.R <= p) {
				return _set[^1].ToTuple();
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
			var right = _set.UpperBound(new LR(p, p));
			var (_, left) = _set.Prev(right);
			if (left.L <= p && p < left.R) {
				return;
			}

			bool willMerge = _mergesAdjacentInterval
				? right.value.L == p + 1
				: right.value.L == p;
			if (left.R == p) {
				if (willMerge == false) {
					_set.Remove(left);
					_set.Add(new LR(left.L, p + 1));
				} else {
					_set.Remove(left);
					_set.Remove(right.value);
					_set.Add(new LR(left.L, right.value.R));
				}
			} else {
				if (willMerge == false) {
					_set.Add(new LR(p, p + 1));
				} else {
					_set.Remove(right.value);
					_set.Add(new LR(p, right.value.R));
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(long l, long r)
		{
			if (l > r) {
				(l, r) = (r, l);
			}

			var it = _set.LowerBound(new LR(l, r));
			var t = _set[it.index - 1];
			bool willMerge = _mergesAdjacentInterval
				? t.L <= l && l <= t.R
				: t.L < l && l < t.R;
			if (willMerge) {
				l = Math.Min(l, t.L);
				r = Math.Max(r, t.R);
				_set.RemoveAt(it.index - 1);
			}

			it = _set.LowerBound(new LR(l, r));
			while (true) {
				willMerge = _mergesAdjacentInterval
					 ? l <= it.value.L && it.value.L <= r
					 : l < it.value.L && it.value.L < r;
				if (willMerge) {
					r = Math.Max(r, it.value.R);
					_set.RemoveAt(it.index);
					it = (it.index, _set[it.index]);
				} else {
					break;
				}
			}

			_set.Add(new LR(l, r));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Remove(long p)
		{
			var (_, value) = _set.Prev(_set.UpperBound(new LR(p, p)));
			if (value.R <= p) {
				return false;
			}

			_set.Remove(value);
			if (value.L < p) {
				_set.Add(new LR(value.L, p));
			}

			if (p + 1 < value.R) {
				_set.Add(new LR(p + 1, value.R));
			}

			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IReadOnlyList<(long l, long r)> Remove(long l, long r)
		{
			if (l > r) {
				(l, r) = (r, l);
			}

			var itL = _set.LowerBound(new LR(l, l));
			var itR = _set.UpperBound(new LR(r, r));
			itL = _set.Prev(itL);
			itR = _set.Prev(itR);
			var removes = new List<(long l, long r)>();
			for (int i = itR.index; i >= itL.index; i--) {
				bool isEdge = i == 0 || i == _set.Count - 1;
				var (found, removed) = _set.RemoveAt(i);
				if (isEdge == false && found) {
					removes.Add((Math.Max(l, removed.L), Math.Min(r, removed.R)));
				}
			}

			if (itL.value.L < l) {
				_set.Add(new LR(itL.value.L, Math.Min(itL.value.R, l)));
			}

			if (r < itR.value.R) {
				_set.Add(new LR(Math.Max(itR.value.L, r), itR.value.R));
			}

			return removes;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long Mex(long p = 0)
		{
			var lower = _set.LowerBound(new LR(p, p));
			if (lower.value.L != p) {
				lower = _set.Prev(lower);
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
			var lower = _set.LowerBound(new LR(p, p));
			if (lower.value.L == p) {
				return (lower.index, lower.value.L, lower.value.R);
			}

			var (index, value) = _set.Prev(lower);
			if (value.R > p) {
				return (index, value.L, value.R);
			} else {
				return (lower.index, lower.value.L, lower.value.R);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int index, long l, long r) UpperBound(long p)
		{
			var (index, value) = _set.UpperBound(new LR(p, p));
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
		private readonly RedBlackTree<LR> _set;

		public IntervalSet64(long inf = long.MaxValue)
		{
			_set = new RedBlackTree<LR>(false) {
				new LR(-inf, -inf, default),
				new LR(inf, inf, default),
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long l, long r) GetInterval(long p)
		{
			var lower = _set.LowerBound(new LR(p, p, default));
			if (lower.value.L != p) {
				lower = _set.Prev(lower);
			}

			var (index, value) = lower;
			if (index < 0 || value.R <= p) {
				return _set[^1].ToTuple();
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
			_set.Add(new LR(l, r, value));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IReadOnlyList<(long l, long r, T value)> Remove(long l, long r)
		{
			if (l > r) {
				(l, r) = (r, l);
			}

			var itL = _set.LowerBound(new LR(l, l, default));
			var itR = _set.UpperBound(new LR(r, r, default));
			itL = _set.Prev(itL);
			itR = _set.Prev(itR);
			var removes = new List<(long l, long r, T value)>();
			for (int i = itR.index; i >= itL.index; i--) {
				bool isEdge = i == 0 || i == _set.Count - 1;
				var (found, removed) = _set.RemoveAt(i);
				if (isEdge == false && found) {
					removes.Add((Math.Max(l, removed.L), Math.Min(r, removed.R), removed.Value));
				}
			}

			if (itL.value.L < l) {
				_set.Add(new LR(itL.value.L, Math.Min(itL.value.R, l), itL.value.Value));
			}

			if (r < itR.value.R) {
				_set.Add(new LR(Math.Max(itR.value.L, r), itR.value.R, itR.value.Value));
			}

			return removes;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long Mex(long p = 0)
		{
			var lower = _set.LowerBound(new LR(p, p, default));
			if (lower.value.L != p) {
				lower = _set.Prev(lower);
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
			var lower = _set.LowerBound(new LR(p, p, default));
			if (lower.value.L == p) {
				return (lower.index, lower.value.L, lower.value.R);
			}

			var (index, value) = _set.Prev(lower);
			if (value.R > p) {
				return (index, value.L, value.R);
			} else {
				return (lower.index, lower.value.L, lower.value.R);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int index, long l, long r) UpperBound(long p)
		{
			var (index, value) = _set.UpperBound(new LR(p, p, default));
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
		private readonly RedBlackTree<LR> _set;
		private readonly bool _mergesAdjacentInterval;

		public IntervalSetAny(TRange inf, TRange minf, bool mergesAdjacentInterval)
		{
			_mergesAdjacentInterval = mergesAdjacentInterval;
			_set = new RedBlackTree<LR>(false) {
				new LR(minf, minf),
				new LR(inf, inf),
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (TRange l, TRange r) GetInterval(TRange p)
		{
			var lower = _set.LowerBound(new LR(p, p));
			if (lower.value.L.CompareTo(p) != 0) {
				lower = _set.Prev(lower);
			}

			var (index, value) = lower;
			if (index < 0 || value.R.CompareTo(p) <= 0) {
				return _set[^1].ToTuple();
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

			var it = _set.LowerBound(new LR(l, r));
			var t = _set[it.index - 1];
			bool willMerge = _mergesAdjacentInterval
				? t.L.CompareTo(l) <= 0 && l.CompareTo(t.R) <= 0
				: t.L.CompareTo(l) < 0 && l.CompareTo(t.R) < 0;
			if (willMerge) {
				if (l.CompareTo(t.L) > 0) {
					l = t.L;
				}

				if (r.CompareTo(t.R) < 0) {
					r = t.R;
				}

				_set.RemoveAt(it.index - 1);
			}

			it = _set.LowerBound(new LR(l, r));
			while (true) {
				willMerge = _mergesAdjacentInterval
					 ? l.CompareTo(it.value.L) <= 0 && it.value.L.CompareTo(r) <= 0
					 : l.CompareTo(it.value.L) < 0 && it.value.L.CompareTo(r) < 0;

				if (willMerge) {
					if (r.CompareTo(it.value.R) < 0) {
						r = it.value.R;
					}

					_set.RemoveAt(it.index);
					it = (it.index, _set[it.index]);
				} else {
					break;
				}
			}

			_set.Add(new LR(l, r));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IReadOnlyList<(TRange l, TRange r)> Remove(TRange l, TRange r)
		{
			if (l.CompareTo(r) > 0) {
				(l, r) = (r, l);
			}

			var itL = _set.LowerBound(new LR(l, l));
			var itR = _set.UpperBound(new LR(r, r));
			itL = _set.Prev(itL);
			itR = _set.Prev(itR);
			var removes = new List<(TRange l, TRange r)>();
			for (int i = itR.index; i >= itL.index; i--) {
				bool isEdge = i == 0 || i == _set.Count - 1;
				var (found, removed) = _set.RemoveAt(i);
				if (isEdge == false && found) {
					removes.Add((
						l.CompareTo(removed.L) >= 0 ? l : removed.L,
						r.CompareTo(removed.R) <= 0 ? r : removed.R));
				}

				_set.RemoveAt(i);
			}

			if (itL.value.L.CompareTo(l) < 0) {
				TRange min = itL.value.R.CompareTo(l) <= 0 ? itL.value.R : l;
				_set.Add(new LR(itL.value.L, min));
			}

			if (r.CompareTo(itR.value.R) < 0) {
				TRange max = itR.value.L.CompareTo(r) >= 0 ? itR.value.L : r;
				_set.Add(new LR(max, itR.value.R));
			}

			return removes;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TRange Mex(TRange p)
		{
			var lower = _set.LowerBound(new LR(p, p));
			if (lower.value.L.CompareTo(p) != 0) {
				lower = _set.Prev(lower);
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
			var lower = _set.LowerBound(new LR(p, p));
			if (lower.value.L.CompareTo(p) == 0) {
				return (lower.index, lower.value.L, lower.value.R);
			}

			var (index, value) = _set.Prev(lower);
			if (value.R.CompareTo(p) > 0) {
				return (index, value.L, value.R);
			} else {
				return (lower.index, lower.value.L, lower.value.R);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int index, TRange l, TRange r) UpperBound(TRange p)
		{
			var (index, value) = _set.UpperBound(new LR(p, p));
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
		private readonly RedBlackTree<LR> _set;

		public IntervalSetAny(TRange inf, TRange minf)
		{
			_set = new RedBlackTree<LR>(false) {
				new LR(minf, minf, default),
				new LR(inf, inf, default),
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (TRange l, TRange r) GetInterval(TRange p)
		{
			var lower = _set.LowerBound(new LR(p, p, default));
			if (lower.value.L.CompareTo(p) != 0) {
				lower = _set.Prev(lower);
			}

			var (index, value) = lower;
			if (index < 0 || value.R.CompareTo(p) <= 0) {
				return _set[^1].ToTuple();
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
			_set.Add(new LR(l, r, value));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IReadOnlyList<(TRange l, TRange r)> Remove(TRange l, TRange r)
		{
			if (l.CompareTo(r) > 0) {
				(l, r) = (r, l);
			}

			var itL = _set.LowerBound(new LR(l, l, default));
			var itR = _set.UpperBound(new LR(r, r, default));
			itL = _set.Prev(itL);
			itR = _set.Prev(itR);
			var removes = new List<(TRange l, TRange r)>();
			for (int i = itR.index; i >= itL.index; i--) {
				bool isEdge = i == 0 || i == _set.Count - 1;
				var (found, removed) = _set.RemoveAt(i);
				if (isEdge == false && found) {
					removes.Add((
						l.CompareTo(removed.L) >= 0 ? l : removed.L,
						r.CompareTo(removed.R) <= 0 ? r : removed.R));
				}

				_set.RemoveAt(i);
			}

			if (itL.value.L.CompareTo(l) < 0) {
				TRange min = itL.value.R.CompareTo(l) <= 0 ? itL.value.R : l;
				_set.Add(new LR(itL.value.L, min, itL.value.Value));
			}

			if (r.CompareTo(itR.value.R) < 0) {
				TRange max = itR.value.L.CompareTo(r) >= 0 ? itR.value.L : r;
				_set.Add(new LR(max, itR.value.R, itR.value.Value));
			}

			return removes;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TRange Mex(TRange p)
		{
			var lower = _set.LowerBound(new LR(p, p, default));
			if (lower.value.L.CompareTo(p) != 0) {
				lower = _set.Prev(lower);
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
			var lower = _set.LowerBound(new LR(p, p, default));
			if (lower.value.L.CompareTo(p) == 0) {
				return (lower.index, lower.value.L, lower.value.R);
			}

			var (index, value) = _set.Prev(lower);
			if (value.R.CompareTo(p) > 0) {
				return (index, value.L, value.R);
			} else {
				return (lower.index, lower.value.L, lower.value.R);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int index, TRange l, TRange r) UpperBound(TRange p)
		{
			var (index, value) = _set.UpperBound(new LR(p, p, default));
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
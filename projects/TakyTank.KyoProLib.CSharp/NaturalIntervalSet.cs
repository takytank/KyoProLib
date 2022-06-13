using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class NaturalIntervalSet<T>
	{
		private readonly IntervalPivotTree<T> _set;

		public NaturalIntervalSet(long max)
		{
			max += IntervalPivotTree<T>.OFFSET;
			int height = 0;
			while (max > 0) {
				++height;
				max >>= 1;
			}

			_set = new IntervalPivotTree<T>(height);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long l, long r) GetInterval(long p)
		{
			var lower = _set.LowerBound(p);
			if (lower.l != p) {
				lower = _set.Prev(lower.l);
			}

			if (_set.IsEdge(lower.l) || lower.r <= p) {
				return (-1, -1);
			} else {
				return (lower.l, lower.r);
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
			_set.Add(l, r, value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IReadOnlyList<(long l, long r, T value)> Remove(long l, long r)
		{
			if (l > r) {
				(l, r) = (r, l);
			}

			var itL = _set.LowerBound(l);
			var itR = _set.UpperBound(r);
			itL = _set.Prev(itL.l);
			itR = _set.Prev(itR.l);
			var removes = new List<(long l, long r, T value)>();
			var current = itR;
			while (current.l >= itL.l) {
				_set.Remove(current.l);
				if (_set.IsEdge(current.l) == false) {
					removes.Add((Math.Max(l, current.l), Math.Min(r, current.r), current.value));
				}

				current = _set.Prev(current.l);
			}

			if (itL.l < l) {
				_set.Add(itL.l, Math.Min(itL.r, l), itL.value);
			}

			if (r < itR.r) {
				_set.Add(Math.Max(itR.l, r), itR.r, itR.value);
			}

			return removes;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long Mex(long p = 0)
		{
			var lower = _set.LowerBound(p);
			if (lower.l != p) {
				lower = _set.Prev(lower.l);
			}

			if (_set.IsEdge(lower.l) || lower.r <= p) {
				return p;
			} else {
				return lower.r;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long l, long r) LowerBound(long p)
		{
			var lower = _set.LowerBound(p);
			if (lower.l == p) {
				return (lower.l, lower.r);
			}

			var (l, r, _) = _set.Prev(lower.l);
			if (r > p) {
				return (l, r);
			} else {
				return (lower.l, lower.r);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long l, long r) UpperBound(long p)
		{
			var (l, r, _) = _set.UpperBound(p);
			return (l, r);
		}

		private class IntervalPivotTree<TValue>
		{
			public const int OFFSET = 2;
			private readonly int _height;
			private readonly Node _root;
			private int _count = 0;

			public IntervalPivotTree(int height)
			{
				_height = height;
				_root = new Node(1L << height, 1L << height, default(TValue), 1L << height);
				_count = 1;
				Add(-1, -1, default(TValue));
			}

			public int Count => _count;
			public long Inf => (1L << _height) - OFFSET;
			public (long l, long r, TValue value) Max => Prev(Inf);
			public (long l, long r, TValue value) Min => Next(-1);

			public bool IsEdge(long l) => l < 0 || l >= Inf;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Add(long l, long r, TValue value)
			{
				l += OFFSET;
				r += OFFSET;
				var node = _root;
				while (true) {
					if (l < node.L) {
						if (l < node.Pivot) {
							if (node.Left != null) {
								node = node.Left;
							} else {
								long p = node.Pivot;
								node.Left = new Node(l, r, value, p - (p & -p) / 2);
								break;
							}
						} else {
							long tempL = node.L;
							long tempR = node.R;
							TValue tempV = node.Value;
							node.L = l;
							node.R = r;
							node.Value = value;
							if (node.Right != null) {
								node = node.Right;
								l = tempL;
								r = tempR;
								value = tempV;
							} else {
								long p = node.Pivot;
								node.Right = new Node(tempL, tempR, tempV, p + (p & -p) / 2);
								break;
							}
						}
					} else if (l > node.L) {
						if (node.L < node.Pivot) {
							long tempL = node.L;
							long tempR = node.R;
							TValue tempV = node.Value;
							node.L = l;
							node.R = r;
							node.Value = value;
							if (node.Left != null) {
								node = node.Left;
								l = tempL;
								r = tempR;
								value = tempV;
							} else {
								long p = node.Pivot;
								node.Left = new Node(tempL, tempR, tempV, p - (p & -p) / 2);
								break;
							}
						} else {
							if (node.Right != null) {
								node = node.Right;
							} else {
								long p = node.Pivot;
								node.Right = new Node(l, r, value, p + (p & -p) / 2);
								break;
							}
						}
					} else {
						return;
					}
				}

				++_count;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Remove(long v) => RemoveCore(v);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private void RemoveCore(long v, Node node = null, Node prev = null)
			{
				v += OFFSET;
				if (node == null) {
					node = _root;
				}

				if (prev == null) {
					prev = node;
				}

				while (v != node.L) {
					prev = node;
					if (v <= node.L) {
						if (node.Left != null) {
							node = node.Left;
						} else {
							return;
						}
					} else {
						if (node.Right != null) {
							node = node.Right;
						} else {
							return;
						}
					}
				}

				if (node.Left == null && node.Right == null) {
					if (prev.Left == null) {
						prev.Right = null;
					} else if (prev.Right == null) {
						prev.Left = null;
					} else {
						if (node.Pivot == prev.Left.Pivot) {
							prev.Left = null;
						} else {
							prev.Right = null;
						}
					}

					--_count;
				} else if (node.Right != null) {
					var leftest = LeftMost(node.Right);
					node.L = leftest.L;
					node.R = leftest.R;
					node.Value = leftest.Value;
					RemoveCore(node.L - OFFSET, node.Right, node);
				} else {
					var rightest = RightMost(node.Left);
					node.L = rightest.L;
					node.R = rightest.R;
					node.Value = rightest.Value;
					RemoveCore(node.L - OFFSET, node.Left, node);
				}
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public (long l, long r, TValue value) LowerBound(long l) => Next(l - 1);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public (long l, long r, TValue value) UpperBound(long l) => Next(l);

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public (long l, long r, TValue value) FindL(long v) => Prev(v);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public (long l, long r, TValue value) Prev(long v)
			{
				v += OFFSET;
				var node = _root;
				var prev = new Node(-2 + OFFSET, -2 + OFFSET, default(TValue), 0);
				if (node.L < v) {
					prev = node;
				}

				while (true) {
					if (v <= node.L) {
						if (node.Left != null) {
							node = node.Left;
						} else {
							return prev.ToInterval();
						}
					} else {
						prev = node;
						if (node.Right != null) {
							node = node.Right;
						} else {
							return prev.ToInterval();
						}
					}
				}
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public (long l, long r, TValue value) FindR(long v) => Next(v);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public (long l, long r, TValue value) Next(long v)
			{
				v += OFFSET;
				var node = _root;
				var next = new Node(Inf + OFFSET, Inf + OFFSET, default(TValue), 0);
				if (node.L > v) {
					next = node;
				}

				while (true) {
					if (v < node.L) {
						next = node;
						if (node.Left != null) {
							node = node.Left;
						} else {
							return next.ToInterval();
						}
					} else {
						if (node.Right != null) {
							node = node.Right;
						} else {
							return next.ToInterval();
						}
					}
				}
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private Node LeftMost(Node node)
			{
				if (node.Left != null) {
					return LeftMost(node.Left);
				}

				return node;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private Node RightMost(Node node)
			{
				if (node.Right != null) {
					return RightMost(node.Right);
				}

				return node;
			}

			private class Node
			{
				public long L { get; set; }
				public long R { get; set; }
				public TValue Value { get; set; }
				public long Pivot { get; set; }
				public Node Left { get; set; }
				public Node Right { get; set; }
				public Node(long l, long r, TValue value, long pivot)
				{
					L = l;
					R = r;
					Value = value;
					Pivot = pivot;
				}

				public (long l, long r, TValue value) ToInterval()
					=> (L - OFFSET, R - OFFSET, Value);
			}
		}
	}
}

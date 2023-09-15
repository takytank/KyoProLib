using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class NaturalIntervalSet : IEnumerable<(long l, long r)>
	{
		private readonly IntervalPivotTree _set;
		private readonly bool _mergesAdjacentInterval;

		public NaturalIntervalSet(long max, bool mergesAdjacentInterval = true)
		{
			_mergesAdjacentInterval = mergesAdjacentInterval;
			max += IntervalPivotTree.OFFSET;
			int height = 0;
			while (max > 0) {
				++height;
				max >>= 1;
			}

			_set = new IntervalPivotTree(height);
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
		public void Add(long p)
		{
			var (rightL, rightR) = _set.UpperBound(p);
			var (leftL, leftR) = _set.Prev(rightL);
			if (leftL <= p && p < leftR) {
				return;
			}

			bool willMerge = _mergesAdjacentInterval
				? rightL == p + 1
				: rightL == p;
			if (leftR == p) {
				if (willMerge == false) {
					_set.Remove(leftL);
					_set.Add(leftL, p + 1);
				} else {
					_set.Remove(leftL);
					_set.Remove(rightL);
					_set.Add(leftL, rightR);
				}
			} else {
				if (willMerge == false) {
					_set.Add(p, p + 1);
				} else {
					_set.Remove(rightL);
					_set.Add(p, rightR);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(long l, long r)
		{
			if (l > r) {
				(l, r) = (r, l);
			}

			var it = _set.LowerBound(l);
			var t = _set.Prev(it.l);
			bool willMerge = _mergesAdjacentInterval
				? t.l <= l && l <= t.r
				: t.l < l && l < t.r;
			if (willMerge) {
				l = Math.Min(l, t.l);
				r = Math.Max(r, t.r);
				_set.Remove(_set.Prev(it.l).l);
			}

			it = _set.LowerBound(l);
			while (true) {
				willMerge = _mergesAdjacentInterval
					 ? l <= it.l && it.l <= r
					 : l < it.l && it.l < r;
				if (willMerge) {
					r = Math.Max(r, it.r);
					_set.Remove(it.l);
					it = _set.Next(it.l);
				} else {
					break;
				}
			}

			_set.Add(l, r);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Remove(long p)
		{
			var (l, r) = _set.Prev(_set.UpperBound(p).l);
			if (r <= p) {
				return false;
			}

			_set.Remove(l);
			if (l < p) {
				_set.Add(l, p);
			}

			if (p + 1 < r) {
				_set.Add(p + 1, r);
			}

			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IReadOnlyList<(long l, long r)> Remove(long l, long r)
		{
			if (l > r) {
				(l, r) = (r, l);
			}

			var itL = _set.LowerBound(l);
			if (itL.r > l) {
				var temp = itL;
				itL = _set.Prev(itL.l);
				if (itL.r <= l) {
					itL = temp;
				}
			}

			var itR = _set.UpperBound(r);
			itR = _set.Prev(itR.l);
			if (itR.l >= r) {
				itR = _set.Prev(itR.l);
			}

			var removes = new List<(long l, long r)>();
			var current = itR;
			while (current.l >= itL.l) {
				_set.Remove(current.l);
				if (_set.IsEdge(current.l) == false) {
					removes.Add((Math.Max(l, current.l), Math.Min(r, current.r)));
				}

				current = _set.Prev(current.l);
			}

			if (itL.l < l) {
				_set.Add(itL.l, Math.Min(itL.r, l));
			}

			if (r < itR.r) {
				_set.Add(Math.Max(itR.l, r), itR.r);
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

			var (l, r) = _set.Prev(lower.l);
			if (r > p) {
				return (l, r);
			} else {
				return (lower.l, lower.r);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long l, long r) UpperBound(long p)
		{
			var (l, r) = _set.UpperBound(p);
			return (l, r);
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		public IEnumerator<(long l, long r)> GetEnumerator()
		{
			foreach (var lr in _set) {
				yield return lr;
			}
		}

		private class IntervalPivotTree : IEnumerable<(long l, long r)>
		{
			public const int OFFSET = 2;
			private readonly int _height;
			private readonly long _inf;
			private readonly Node _root;
			private readonly Stack<Node> _pool = new Stack<Node>();
			private int _count = 0;

			public int Count => _count;
			public long Inf => _inf;
			public (long l, long r) Max => Prev(Inf);
			public (long l, long r) Min => Next(-1);

			public bool IsEdge(long l) => l < 0 || l >= Inf;

			public IntervalPivotTree(int height)
			{
				_height = height;
				_inf = (1L << _height) - OFFSET;
				_root = new Node(_inf + OFFSET, _inf + OFFSET, _inf + OFFSET);
				_count = 1;
				Add(-1, -1);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Add(long l, long r)
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
								node.Left = NewNode(l, r, p - (p & -p) / 2);
								break;
							}
						} else {
							long tempL = node.L;
							long tempR = node.R;
							node.L = l;
							node.R = r;
							if (node.Right != null) {
								node = node.Right;
								l = tempL;
								r = tempR;
							} else {
								long p = node.Pivot;
								node.Right = NewNode(tempL, tempR, p + (p & -p) / 2);
								break;
							}
						}
					} else if (l > node.L) {
						if (node.L < node.Pivot) {
							long tempL = node.L;
							long tempR = node.R;
							node.L = l;
							node.R = r;
							if (node.Left != null) {
								node = node.Left;
								l = tempL;
								r = tempR;
							} else {
								long p = node.Pivot;
								node.Left = NewNode(tempL, tempR, p - (p & -p) / 2);
								break;
							}
						} else {
							if (node.Right != null) {
								node = node.Right;
							} else {
								long p = node.Pivot;
								node.Right = NewNode(l, r, p + (p & -p) / 2);
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

					_pool.Push(node);
					--_count;
				} else if (node.Right != null) {
					var leftest = LeftMost(node.Right);
					node.L = leftest.L;
					node.R = leftest.R;
					RemoveCore(node.L - OFFSET, node.Right, node);
				} else {
					var rightest = RightMost(node.Left);
					node.L = rightest.L;
					node.R = rightest.R;
					RemoveCore(node.L - OFFSET, node.Left, node);
				}
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public (long l, long r) LowerBound(long l) => Next(l - 1);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public (long l, long r) UpperBound(long l) => Next(l);

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public (long l, long r) FindL(long v) => Prev(v);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public (long l, long r) Prev(long v)
			{
				v += OFFSET;
				var node = _root;
				var prev = new Node(-2 + OFFSET, -2 + OFFSET, 0);
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
			public (long l, long r) FindR(long v) => Next(v);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public (long l, long r) Next(long v)
			{
				v += OFFSET;
				var node = _root;
				var next = new Node(Inf + OFFSET, Inf + OFFSET, 0);
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

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
			public IEnumerator<(long l, long r)> GetEnumerator()
			{
				long inf = Inf;
				var stack = new Stack<Node>();
				Node node = _root;
				Node next;
				while (node != null) {
					next = node.Left;
					stack.Push(node);
					node = next;
				}

				while (stack.Count > 0) {
					var current = stack.Pop();
					var (l, r) = (current.L - OFFSET, current.R - OFFSET);
					if (l >= 0 && r < inf) {
						yield return (l, r);
					}

					node = current.Right;
					while (node != null) {
						next = node.Left;
						stack.Push(node);
						node = next;
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

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private Node NewNode(long l, long r, long pivot)
			{
				if (_pool.Count > 0) {
					var node = _pool.Pop();
					node.L = l;
					node.R = r;
					node.Pivot = pivot;
					node.Left = null;
					node.Right = null;
					return node;
				} else {
					return new Node(l, r, pivot);
				}
			}

			private class Node
			{
				public long L { get; set; }
				public long R { get; set; }
				public long Pivot { get; set; }
				public Node Left { get; set; }
				public Node Right { get; set; }
				public Node(long l, long r, long pivot)
				{
					L = l;
					R = r;
					Pivot = pivot;
				}

				public (long l, long r) ToInterval()
					=> (L - OFFSET, R - OFFSET);
			}
		}
	}

	public class NaturalIntervalSet<T> : IEnumerable<(long l, long r)>
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
			if (itL.r > l) {
				var temp = itL;
				itL = _set.Prev(itL.l);
				if (itL.r <= l) {
					itL = temp;
				}
			}

			var itR = _set.UpperBound(r);
			itR = _set.Prev(itR.l);
			if (itR.l >= r) {
				itR = _set.Prev(itR.l);
			}

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

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		public IEnumerator<(long l, long r)> GetEnumerator()
		{
			foreach (var lr in _set) {
				yield return lr;
			}
		}

		private class IntervalPivotTree<TValue> : IEnumerable<(long l, long r)>
		{
			public const int OFFSET = 2;
			private readonly int _height;
			private readonly long _inf;
			private readonly Node _root;
			private readonly Stack<Node> _pool = new Stack<Node>();
			private int _count = 0;

			public int Count => _count;
			public long Inf => _inf;
			public (long l, long r, TValue value) Max => Prev(Inf);
			public (long l, long r, TValue value) Min => Next(-1);

			public bool IsEdge(long l) => l < 0 || l >= Inf;

			public IntervalPivotTree(int height)
			{
				_height = height;
				_inf = (1L << _height) - OFFSET;
				_root = new Node(_inf + OFFSET, _inf + OFFSET, default(TValue), _inf + OFFSET);
				_count = 1;
				Add(-1, -1, default(TValue));
			}

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
								node.Left = NewNode(l, r, value, p - (p & -p) / 2);
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
								node.Right = NewNode(tempL, tempR, tempV, p + (p & -p) / 2);
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
								node.Left = NewNode(tempL, tempR, tempV, p - (p & -p) / 2);
								break;
							}
						} else {
							if (node.Right != null) {
								node = node.Right;
							} else {
								long p = node.Pivot;
								node.Right = NewNode(l, r, value, p + (p & -p) / 2);
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

					_pool.Push(node);
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

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
			public IEnumerator<(long l, long r)> GetEnumerator()
			{
				long inf = Inf;
				var stack = new Stack<Node>();
				Node node = _root;
				Node next;
				while (node != null) {
					next = node.Left;
					stack.Push(node);
					node = next;
				}

				while (stack.Count > 0) {
					var current = stack.Pop();
					var (l, r) = (current.L - OFFSET, current.R - OFFSET);
					if (l >= 0 && r < inf) {
						yield return (l, r);
					}

					node = current.Right;
					while (node != null) {
						next = node.Left;
						stack.Push(node);
						node = next;
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

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private Node NewNode(long l, long r, TValue value, long pivot)
			{
				if (_pool.Count > 0) {
					var node = _pool.Pop();
					node.L = l;
					node.R = r;
					node.Value = value;
					node.Pivot = pivot;
					node.Left = null;
					node.Right = null;
					return node;
				} else {
					return new Node(l, r, value, pivot);
				}
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

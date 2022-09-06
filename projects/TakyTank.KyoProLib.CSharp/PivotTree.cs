using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class PivotTree : IReadOnlyCollection<long>
	{
		private const int OFFSET = 1;
		private readonly int _height;
		private readonly Node _root;
		private readonly Stack<Node> _pool = new Stack<Node>();
		private int _count = 0;

		public PivotTree(long max)
		{
			max += OFFSET;
			int height = 0;
			while (max > 0) {
				++height;
				max >>= 1;
			}

			_height = height;
			_root = new Node(1L << height, 1L << height);
		}

		public int Count => _count;
		public long Inf => (1L << _height) - OFFSET;
		public long Max => Prev(Inf);
		public long Min => Next(-1);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Contains(long v) => Next(v - 1) == v;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(long v, bool exclusive = false)
		{
			v += OFFSET;
			var node = _root;
			while (true) {
				if (v == node.Value) {
					if (exclusive) {
						Remove(v - OFFSET);
					}

					return;
				} else {
					long min = Math.Min(v, node.Value);
					long max = Math.Max(v, node.Value);
					if (min < node.Pivot) {
						node.Value = max;
						if (node.Left != null) {
							node = node.Left;
							v = min;
						} else {
							long p = node.Pivot;
							node.Left = NewNode(min, p - (p & -p) / 2);
							break;
						}
					} else {
						node.Value = min;
						if (node.Right != null) {
							node = node.Right;
							v = max;
						} else {
							long p = node.Pivot;
							node.Right = NewNode(max, p + (p & -p) / 2);
							break;
						}
					}
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

			while (v != node.Value) {
				prev = node;
				if (v <= node.Value) {
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
				node.Value = LeftMost(node.Right).Value;
				RemoveCore(node.Value - OFFSET, node.Right, node);
			} else {
				node.Value = RightMost(node.Left).Value;
				RemoveCore(node.Value - OFFSET, node.Left, node);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long LowerBound(long v) => Next(v - 1);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long UpperBound(long v) => Next(v);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long FindL(long v) => Prev(v);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long Prev(long v)
		{
			v += OFFSET;
			var node = _root;
			long prev = 0;
			if (node.Value < v) {
				prev = node.Value;
			}

			while (true) {
				if (v <= node.Value) {
					if (node.Left != null) {
						node = node.Left;
					} else {
						return prev - OFFSET;
					}
				} else {
					prev = node.Value;
					if (node.Right != null) {
						node = node.Right;
					} else {
						return prev - OFFSET;
					}
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long FindR(long v) => Next(v);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long Next(long v)
		{
			v += OFFSET;
			var node = _root;
			long next = 0;
			if (node.Value > v) {
				next = node.Value;
			}

			while (true) {
				if (v < node.Value) {
					next = node.Value;
					if (node.Left != null) {
						node = node.Left;
					} else {
						return next - OFFSET;
					}
				} else {
					if (node.Right != null) {
						node = node.Right;
					} else {
						return next - OFFSET;
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Node NewNode(long value, long pivot)
		{
			if (_pool.Count > 0) {
				var node = _pool.Pop();
				node.Value = value;
				node.Pivot = pivot;
				node.Left = null;
				node.Right = null;
				return node;
			} else {
				return new Node(value, pivot);
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
		public IEnumerator<long> GetEnumerator()
		{
			var stack = new Stack<Node>(_height * 2);
			var node = _root;
			Node next;
			while (node != null) {
				next = node.Left;
				stack.Push(node);
				node = next;
			}

			while (stack.Count > 0) {
				var cur = stack.Pop();
				yield return cur.Value - OFFSET;

				node = cur.Right;
				while (node != null) {
					next = node.Left;
					stack.Push(node);
					node = next;
				}
			}
		}

		private class Node
		{
			public long Value { get; set; }
			public long Pivot { get; set; }
			public Node Left { get; set; }
			public Node Right { get; set; }
			public Node(long value, long pivot)
			{
				Value = value;
				Pivot = pivot;
			}
		}
	}

	public class IndexPivotTree : IReadOnlyCollection<long>
	{
		private const int OFFSET = 1;
		private readonly int _height;
		private readonly Node _root;
		private readonly Stack<Node> _pool = new Stack<Node>();
		private int _count = 0;

		public IndexPivotTree(long max)
		{
			max += OFFSET;
			int height = 0;
			while (max > 0) {
				++height;
				max >>= 1;
			}

			_height = height;
			_root = new Node(1L << height, 1L << height);
		}

		public int Count => _count;
		public long Inf => (1L << _height) - OFFSET;
		public (int index, long value) Max => Prev(Inf);
		public (int index, long value) Min => Next(-1);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Contains(long v) => Next(v - 1).value == v;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(long v, bool exclusive = false)
		{
			v += OFFSET;
			var node = _root;
			while (true) {
				if (v == node.Value) {
					if (exclusive) {
						Remove(v - OFFSET);
					}

					return;
				} else {
					long min = Math.Min(v, node.Value);
					long max = Math.Max(v, node.Value);
					if (min < node.Pivot) {
						node.Value = max;
						if (node.Left != null) {
							node = node.Left;
							v = min;
						} else {
							long p = node.Pivot;
							node.Left = NewNode(min, p - (p & -p) / 2);
							node.Left.Parent = node;
							UpdateSize(node);
							break;
						}
					} else {
						node.Value = min;
						if (node.Right != null) {
							node = node.Right;
							v = max;
						} else {
							long p = node.Pivot;
							node.Right = NewNode(max, p + (p & -p) / 2);
							node.Right.Parent = node;
							UpdateSize(node);
							break;
						}
					}
				}
			}

			++_count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (bool found, int index, long value) RemoveAt(int index)
		{
			if (index < 0 || index >= Count) {
				throw new IndexOutOfRangeException();
			}

			var target = FindNodeByIndex(index);
			var value = target.Value - OFFSET;
			Remove(value);
			return (true, index, value);
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

			while (v != node.Value) {
				prev = node;
				if (v <= node.Value) {
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

				UpdateSize(prev);
				_pool.Push(node);
				--_count;
			} else if (node.Right != null) {
				node.Value = LeftMost(node.Right).Value;
				RemoveCore(node.Value - OFFSET, node.Right, node);
			} else {
				node.Value = RightMost(node.Left).Value;
				RemoveCore(node.Value - OFFSET, node.Left, node);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int index, long value) LowerBound(long l) => Next(l - 1);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int index, long value) UpperBound(long l) => Next(l);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int index, long value) FindL(long v) => Prev(v);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int index, long value) Prev(long v)
		{
			v += OFFSET;
			var node = _root;
			var prev = new Node(0, 0);
			if (node.Value < v) {
				prev = node;
			}

			int index = 0;
			while (true) {
				if (v <= node.Value) {
					if (node.Left != null) {
						node = node.Left;
					} else {
						index--;
						return (index, prev.Value - OFFSET);
					}
				} else {
					prev = node;
					if (node.Right != null) {
						index += CountOf(node.Left) + 1;
						node = node.Right;
					} else {
						index += CountOf(node.Left);
						return (index, prev.Value - OFFSET);
					}
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int index, long value) FindR(long v) => Next(v);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int index, long value) Next(long v)
		{
			v += OFFSET;
			var node = _root;
			var next = new Node(Inf + 2, 0);
			if (node.Value > v) {
				next = node;
			}

			int index = 0;
			while (true) {
				if (v < node.Value) {
					next = node;
					if (node.Left != null) {
						node = node.Left;
					} else {
						index += CountOf(next.Left);
						return (index, next.Value - OFFSET);
					}
				} else {
					if (node.Right != null) {
						index += CountOf(node.Left) + 1;
						node = node.Right;
					} else {
						index += CountOf(node);
						return (index, next.Value - OFFSET);
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Node FindNodeByIndex(int index)
		{
			var current = _root;
			var currentIndex = current.Size - CountOf(current.Right) - 1;
			while (currentIndex != index) {
				if (currentIndex > index) {
					current = current.Left;
					if (current is null) {
						break;
					}

					currentIndex -= CountOf(current.Right) + 1;
				} else {
					current = current.Right;
					if (current is null) {
						break;
					}

					currentIndex += CountOf(current.Left) + 1;
				}
			}

			return current;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int CountOf(Node node) => node?.Size ?? 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void UpdateSize(Node node)
		{
			while (node is null == false) {
				node.Size = 1 + CountOf(node.Left) + CountOf(node.Right);
				node = node.Parent;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Node NewNode(long value, long pivot)
		{
			if (_pool.Count > 0) {
				var node = _pool.Pop();
				node.Value = value;
				node.Pivot = pivot;
				node.Left = null;
				node.Right = null;
				return node;
			} else {
				return new Node(value, pivot);
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
		public IEnumerator<long> GetEnumerator()
		{
			var stack = new Stack<Node>(_height * 2);
			var node = _root;
			Node next;
			while (node != null) {
				next = node.Left;
				stack.Push(node);
				node = next;
			}

			while (stack.Count > 0) {
				var cur = stack.Pop();
				yield return cur.Value - OFFSET;

				node = cur.Right;
				while (node != null) {
					next = node.Left;
					stack.Push(node);
					node = next;
				}
			}
		}

		private class Node
		{
			public long Value { get; set; }
			public long Pivot { get; set; }
			public int Size { get; set; } = 1;
			public Node Parent { get; set; }
			public Node Left { get; set; }
			public Node Right { get; set; }
			public Node(long value, long pivot)
			{
				Value = value;
				Pivot = pivot;
			}
		}
	}

	public class IndexIntervalPivotTree<T>
	{
		private const int OFFSET = 2;
		private readonly int _height;
		private readonly Node _root;
		private int _count = 0;

		public IndexIntervalPivotTree(int height)
		{
			_height = height;
			_root = new Node(1L << height, 1L << height, default(T), 1L << height);
		}

		public int Count => _count;
		public long Inf => (1L << _height) - OFFSET;
		public (int index, (long l, long r, T value) interval) Max => Prev(Inf);
		public (int index, (long l, long r, T value) interval) Min => Next(-1);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(long l, long r, T value)
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
							node.Left = new Node(l, r, value, p - (p & -p) / 2) {
								Parent = node
							};
							UpdateSize(node);
							break;
						}
					} else {
						long tempL = node.L;
						long tempR = node.R;
						T tempV = node.Value;
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
							node.Right = new Node(tempL, tempR, tempV, p + (p & -p) / 2) {
								Parent = node
							};
							UpdateSize(node);
							break;
						}
					}
				} else if (l > node.L) {
					if (node.L < node.Pivot) {
						long tempL = node.L;
						long tempR = node.R;
						T tempV = node.Value;
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
							node.Left = new Node(tempL, tempR, tempV, p - (p & -p) / 2) {
								Parent = node
							};
							UpdateSize(node);
							break;
						}
					} else {
						if (node.Right != null) {
							node = node.Right;
						} else {
							long p = node.Pivot;
							node.Right = new Node(l, r, value, p + (p & -p) / 2) {
								Parent = node
							};
							UpdateSize(node);
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
		public (bool found, int index, (long l, long r, T value) interval) RemoveAt(int index)
		{
			if (index < 0 || index >= Count) {
				throw new IndexOutOfRangeException();
			}

			var target = FindNodeByIndex(index);
			var interval = target.ToInterval();
			Remove(target.L - 2);
			return (true, index, interval);
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

				UpdateSize(prev);
				--_count;
			} else if (node.Right != null) {
				var leftest = LeftMost(node.Right);
				node.L = leftest.L;
				node.R = leftest.R;
				node.Value = leftest.Value;
				RemoveCore(node.L - 2, node.Right, node);
			} else {
				var rightest = RightMost(node.Left);
				node.L = rightest.L;
				node.R = rightest.R;
				node.Value = rightest.Value;
				RemoveCore(node.L - 2, node.Left, node);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int index, (long l, long r, T value) interval) LowerBound(long l) => Next(l - 1);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int index, (long l, long r, T value) interval) UpperBound(long l) => Next(l);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int index, (long l, long r, T value) interval) FindL(long v) => Prev(v);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int index, (long l, long r, T value) interval) Prev(long v)
		{
			v += OFFSET;
			var node = _root;
			var prev = new Node(0, 0, default(T), 0);
			if (node.L < v) {
				prev = node;
			}

			int index = 0;
			while (true) {
				if (v <= node.L) {
					if (node.Left != null) {
						node = node.Left;
					} else {
						index--;
						return (index, prev.ToInterval());
					}
				} else {
					prev = node;
					if (node.Right != null) {
						index += CountOf(node.Left) + 1;
						node = node.Right;
					} else {
						index += CountOf(node.Left);
						return (index, prev.ToInterval());
					}
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int index, (long l, long r, T value) interval) FindR(long v) => Next(v);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int index, (long l, long r, T value) interval) Next(long v)
		{
			v += OFFSET;
			var node = _root;
			var next = new Node(Inf + OFFSET, Inf + OFFSET, default(T), 0);
			if (node.L > v) {
				next = node;
			}

			int index = 0;
			while (true) {
				if (v < node.L) {
					next = node;
					if (node.Left != null) {
						node = node.Left;
					} else {
						index += CountOf(next.Left);
						return (index, next.ToInterval());
					}
				} else {
					if (node.Right != null) {
						index += CountOf(node.Left) + 1;
						node = node.Right;
					} else {
						index += CountOf(node);
						return (index, next.ToInterval());
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Node FindNodeByIndex(int index)
		{
			var current = _root;
			var currentIndex = current.Size - CountOf(current.Right) - 1;
			while (currentIndex != index) {
				if (currentIndex > index) {
					current = current.Left;
					if (current is null) {
						break;
					}

					currentIndex -= CountOf(current.Right) + 1;
				} else {
					current = current.Right;
					if (current is null) {
						break;
					}

					currentIndex += CountOf(current.Left) + 1;
				}
			}

			return current;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int CountOf(Node node) => node?.Size ?? 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void UpdateSize(Node node)
		{
			while (node is null == false) {
				node.Size = 1 + CountOf(node.Left) + CountOf(node.Right);
				node = node.Parent;
			}
		}

		private class Node
		{
			public long L { get; set; }
			public long R { get; set; }
			public T Value { get; set; }
			public long Pivot { get; set; }
			public int Size { get; set; } = 1;
			public Node Parent { get; set; }
			public Node Left { get; set; }
			public Node Right { get; set; }
			public Node(long l, long r, T value, long pivot)
			{
				L = l;
				R = r;
				Value = value;
				Pivot = pivot;
			}

			public (long l, long r, T value) ToInterval()
				=> (L - OFFSET, R - OFFSET, Value);
		}
	}
}

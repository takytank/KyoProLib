using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class MultiPivotTree : IReadOnlyCollection<long>
	{
		private const int OFFSET = 1;
		private readonly int _height;
		private readonly Node _root;
		private readonly Stack<Node> _pool = new Stack<Node>();
		private int _count = 0;

		public MultiPivotTree(long max)
		{
			max += OFFSET;
			int height = 0;
			while (max > 0) {
				++height;
				max >>= 1;
			}

			_height = height;
			_root = new Node(1L << height, 1L << height, 1);
		}

		public int Count => _count;
		public long Inf => (1L << _height) - OFFSET;
		public long Max => Prev(Inf);
		public long Min => Next(-1);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Contains(long v) => Next(v - OFFSET) == v;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(long v)
		{
			v += OFFSET;
			int count = 1;
			var node = _root;
			while (true) {
				if (v < node.Value) {
					if (v < node.Pivot) {
						if (node.Left != null) {
							node = node.Left;
						} else {
							long p = node.Pivot;
							node.Left = NewNode(v, p - ((p & -p) >> 1), count);
							break;
						}
					} else {
						long tempValue = node.Value;
						int tempCount = node.Count;
						node.Value = v;
						node.Count = count;
						if (node.Right != null) {
							node = node.Right;
							v = tempValue;
							count = tempCount;
						} else {
							long p = node.Pivot;
							node.Right = NewNode(tempValue, p + ((p & -p) >> 1), tempCount);
							break;
						}
					}
				} else if (v > node.Value) {
					if (node.Value < node.Pivot) {
						long tempValue = node.Value;
						int tempCount = node.Count;
						node.Value = v;
						node.Count = count;
						if (node.Left != null) {
							node = node.Left;
							v = tempValue;
							count = tempCount;
						} else {
							long p = node.Pivot;
							node.Left = NewNode(tempValue, p - ((p & -p) >> 1), tempCount);
							break;
						}
					} else {
						if (node.Right != null) {
							node = node.Right;
						} else {
							long p = node.Pivot;
							node.Right = NewNode(v, p + ((p & -p) >> 1), count);
							break;
						}
					}
				} else {
					++node.Count;
					break;
				}
			}

			++_count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Remove(long v) => RemoveCore(v, removesOne: true);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void RemoveCore(long v, Node node = null, Node prev = null, bool removesOne = false)
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

			if (removesOne && node.Count > 1) {
				--node.Count;
				--_count;
				return;
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
				node.Value = leftest.Value;
				node.Count = leftest.Count;
				RemoveCore(node.Value - OFFSET, node.Right, node);
			} else {
				var rightest = RightMost(node.Left);
				node.Value = rightest.Value;
				node.Count = rightest.Count;
				RemoveCore(node.Value - OFFSET, node.Left, node);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long LowerBound(long l) => Next(l - 1);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long UpperBound(long l) => Next(l);

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
		private Node NewNode(long value, long pivot, int count)
		{
			if (_pool.Count > 0) {
				var node = _pool.Pop();
				node.Value = value;
				node.Pivot = pivot;
				node.Count = count;
				node.Left = null;
				node.Right = null;
				return node;
			} else {
				return new Node(value, pivot, count);
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			=> GetEnumerator();
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
				for (int i = 0; i < cur.Count; i++) {
					yield return cur.Value - OFFSET;
				}

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
			public int Count { get; set; }
			public Node Left { get; set; }
			public Node Right { get; set; }
			public Node(long value, long pivot, int count)
			{
				Value = value;
				Pivot = pivot;
				Count = count;
			}
		}
	}

	public class MultiIndexPivotTree : IReadOnlyCollection<long>
	{
		private const int OFFSET = 1;
		private readonly int _height;
		private readonly long _inf;
		private readonly Node _root;
		private readonly Stack<Node> _pool = new Stack<Node>();
		private int _count = 0;

		public MultiIndexPivotTree(long max)
		{
			max += OFFSET;
			int height = 0;
			while (max > 0) {
				++height;
				max >>= 1;
			}

			_height = height;
			_inf = (1L << _height) - OFFSET;
			_root = new Node(1L << height, 1L << height, 1);
		}

		public int Count => _count;
		public long Inf => _inf;
		public (int index, long value) Max => Prev(Inf);
		public (int index, long value) Min => Next(-1);

		public long this[int index] => FindNodeByIndex(index).Value - OFFSET;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Contains(long v) => Next(v - OFFSET).value == v;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(long v)
		{
			v += OFFSET;
			int count = 1;
			var node = _root;
			while (true) {
				if (v < node.Value) {
					if (v < node.Pivot) {
						if (node.Left != null) {
							node = node.Left;
						} else {
							long p = node.Pivot;
							node.Left = NewNode(v, p - ((p & -p) >> 1), count);
							node.Left.Parent = node;
							UpdateSize(node);
							break;
						}
					} else {
						long tempValue = node.Value;
						int tempCount = node.Count;
						node.Value = v;
						node.Count = count;
						if (node.Right != null) {
							node = node.Right;
							v = tempValue;
							count = tempCount;
						} else {
							long p = node.Pivot;
							node.Right = NewNode(tempValue, p + ((p & -p) >> 1), tempCount);
							node.Right.Parent = node;
							UpdateSize(node);
							break;
						}
					}
				} else if (v > node.Value) {
					if (node.Value < node.Pivot) {
						long tempValue = node.Value;
						int tempCount = node.Count;
						node.Value = v;
						node.Count = count;
						if (node.Left != null) {
							node = node.Left;
							v = tempValue;
							count = tempCount;
						} else {
							long p = node.Pivot;
							node.Left = NewNode(tempValue, p - ((p & -p) >> 1), tempCount);
							node.Left.Parent = node;
							UpdateSize(node);
							break;
						}
					} else {
						if (node.Right != null) {
							node = node.Right;
						} else {
							long p = node.Pivot;
							node.Right = NewNode(v, p + ((p & -p) >> 1), count);
							node.Right.Parent = node;
							UpdateSize(node);
							break;
						}
					}
				} else {
					++node.Count;
					UpdateSize(node);
					break;
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
		public void Remove(long v) => RemoveCore(v, removesOne: true);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void RemoveCore(long v, Node node = null, Node prev = null, bool removesOne = false)
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

			if (removesOne && node.Count > 1) {
				--node.Count;
				--_count;
				return;
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
				var leftest = LeftMost(node.Right);
				node.Value = leftest.Value;
				node.Count = leftest.Count;
				RemoveCore(node.Value - OFFSET, node.Right, node);
			} else {
				var rightest = RightMost(node.Left);
				node.Value = rightest.Value;
				node.Count = rightest.Count;
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
			var prev = new Node(0, 0, 1);
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
			var next = new Node(Inf + 2, 0, 1);
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
				if (currentIndex < index && index < currentIndex + current.Count) {
					break;
				}

				if (currentIndex > index) {
					current = current.Left;
					if (current is null) {
						break;
					}

					currentIndex -= CountOf(current.Right) + 1;
					currentIndex -= current.Count - 1;
				} else {
					var prev = current;
					current = current.Right;
					if (current is null) {
						break;
					}

					currentIndex += prev.Count - 1;
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
				node.Size = node.Count + CountOf(node.Left) + CountOf(node.Right);
				node = node.Parent;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Node NewNode(long value, long pivot, int count)
		{
			if (_pool.Count > 0) {
				var node = _pool.Pop();
				node.Value = value;
				node.Pivot = pivot;
				node.Count = count;
				node.Size = count;
				node.Left = null;
				node.Right = null;
				return node;
			} else {
				return new Node(value, pivot, count);
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			=> GetEnumerator();
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
				if (cur.Value != _inf + OFFSET) {
					for (int i = 0; i < cur.Count; i++) {
						yield return cur.Value - OFFSET;
					}
				}

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
			public int Count { get; set; }
			public int Size { get; set; }
			public Node Parent { get; set; }
			public Node Left { get; set; }
			public Node Right { get; set; }
			public Node(long value, long pivot, int count)
			{
				Value = value;
				Pivot = pivot;
				Count = count;
				Size = count;
			}
		}
	}
}

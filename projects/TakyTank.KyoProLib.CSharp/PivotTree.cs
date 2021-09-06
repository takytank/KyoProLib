using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class PivotTree
	{
		private readonly int _height;
		private readonly Node _root;
		private int _count = 0;

		public PivotTree(int height)
		{
			_height = height;
			_root = new Node(1L << height, 1L << height);
		}

		public int Count => _count;
		public long Max => Prev((1L << _height) - 1);
		public long Min => Next(-1);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Contains(long v) => Next(v - 1) == v;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(long v, bool exclusive = false)
		{
			++v;
			var node = _root;
			while (true) {
				if (v == node.Value) {
					if (exclusive) {
						Remove(v - 1);
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
							node.Left = new Node(min, p - (p & -p) / 2);
							break;
						}
					} else {
						node.Value = min;
						if (node.Right != null) {
							node = node.Right;
							v = max;
						} else {
							long p = node.Pivot;
							node.Right = new Node(max, p + (p & -p) / 2);
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
			++v;
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

				--_count;
			} else if (node.Right != null) {
				node.Value = LeftMost(node.Right).Value;
				RemoveCore(node.Value - 1, node.Right, node);
			} else {
				node.Value = RightMost(node.Left).Value;
				RemoveCore(node.Value - 1, node.Left, node);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long FindL(long v) => Prev(v);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long Prev(long v)
		{
			++v;
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
						return prev - 1;
					}
				} else {
					prev = node.Value;
					if (node.Right != null) {
						node = node.Right;
					} else {
						return prev - 1;
					}
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long FindR(long v) => Next(v);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long Next(long v)
		{
			++v;
			var node = _root;
			long prev = 0;
			if (node.Value > v) {
				prev = node.Value;
			}

			while (true) {
				if (v < node.Value) {
					prev = node.Value;
					if (node.Left != null) {
						node = node.Left;
					} else {
						return prev - 1;
					}
				} else {
					if (node.Right != null) {
						node = node.Right;
					} else {
						return prev - 1;
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
}

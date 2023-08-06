using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V11
{
	public class ReorderArray
	{
		private readonly int _n;
		private readonly SortedSet<int> _keys = new SortedSet<int>();
		private readonly Dictionary<int, SplitBinaryTrie> _tries = new Dictionary<int, SplitBinaryTrie>();

		public ReorderArray(Span<long> array)
		{
			_n = array.Length;
			for (int i = 0; i < _n; i++) {
				_keys.Add(i);
				_tries[i] = new SplitBinaryTrie(array[i]);
			}
		}

		public IEnumerable<long> ForEach()
		{
			foreach (var key in _keys) {
				foreach (var value in _tries[key].ForeEach()) {
					yield return value;
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Sort(int l, int r, bool rev)
		{
			var lv = GetMaxLessThanOrEqualTo(l);
			if (lv != l) {
				var (left, right) = SplitTrie(_tries[lv], l - lv);
				Add(lv, left);
				Add(l, right);
			}

			var rv = GetMaxLessThanOrEqualTo(r);
			if (rv != r) {
				var (left, right) = SplitTrie(_tries[rv], r - rv);
				Add(rv, left);
				Add(r, right);
			}

			var merged = new SplitBinaryTrie();
			var range = _keys.GetViewBetween(l, r - 1).ToArray();
			foreach (var i in range) {
				merged = SplitBinaryTrie.Merge(merged, _tries[i]);
			}

			foreach (var i in range) {
				Remove(i);
			}

			merged.Reverses = rev;
			if (merged.Empty() == false) {
				Add(l, merged);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Add(int v, SplitBinaryTrie trie)
		{
			_keys.Add(v);
			_tries[v] = trie;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Remove(int v)
		{
			_keys.Remove(v);
			_tries.Remove(v);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private (SplitBinaryTrie left, SplitBinaryTrie right) SplitTrie(SplitBinaryTrie trie, int idnex)
		{
			bool rev = trie.Reverses;
			if (rev) {
				idnex = trie.Count - idnex;
			}

			var (left, right) = trie.Split(idnex);
			if (rev) {
				left.Reverses = true;
				right.Reverses = true;
				(left, right) = (right, left);
			}

			return (left, right);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int GetMaxLessThanOrEqualTo(int value) => _keys.GetViewBetween(0, value).Max;

		public class SplitBinaryTrie
		{
			private readonly int _bitLength = 18;
			private readonly Stack<Node> _pool = new Stack<Node>();
			private Node _root = null;

			public bool Reverses { get; set; } = false;
			public int Count => SizeOf(_root);

			public SplitBinaryTrie() { }
			public SplitBinaryTrie(long value) => Insert(value);
			public SplitBinaryTrie(Span<long> values)
			{
				foreach (var value in values) {
					Insert(value);
				}
			}

			private SplitBinaryTrie(Node node) => _root = node;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool Empty() => Count == 0;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Insert(long value) => _root = InsertCore(_root, value);

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public (SplitBinaryTrie left, SplitBinaryTrie right) Split(int index)
			{
				var (left, right) = SplitCore(_root, index);
				return (new SplitBinaryTrie(left), new SplitBinaryTrie(right));
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static SplitBinaryTrie Merge(SplitBinaryTrie lhs, SplitBinaryTrie rhs)
			{
				if (lhs.Count < rhs.Count) {
					(lhs, rhs) = (rhs, lhs);
				}

				return new SplitBinaryTrie(MergeCore(lhs._root, rhs._root, lhs._bitLength));
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public IEnumerable<long> ForeEach()
			{
				var target = new List<long>();
				AddToCore(_root, 0, target);
				if (Reverses) {
					target.Reverse();
				}

				foreach (var value in target) {
					yield return value;
				}
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static int SizeOf(Node node) => (node == null) ? 0 : node.Count;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private Node InsertCore(Node node, long value) => InsertCore(node, value, _bitLength - 1);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private Node InsertCore(Node node, long value, int bit)
			{
				if (node == null) {
					node = NewNode(0);
				}

				node.Count += 1;
				if (bit < 0) {
					return node;
				}

				long f = (value >> bit) & 1;
				node.Child[f] = InsertCore(node.Child[f], value, bit - 1);
				return node;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private void AddToCore(Node node, long value, List<long> target)
				=> AddToCore(node, value, target, _bitLength - 1);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private void AddToCore(Node node, long value, List<long> target, int bit)
			{
				if (node == null) {
					return;
				}

				if (bit < 0) {
					target.Add(value);
					return;
				}

				if (node.Child[0] != null) {
					AddToCore(node.Child[0], value, target, bit - 1);
				}

				if (node.Child[1] != null) {
					AddToCore(node.Child[1], value | (1L << bit), target, bit - 1);
				}
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private (Node left, Node right) SplitCore(Node node, int index)
				=> SplitCore(node, index, _bitLength - 1);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private (Node left, Node right) SplitCore(Node node, int index, int bit)
			{
				if (node == null) {
					return (null, null);
				}

				if (bit < 0) {
					if (index <= 0) {
						return (null, node);
					} else {
						return (node, null);
					}
				}

				int leftSize = SizeOf(node.Child[0]);
				if (leftSize <= index) {
					var (l, r) = SplitCore(node.Child[1], index - leftSize, bit - 1);
					var left = NewNode(SizeOf(node.Child[0]) + SizeOf(l), node.Child[0], l);
					var right = NewNode(SizeOf(r), null, r);
					return (left, right);
				} else {
					var (l, r) = SplitCore(node.Child[0], index, bit - 1);
					var left = NewNode(SizeOf(l), l, null);
					var right = NewNode(SizeOf(r) + SizeOf(node.Child[1]), r, node.Child[1]);
					return (left, right);
				}
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static Node MergeCore(Node left, Node right, int bit)
			{
				if (right == null) {
					return left;
				}

				if (left == null) {
					(left, _) = (right, left);
					return left;
				}

				int msize = SizeOf(left) + SizeOf(right);
				Node lc = null;
				Node rc = null;
				if (bit >= 0) {
					lc = MergeCore(left.Child[0], right.Child[0], bit - 1);
					rc = MergeCore(left.Child[1], right.Child[1], bit - 1);
				}

				left.Child[0] = lc;
				left.Child[1] = rc;
				left.Count = msize;
				return left;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private Node NewNode(int leaf_count, Node l = null, Node r = null)
			{
				if (_pool.Count > 0) {
					var node = _pool.Pop();
					node.Count = leaf_count;
					node.Child[0] = l;
					node.Child[1] = r;
					return node;
				} else {
					return new Node(leaf_count, l, r);
				}
			}

			private class Node
			{
				public int Count { get; set; } = 0;
				public Node[] Child { get; set; } = new Node[] { null, null };
				public Node(int n, Node l = null, Node r = null)
				{
					Count = n;
					Child[0] = l;
					Child[1] = r;
				}
			}
		}
	}
}

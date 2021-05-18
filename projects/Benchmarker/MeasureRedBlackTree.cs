using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace Benchmarker
{
	public class MeasureRedBlackTree
	{
		private readonly long[] src_;
		public MeasureRedBlackTree()
		{
			int n = 500000;
			var rnd = new Random();
			src_ = new long[n];
			for (int i = 0; i < n; i++) {
				src_[i] = (long)rnd.Next() * rnd.Next();
			}
		}

		[Benchmark]
		public void Add1()
		{
			var tree = new RedBlackTree3<long>();
			for (int i = 0; i < src_.Length; i++) {
				tree.Add(src_[i]);
			}
		}

		[Benchmark]
		public void Add2()
		{
			var tree = new RedBlackTree2<long>();
			for (int i = 0; i < src_.Length; i++) {
				tree.Add(src_[i]);
			}
		}

		[Benchmark]
		public void Add3()
		{
			var tree = new RedBlackTree3<long>();
			for (int i = 0; i < src_.Length; i++) {
				tree.Add(src_[i]);
			}
		}

		[Benchmark]
		public void RemoveAt1()
		{
			var tree = new RedBlackTree3<long>();
			for (int i = 0; i < src_.Length; i++) {
				tree.Add(src_[i]);
			}

			var rnd = new Random();
			for (int i = 0; i < src_.Length / 2; i++) {
				tree.RemoveAt(rnd.Next(0, src_.Length - i - 1));
			}
		}

		[Benchmark]
		public void RemoveAt2()
		{
			var tree = new RedBlackTree2<long>();
			for (int i = 0; i < src_.Length; i++) {
				tree.Add(src_[i]);
			}

			var rnd = new Random();
			for (int i = 0; i < src_.Length / 2; i++) {
				tree.RemoveAt(rnd.Next(0, src_.Length - i - 1));
			}
		}

		[Benchmark]
		public void RemoveAt3()
		{
			var tree = new RedBlackTree3<long>();
			for (int i = 0; i < src_.Length; i++) {
				tree.Add(src_[i]);
			}

			var rnd = new Random();
			for (int i = 0; i < src_.Length / 2; i++) {
				tree.RemoveAt(rnd.Next(0, src_.Length - i - 1));
			}
		}

		[Benchmark]
		public void FindByIndex1()
		{
			var tree = new RedBlackTree3<long>();
			for (int i = 0; i < src_.Length; i++) {
				tree.Add(src_[i]);
			}

			var dst = new long[src_.Length];
			var rnd = new Random();
			for (int i = 0; i < src_.Length; i++) {
				dst[i] = tree.FindByIndex(rnd.Next(0, src_.Length - 1)).Item;
			}
		}

		[Benchmark]
		public void FindByIndex2()
		{
			var tree = new RedBlackTree2<long>();
			for (int i = 0; i < src_.Length; i++) {
				tree.Add(src_[i]);
			}

			var dst = new long[src_.Length];
			var rnd = new Random();
			for (int i = 0; i < src_.Length; i++) {
				dst[i] = tree.Find(rnd.Next(0, src_.Length - 1));
			}
		}

		[Benchmark]
		public void FindByIndex3()
		{
			var tree = new RedBlackTree3<long>();
			for (int i = 0; i < src_.Length; i++) {
				tree.Add(src_[i]);
			}

			var dst = new long[src_.Length];
			var rnd = new Random();
			for (int i = 0; i < src_.Length; i++) {
				dst[i] = tree.FindByIndex(rnd.Next(0, src_.Length - 1)).Item;
			}
		}

		[Benchmark]
		public void BinarySeach1()
		{
			var tree = new RedBlackTree3<long>();
			for (int i = 0; i < src_.Length; i++) {
				tree.Add(src_[i]);
			}

			var dst = new int[src_.Length];
			for (int i = 0; i < src_.Length; i++) {
				dst[i] = tree.BinarySearch(src_[i], true).index;
			}
		}

		[Benchmark]
		public void BinarySeach2()
		{
			var tree = new RedBlackTree2<long>();
			for (int i = 0; i < src_.Length; i++) {
				tree.Add(src_[i]);
			}

			var dst = new int[src_.Length];
			for (int i = 0; i < src_.Length; i++) {
				dst[i] = tree.BinarySearch(src_[i], true).index;
			}
		}

		[Benchmark]
		public void BinarySeach3()
		{
			var tree = new RedBlackTree3<long>();
			for (int i = 0; i < src_.Length; i++) {
				tree.Add(src_[i]);
			}

			var dst = new int[src_.Length];
			for (int i = 0; i < src_.Length; i++) {
				dst[i] = tree.BinarySearch(src_[i], true).index;
			}
		}

		[Benchmark]
		public (int count, int num) Enumerate1()
		{
			var tree = new RedBlackTree3<long>();
			for (int i = 0; i < src_.Length; i++) {
				tree.Add(src_[i]);
			}

			int count = 0;
			var dst = new HashSet<long>();
			foreach (var item in tree) {
				dst.Add(item);
				++count;
			}

			int num = 0;
			foreach (var item in src_) {
				if (dst.Contains(item)) {
					++num;
				}
			}

			return (count, num);
		}

		[Benchmark]
		public (int count, int num) Enumerate2()
		{
			var tree = new RedBlackTree2<long>();
			for (int i = 0; i < src_.Length; i++) {
				tree.Add(src_[i]);
			}

			int count = 0;
			var dst = new HashSet<long>();
			foreach (var item in tree) {
				dst.Add(item);
				++count;
			}

			int num = 0;
			foreach (var item in src_) {
				if (dst.Contains(item)) {
					++num;
				}
			}

			return (count, num);
		}

		[Benchmark]
		public (int count, int num) Enumerate3()
		{
			var tree = new RedBlackTree3<long>();
			for (int i = 0; i < src_.Length; i++) {
				tree.Add(src_[i]);
			}

			int count = 0;
			var dst = new HashSet<long>();
			foreach (var item in tree) {
				dst.Add(item);
				++count;
			}

			int num = 0;
			foreach (var item in src_) {
				if (dst.Contains(item)) {
					++num;
				}
			}

			return (count, num);
		}
	}

	public class RedBlackTree2<T> : ICollection<T>, IReadOnlyCollection<T>
	{
		private readonly Comparison<T> comparer_;
		private readonly bool isMulti_;
		private Node root_;

		public RedBlackTree2(bool isMulti = false)
			: this(Comparer<T>.Default, isMulti) { }

		public RedBlackTree2(IEnumerable<T> collection, bool isMulti = false)
			: this(collection, Comparer<T>.Default, isMulti) { }

		public RedBlackTree2(IComparer<T> comparer, bool isMulti = false)
			: this(comparer.Compare, isMulti) { }
		public RedBlackTree2(Comparison<T> comparer, bool isMulti = false)
		{
			comparer_ = comparer;
			isMulti_ = isMulti;
		}

		public RedBlackTree2(IEnumerable<T> collection, IComparer<T> comparer, bool isMulti = false)
			: this(collection, comparer.Compare, isMulti) { }
		public RedBlackTree2(IEnumerable<T> collection, Comparison<T> comparer, bool isMulti = false)
		{
			comparer_ = comparer;
			isMulti_ = isMulti;
			var arr = InitializeArray(collection);
			root_ = ConstructRootFromSortedArray(arr, 0, arr.Length - 1, null);
		}

		public T this[int index] => Find(index);

		public int Count => CountOf(root_);
		public T Min
		{
			get
			{
				if (root_ is null) {
					return default;
				}

				var cur = root_;
				while (cur.Left is null == false) {
					cur = cur.Left;
				}

				return cur.Item;
			}
		}

		public T Max
		{
			get
			{
				if (root_ is null) {
					return default;
				}

				var cur = root_;
				while (cur.Right is null == false) {
					cur = cur.Right;
				}

				return cur.Item;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Contains(T item) => FindNode(item) is null == false;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int IndexOf(T item) => Index(FindNode(item));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Find(int index)
		{
			var node = FindNodeByIndex(index);
			return node is null == false ? node.Item : default;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(T item)
		{
			if (root_ is null) {
				root_ = new Node(item, false);
				return;
			}

			Node current = root_;
			Node parent = null;
			Node grandParent = null;
			Node greatGrandParent = null;
			int order = 0;
			while (current is null == false) {
				order = comparer_(item, current.Item);
				if (order == 0 && isMulti_ == false) {
					root_.IsRed = false;
					return;
				}

				if (current.Is4Node()) {
					current.Split4Node();
					if (Node.IsNonNullRed(parent) == true) {
						InsertionBalance(current, ref parent, grandParent, greatGrandParent);
					}
				}

				greatGrandParent = grandParent;
				grandParent = parent;
				parent = current;
				current = (order < 0) ? current.Left : current.Right;
			}

			Node node = new Node(item, true);
			if (order >= 0) {
				parent.Right = node;
			} else {
				parent.Left = node;
			}

			if (parent.IsRed) {
				InsertionBalance(node, ref parent, grandParent, greatGrandParent);
			}

			root_.IsRed = false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool RemoveAt(int index) => Remove(FindNodeByIndex(index).Item);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Remove(T item)
		{
			if (root_ is null) {
				return false;
			}

			Node current = root_;
			Node parent = null;
			Node grandParent = null;
			Node match = null;
			Node parentOfMatch = null;
			bool foundMatch = false;
			while (current is null == false) {
				if (current.Is2Node()) {
					if (parent is null) {
						current.IsRed = true;
					} else {
						Node sibling = parent.GetSibling(current);
						if (sibling.IsRed) {
							if (parent.Right == sibling) {
								parent.RotateLeft();
							} else {
								parent.RotateRight();
							}

							parent.IsRed = true;
							sibling.IsRed = false;
							ReplaceChildOrRoot(grandParent, parent, sibling);
							grandParent = sibling;
							if (parent == match) {
								parentOfMatch = sibling;
							}

							sibling = (parent.Left == current) ? parent.Right : parent.Left;
						}

						if (sibling.Is2Node()) {
							parent.Merge2Nodes();
						} else {
							TreeRotation rotation = parent.GetRotation(current, sibling);
							Node newGrandParent = parent.Rotate(rotation);
							newGrandParent.IsRed = parent.IsRed;
							parent.IsRed = false;
							current.IsRed = true;
							ReplaceChildOrRoot(grandParent, parent, newGrandParent);
							if (parent == match) {
								parentOfMatch = newGrandParent;
							}
						}
					}
				}

				int order = foundMatch ? -1 : comparer_(item, current.Item);
				if (order == 0) {
					foundMatch = true;
					match = current;
					parentOfMatch = parent;
				}

				grandParent = parent;
				parent = current;
				current = order < 0 ? current.Left : current.Right;
			}

			if (match is null == false) {
				ReplaceNode(match, parentOfMatch, parent, grandParent);
			}

			if (root_ is null == false) {
				root_.IsRed = false;
			}

			return foundMatch;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			root_ = null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(T[] array, int arrayIndex)
		{
			foreach (var item in this) {
				array[arrayIndex++] = item;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (T value, int index) LowerBound(T item) => BinarySearch(item, true);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (T value, int index) UpperBound(T item) => BinarySearch(item, false);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (T value, int index) BinarySearch(T item, bool isLowerBound)
		{
			Node right = null;
			int ri = -1;
			Node current = root_;
			if (current is null) {
				return (default, ri);
			}

			ri = 0;
			while (current is null == false) {
				var order = comparer_(item, current.Item);
				if (order < 0 || (isLowerBound && order == 0)) {
					right = current;
					current = current.Left;
				} else {
					ri += CountOf(current.Left) + 1;
					current = current.Right;
				}
			}

			return (right.Item, ri);
		}

		bool ICollection<T>.IsReadOnly => false;
		public IEnumerable<T> Reversed()
		{
			var e = new Enumerator(this, true, null);
			while (e.MoveNext()) {
				yield return e.Current;
			}
		}
		public IEnumerable<T> Enumerate(Node from) => Enumerate(from, false);
		public IEnumerable<T> Enumerate(Node from, bool reverse)
		{
			if (from is null) {
				yield break;
			}

			var e = new Enumerator(this, reverse, from);
			while (e.MoveNext()) {
				yield return e.Current;
			}
		}
		public Enumerator GetEnumerator() => new Enumerator(this);
		IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator(this);
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => new Enumerator(this);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int CountOf(Node node) => node is null ? 0 : node.Size;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Node FindNode(T item)
		{
			Node current = root_;
			while (current is null == false) {
				int cmp = comparer_(item, current.Item);
				if (cmp == 0) {
					return current;
				}

				current = cmp < 0 ? current.Left : current.Right;
			}

			return null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int Index(Node node)
		{
			if (node is null) {
				return -1;
			}

			var ret = CountOf(node.Left);
			Node prev = node;
			node = node.Parent;
			while (prev != root_) {
				if (node.Left != prev) {
					ret += CountOf(node.Left) + 1;
				}

				prev = node;
				node = node.Parent;
			}

			return ret;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Node FindNodeByIndex(int index)
		{
			var current = root_;
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
		private T[] InitializeArray(IEnumerable<T> collection)
		{
			T[] array;
			if (isMulti_) {
				array = collection.ToArray();
				Array.Sort(array, comparer_);
			} else {
				var list = new List<T>(collection);
				list.Sort(comparer_);
				for (int i = list.Count - 1; i > 0; i--) {
					if (comparer_(list[i - 1], list[i]) == 0) {
						list.RemoveAt(i);
					}
				}

				array = list.ToArray();
			}

			return array;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static Node ConstructRootFromSortedArray(T[] arr, int startIndex, int endIndex, Node redNode)
		{
			int size = endIndex - startIndex + 1;
			Node root;
			switch (size) {
				case 0:
					return null;

				case 1:
					root = new Node(arr[startIndex], false);
					if (redNode is null == false) {
						root.Left = redNode;
					}

					break;

				case 2:
					root = new Node(arr[startIndex], false) { Right = new Node(arr[endIndex], true) };
					if (redNode is null == false) {
						root.Left = redNode;
					}

					break;

				case 3:
					root = new Node(arr[startIndex + 1], false) {
						Left = new Node(arr[startIndex], false),
						Right = new Node(arr[endIndex], false)
					};

					if (redNode is null == false) {
						root.Left.Left = redNode;
					}

					break;

				default:
					int midpt = ((startIndex + endIndex) / 2);
					root = new Node(arr[midpt], false) {
						Left = ConstructRootFromSortedArray(arr, startIndex, midpt - 1, redNode),
						Right = size % 2 == 0
							? ConstructRootFromSortedArray(arr, midpt + 2, endIndex, new Node(arr[midpt + 1], true))
							: ConstructRootFromSortedArray(arr, midpt + 1, endIndex, null)
					};

					break;
			}

			return root;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ReplaceNode(Node match, Node parentOfMatch, Node succesor, Node parentOfSuccesor)
		{
			if (succesor == match) {
				succesor = match.Left;
			} else {
				if (succesor.Right is null == false) {
					succesor.Right.IsRed = false;
				}

				if (parentOfSuccesor != match) {
					parentOfSuccesor.Left = succesor.Right;
					succesor.Right = match.Right;
				}

				succesor.Left = match.Left;
			}

			if (succesor is null == false) {
				succesor.IsRed = match.IsRed;
			}

			ReplaceChildOrRoot(parentOfMatch, match, succesor);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void InsertionBalance(Node current, ref Node parent, Node grandParent, Node greatGrandParent)
		{
			bool parentIsOnRight = grandParent.Right == parent;
			bool currentIsOnRight = parent.Right == current;
			Node newChildOfGreatGrandParent;
			if (parentIsOnRight == currentIsOnRight) {
				newChildOfGreatGrandParent = currentIsOnRight
					? grandParent.RotateLeft()
					: grandParent.RotateRight();
			} else {
				newChildOfGreatGrandParent = currentIsOnRight
					? grandParent.RotateLeftRight()
					: grandParent.RotateRightLeft();
				parent = greatGrandParent;
			}

			grandParent.IsRed = true;
			newChildOfGreatGrandParent.IsRed = false;
			ReplaceChildOrRoot(greatGrandParent, grandParent, newChildOfGreatGrandParent);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ReplaceChildOrRoot(Node parent, Node child, Node newChild)
		{
			if (parent is null == false) {
				if (parent.Left == child) {
					parent.Left = newChild;
				} else {
					parent.Right = newChild;
				}
			} else {
				root_ = newChild;
			}
		}



		public class Node
		{
			public bool IsRed { get; set; }
			public T Item { get; set; }
			public Node Parent { get; private set; }

			private Node _Left;
			public Node Left
			{
				get => _Left;
				set
				{
					_Left = value;
					Update(value);
				}
			}

			private Node _Right;
			public Node Right
			{
				get => _Right;
				set
				{
					_Right = value;
					Update(value);
				}
			}
			public int Size { get; private set; } = 1;
			public Node(T item, bool isRed)
			{
				Item = item;
				IsRed = isRed;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Update(Node child)
			{
				if (child is null == false) {
					child.Parent = this;
				}

				for (var cur = this; cur is null == false; cur = cur.Parent) {
					if (!cur.UpdateSize()) {
						break;
					}

					if (cur.Parent is null == false
						&& cur.Parent.Left != cur
						&& cur.Parent.Right != cur) {
						cur.Parent = null;
						return;
					}
				}
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool UpdateSize()
			{
				var oldsize = Size;
				var size = 1;
				if (Left is null == false) {
					size += Left.Size;
				}
				if (Right is null == false) {
					size += Right.Size;
				}

				Size = size;
				return oldsize != size;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool IsNonNullRed(Node node) => node is null == false && node.IsRed;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool IsNonNullBlack(Node node) => node is null == false && node.IsRed == false;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool IsNullOrBlack(Node node) => node is null || node.IsRed == false;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool Is2Node() => IsRed == false && IsNullOrBlack(Left) && IsNullOrBlack(Right);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool Is4Node() => IsNonNullRed(Left) && IsNonNullRed(Right);

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Merge2Nodes()
			{
				IsRed = false;
				Left.IsRed = true;
				Right.IsRed = true;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Split4Node()
			{
				IsRed = true;
				Left.IsRed = false;
				Right.IsRed = false;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public TreeRotation GetRotation(Node current, Node sibling)
			{
				if (IsNonNullRed(sibling.Left)) {
					if (Left == current) {
						return TreeRotation.RightLeft;
					}

					return TreeRotation.Right;
				} else {
					if (Left == current) {
						return TreeRotation.Left;
					}

					return TreeRotation.LeftRight;
				}
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Node GetSibling(Node node)
			{
				return Left == node ? Right : Left;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Node Rotate(TreeRotation rotation)
			{
				switch (rotation) {
					default:
					case TreeRotation.Right:
						Left.Left.IsRed = false;
						return RotateRight();

					case TreeRotation.Left:
						Right.Right.IsRed = false;
						return RotateLeft();

					case TreeRotation.RightLeft:
						return RotateRightLeft();

					case TreeRotation.LeftRight:
						return RotateLeftRight();
				}
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Node RotateLeft()
			{
				Node child = Right;
				Right = child.Left;
				child.Left = this;
				return child;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Node RotateLeftRight()
			{
				Node child = Left;
				Node grandChild = child.Right;
				Left = grandChild.Right;
				grandChild.Right = this;
				child.Right = grandChild.Left;
				grandChild.Left = child;
				return grandChild;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Node RotateRight()
			{
				Node child = Left;
				Left = child.Right;
				child.Right = this;
				return child;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Node RotateRightLeft()
			{
				Node child = Right;
				Node grandChild = child.Left;
				Right = grandChild.Left;
				grandChild.Left = this;
				child.Left = grandChild.Right;
				grandChild.Right = child;
				return grandChild;
			}

			public override string ToString() => $"size = {Size}, item = {Item}";
		}

		public enum TreeRotation : byte
		{
			Left = 1,
			Right = 2,
			RightLeft = 3,
			LeftRight = 4,
		}

		public struct Enumerator : IEnumerator<T>
		{
			private readonly RedBlackTree2<T> tree_;
			private readonly Stack<Node> stack_;
			private readonly bool reverse_;
			private Node current_;

			internal Enumerator(RedBlackTree2<T> tree)
				: this(tree, false, tree.root_) { }

			internal Enumerator(RedBlackTree2<T> set, bool reverse, Node startNode)
			{
				tree_ = set;
				stack_ = new Stack<Node>(2 * Log2(tree_.Count + 1));
				current_ = null;
				reverse_ = reverse;
				Intialize(startNode);
			}

			private void Intialize(Node startNode)
			{
				current_ = null;
				Node node = startNode;
				Node next;
				while (node != null) {
					next = (reverse_ ? node.Right : node.Left);
					stack_.Push(node);
					node = next;
				}
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static int Log2(int num) => num == 0 ? 0 : BitOperations.Log2((uint)num) + 1;
			object System.Collections.IEnumerator.Current => Current;
			public T Current => current_ is null ? default : current_.Item;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool MoveNext()
			{
				if (stack_.Count == 0) {
					current_ = null;
					return false;
				}

				current_ = stack_.Pop();
				var node = reverse_ ? current_.Left : current_.Right;
				while (node is null == false) {
					var next = reverse_ ? node.Right : node.Left;
					stack_.Push(node);
					node = next;
				}

				return true;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Reset() => throw new NotSupportedException();

			public void Dispose() { }
		}
	}

	public class RedBlackTree3<T> : ICollection<T>, IReadOnlyCollection<T>
	{
		private readonly Comparison<T> comparer_;
		private readonly bool isMulti_;
		private Node root_;

		public RedBlackTree3(bool isMulti = false)
			: this(Comparer<T>.Default, isMulti) { }

		public RedBlackTree3(IEnumerable<T> collection, bool isMulti = false)
			: this(collection, Comparer<T>.Default, isMulti) { }

		public RedBlackTree3(IComparer<T> comparer, bool isMulti = false)
			: this(comparer.Compare, isMulti) { }
		public RedBlackTree3(Comparison<T> comparer, bool isMulti = false)
		{
			comparer_ = comparer;
			isMulti_ = isMulti;
		}

		public RedBlackTree3(IEnumerable<T> collection, IComparer<T> comparer, bool isMulti = false)
			: this(collection, comparer.Compare, isMulti) { }
		public RedBlackTree3(IEnumerable<T> collection, Comparison<T> comparer, bool isMulti = false)
		{
			comparer_ = comparer;
			isMulti_ = isMulti;
			var arr = InitializeArray(collection);
			root_ = ConstructRootFromSortedArray(arr, 0, arr.Length - 1, null);
		}

		public int Count => CountOf(root_);
		public T Min
		{
			get
			{
				if (root_ is null) {
					return default;
				}

				var cur = root_;
				while (cur.Left is null == false) {
					cur = cur.Left;
				}

				return cur.Item;
			}
		}

		public T Max
		{
			get
			{
				if (root_ is null) {
					return default;
				}

				var cur = root_;
				while (cur.Right is null == false) {
					cur = cur.Right;
				}

				return cur.Item;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Contains(T item) => FindNode(item) is null == false;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Node FindNode(T item)
		{
			Node current = root_;
			while (current is null == false) {
				int cmp = comparer_(item, current.Item);
				if (cmp == 0) {
					return current;
				}

				current = cmp < 0 ? current.Left : current.Right;
			}

			return null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Index(Node node)
		{
			var ret = CountOf(node.Left);
			Node prev = node;
			node = node.Parent;
			while (prev != root_) {
				if (node.Left != prev) {
					ret += CountOf(node.Left) + 1;
				}

				prev = node;
				node = node.Parent;
			}

			return ret;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Node FindByIndex(int index)
		{
			var current = root_;
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
		public void Add(T item)
		{
			if (root_ is null) {
				root_ = new Node(item, false);
				return;
			}

			Node current = root_;
			Node parent = null;
			Node grandParent = null;
			Node greatGrandParent = null;
			int order = 0;
			while (current is null == false) {
				order = comparer_(item, current.Item);
				if (order == 0 && isMulti_ == false) {
					root_.IsRed = false;
					return;
				}

				if (current.Is4Node()) {
					current.Split4Node();
					if (Node.IsNonNullRed(parent) == true) {
						InsertionBalance(current, ref parent, grandParent, greatGrandParent);
					}
				}

				greatGrandParent = grandParent;
				grandParent = parent;
				parent = current;
				current = (order < 0) ? current.Left : current.Right;
			}

			Node node = new Node(item, true);
			if (order >= 0) {
				parent.Right = node;
			} else {
				parent.Left = node;
			}

			if (parent.IsRed) {
				InsertionBalance(node, ref parent, grandParent, greatGrandParent);
			}

			root_.IsRed = false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool RemoveAt(int index) => Remove(FindByIndex(index).Item);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Remove(T item)
		{
			if (root_ is null) {
				return false;
			}

			Node current = root_;
			Node parent = null;
			Node grandParent = null;
			Node match = null;
			Node parentOfMatch = null;
			bool foundMatch = false;
			while (current is null == false) {
				if (current.Is2Node()) {
					if (parent is null) {
						current.IsRed = true;
					} else {
						Node sibling = parent.GetSibling(current);
						if (sibling.IsRed) {
							if (parent.Right == sibling) {
								parent.RotateLeft();
							} else {
								parent.RotateRight();
							}

							parent.IsRed = true;
							sibling.IsRed = false;
							ReplaceChildOrRoot(grandParent, parent, sibling);
							grandParent = sibling;
							if (parent == match) {
								parentOfMatch = sibling;
							}

							sibling = (parent.Left == current) ? parent.Right : parent.Left;
						}

						if (sibling.Is2Node()) {
							parent.Merge2Nodes();
						} else {
							TreeRotation rotation = parent.GetRotation(current, sibling);
							Node newGrandParent = parent.Rotate(rotation);
							newGrandParent.IsRed = parent.IsRed;
							parent.IsRed = false;
							current.IsRed = true;
							ReplaceChildOrRoot(grandParent, parent, newGrandParent);
							if (parent == match) {
								parentOfMatch = newGrandParent;
							}
						}
					}
				}

				int order = foundMatch ? -1 : comparer_(item, current.Item);
				if (order == 0) {
					foundMatch = true;
					match = current;
					parentOfMatch = parent;
				}

				grandParent = parent;
				parent = current;
				current = order < 0 ? current.Left : current.Right;
			}

			if (match is null == false) {
				ReplaceNode(match, parentOfMatch, parent, grandParent);
			}

			if (root_ is null == false) {
				root_.IsRed = false;
			}

			return foundMatch;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			root_ = null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(T[] array, int arrayIndex)
		{
			foreach (var item in this) {
				array[arrayIndex++] = item;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (Node node, int index) BinarySearch(T item, bool isLowerBound)
		{
			Node right = null;
			int ri = -1;
			Node current = root_;
			if (current is null) {
				return (right, ri);
			}

			ri = 0;
			while (current is null == false) {
				var order = comparer_(item, current.Item);
				if (order < 0 || (isLowerBound && order == 0)) {
					right = current;
					current = current.Left;
				} else {
					ri += CountOf(current.Left) + 1;
					current = current.Right;
				}
			}

			return (right, ri);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Node FindNodeLowerBound(T item) => BinarySearch(item, true).node;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Node FindNodeUpperBound(T item) => BinarySearch(item, false).node;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T LowerBoundItem(T item) => BinarySearch(item, true).node.Item;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T UpperBoundItem(T item) => BinarySearch(item, false).node.Item;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int LowerBoundIndex(T item) => BinarySearch(item, true).index;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int UpperBoundIndex(T item) => BinarySearch(item, false).index;

		bool ICollection<T>.IsReadOnly => false;
		public IEnumerable<T> Reversed()
		{
			var e = new Enumerator(this, true, null);
			while (e.MoveNext()) {
				yield return e.Current;
			}
		}
		public IEnumerable<T> Enumerate(Node from) => Enumerate(from, false);
		public IEnumerable<T> Enumerate(Node from, bool reverse)
		{
			if (from is null) {
				yield break;
			}

			var e = new Enumerator(this, reverse, from);
			while (e.MoveNext()) {
				yield return e.Current;
			}
		}
		public Enumerator GetEnumerator() => new Enumerator(this);
		IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator(this);
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => new Enumerator(this);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int CountOf(Node node) => node is null ? 0 : node.Size;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private T[] InitializeArray(IEnumerable<T> collection)
		{
			T[] array;
			if (isMulti_) {
				array = collection.ToArray();
				Array.Sort(array, comparer_);
			} else {
				var list = new List<T>(collection);
				list.Sort(comparer_);
				for (int i = list.Count - 1; i > 0; i--) {
					if (comparer_(list[i - 1], list[i]) == 0) {
						list.RemoveAt(i);
					}
				}

				array = list.ToArray();
			}

			return array;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static Node ConstructRootFromSortedArray(T[] arr, int startIndex, int endIndex, Node redNode)
		{
			int size = endIndex - startIndex + 1;
			Node root;
			switch (size) {
				case 0:
					return null;

				case 1:
					root = new Node(arr[startIndex], false);
					if (redNode is null == false) {
						root.Left = redNode;
					}

					break;

				case 2:
					root = new Node(arr[startIndex], false) { Right = new Node(arr[endIndex], true) };
					if (redNode is null == false) {
						root.Left = redNode;
					}

					break;

				case 3:
					root = new Node(arr[startIndex + 1], false) {
						Left = new Node(arr[startIndex], false),
						Right = new Node(arr[endIndex], false)
					};

					if (redNode is null == false) {
						root.Left.Left = redNode;
					}

					break;

				default:
					int midpt = ((startIndex + endIndex) / 2);
					root = new Node(arr[midpt], false) {
						Left = ConstructRootFromSortedArray(arr, startIndex, midpt - 1, redNode),
						Right = size % 2 == 0
							? ConstructRootFromSortedArray(arr, midpt + 2, endIndex, new Node(arr[midpt + 1], true))
							: ConstructRootFromSortedArray(arr, midpt + 1, endIndex, null)
					};

					break;
			}

			return root;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ReplaceNode(Node match, Node parentOfMatch, Node succesor, Node parentOfSuccesor)
		{
			if (succesor == match) {
				succesor = match.Left;
			} else {
				if (succesor.Right is null == false) {
					succesor.Right.IsRed = false;
				}

				if (parentOfSuccesor != match) {
					parentOfSuccesor.Left = succesor.Right;
					succesor.Right = match.Right;
				}

				succesor.Left = match.Left;
			}

			if (succesor is null == false) {
				succesor.IsRed = match.IsRed;
			}

			ReplaceChildOrRoot(parentOfMatch, match, succesor);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void InsertionBalance(Node current, ref Node parent, Node grandParent, Node greatGrandParent)
		{
			bool parentIsOnRight = grandParent.Right == parent;
			bool currentIsOnRight = parent.Right == current;
			Node newChildOfGreatGrandParent;
			if (parentIsOnRight == currentIsOnRight) {
				newChildOfGreatGrandParent = currentIsOnRight
					? grandParent.RotateLeft()
					: grandParent.RotateRight();
			} else {
				newChildOfGreatGrandParent = currentIsOnRight
					? grandParent.RotateLeftRight()
					: grandParent.RotateRightLeft();
				parent = greatGrandParent;
			}

			grandParent.IsRed = true;
			newChildOfGreatGrandParent.IsRed = false;
			ReplaceChildOrRoot(greatGrandParent, grandParent, newChildOfGreatGrandParent);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ReplaceChildOrRoot(Node parent, Node child, Node newChild)
		{
			if (parent is null == false) {
				if (parent.Left == child) {
					parent.Left = newChild;
				} else {
					parent.Right = newChild;
				}
			} else {
				root_ = newChild;
			}
		}

		public class Node
		{
			public bool IsRed { get; set; }
			public T Item { get; set; }
			public Node Parent { get; private set; }

			private Node _Left;
			public Node Left
			{
				get => _Left;
				set
				{
					_Left = value;
					if (value is null == false) {
						value.Parent = this;
					}

					Update();
				}
			}

			private Node _Right;
			public Node Right
			{
				get => _Right;
				set
				{
					_Right = value;
					if (value is null == false) {
						value.Parent = this;
					}

					Update();
				}
			}
			public int Size { get; private set; } = 1;
			public Node(T item, bool isRed)
			{
				Item = item;
				IsRed = isRed;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Update()
			{
				for (var cur = this; cur is null == false; cur = cur.Parent) {
					if (!cur.UpdateSize()) {
						break;
					}

					if (cur.Parent is null == false
						&& cur.Parent.Left != cur
						&& cur.Parent.Right != cur) {
						cur.Parent = null;
						return;
					}
				}
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool UpdateSize()
			{
				var oldsize = Size;
				var size = 1;
				if (Left is null == false) {
					size += Left.Size;
				}
				if (Right is null == false) {
					size += Right.Size;
				}

				Size = size;
				return oldsize != size;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool IsNonNullRed(Node node) => node is null == false && node.IsRed;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool IsNonNullBlack(Node node) => node is null == false && node.IsRed == false;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool IsNullOrBlack(Node node) => node is null || node.IsRed == false;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool Is2Node() => IsRed == false && IsNullOrBlack(Left) && IsNullOrBlack(Right);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool Is4Node() => IsNonNullRed(Left) && IsNonNullRed(Right);

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Merge2Nodes()
			{
				IsRed = false;
				Left.IsRed = true;
				Right.IsRed = true;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Split4Node()
			{
				IsRed = true;
				Left.IsRed = false;
				Right.IsRed = false;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public TreeRotation GetRotation(Node current, Node sibling)
			{
				if (IsNonNullRed(sibling.Left)) {
					if (Left == current) {
						return TreeRotation.RightLeft;
					}

					return TreeRotation.Right;
				} else {
					if (Left == current) {
						return TreeRotation.Left;
					}

					return TreeRotation.LeftRight;
				}
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Node GetSibling(Node node)
			{
				return Left == node ? Right : Left;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Node Rotate(TreeRotation rotation)
			{
				switch (rotation) {
					default:
					case TreeRotation.Right:
						Left.Left.IsRed = false;
						return RotateRight();

					case TreeRotation.Left:
						Right.Right.IsRed = false;
						return RotateLeft();

					case TreeRotation.RightLeft:
						return RotateRightLeft();

					case TreeRotation.LeftRight:
						return RotateLeftRight();
				}
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Node RotateLeft()
			{
				Node child = Right;
				Right = child.Left;
				child.Left = this;
				return child;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Node RotateLeftRight()
			{
				Node child = Left;
				Node grandChild = child.Right;
				Left = grandChild.Right;
				grandChild.Right = this;
				child.Right = grandChild.Left;
				grandChild.Left = child;
				return grandChild;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Node RotateRight()
			{
				Node child = Left;
				Left = child.Right;
				child.Right = this;
				return child;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Node RotateRightLeft()
			{
				Node child = Right;
				Node grandChild = child.Left;
				Right = grandChild.Left;
				grandChild.Left = this;
				child.Left = grandChild.Right;
				grandChild.Right = child;
				return grandChild;
			}

			public override string ToString() => $"Size = {Size}, Item = {Item}";
		}

		public enum TreeRotation : byte
		{
			Left = 1,
			Right = 2,
			RightLeft = 3,
			LeftRight = 4,
		}

		public struct Enumerator : IEnumerator<T>
		{
			private readonly RedBlackTree3<T> tree_;
			private readonly Stack<Node> stack_;
			private readonly bool reverse_;
			private Node current_;

			internal Enumerator(RedBlackTree3<T> tree)
				: this(tree, false, tree.root_)
			{
			}

			internal Enumerator(RedBlackTree3<T> set, bool reverse, Node startNode)
			{
				tree_ = set;
				stack_ = new Stack<Node>(2 * Log2(tree_.Count + 1));
				current_ = null;
				reverse_ = reverse;
				Intialize(startNode);
			}

			private void Intialize(Node startNode)
			{
				current_ = null;
				Node node = startNode;
				Node next;
				while (node != null) {
					next = (reverse_ ? node.Right : node.Left);
					stack_.Push(node);
					node = next;
				}
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static int Log2(int num) => num == 0 ? 0 : BitOperations.Log2((uint)num) + 1;
			object System.Collections.IEnumerator.Current => Current;
			public T Current => current_ is null ? default : current_.Item;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool MoveNext()
			{
				if (stack_.Count == 0) {
					current_ = null;
					return false;
				}

				current_ = stack_.Pop();
				var node = reverse_ ? current_.Left : current_.Right;
				while (node is null == false) {
					var next = reverse_ ? node.Right : node.Left;
					stack_.Push(node);
					node = next;
				}

				return true;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Reset() => throw new NotSupportedException();

			public void Dispose()
			{
			}
		}
	}
}

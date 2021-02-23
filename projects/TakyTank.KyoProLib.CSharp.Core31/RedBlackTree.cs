using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.Core31
{
	public class RedBlackTree<T> : ICollection<T>, IReadOnlyCollection<T>
	{
		private readonly Comparison<T> comparer_;
		private readonly bool isMulti_;
		private Node root_;

		public RedBlackTree(bool isMulti = false)
			: this(Comparer<T>.Default, isMulti) { }

		public RedBlackTree(IEnumerable<T> collection, bool isMulti = false)
			: this(collection, Comparer<T>.Default, isMulti) { }

		public RedBlackTree(IComparer<T> comparer, bool isMulti = false)
			: this(comparer.Compare, isMulti) { }
		public RedBlackTree(Comparison<T> comparer, bool isMulti = false)
		{
			comparer_ = comparer;
			isMulti_ = isMulti;
		}

		public RedBlackTree(IEnumerable<T> collection, IComparer<T> comparer, bool isMulti = false)
			: this(collection, comparer.Compare, isMulti) { }
		public RedBlackTree(IEnumerable<T> collection, Comparison<T> comparer, bool isMulti = false)
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

		public bool Contains(T item) => FindNode(item) is null == false;
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

				if (Is4Node(current)) {
					Split4Node(current);
					if (IsNonNullRed(parent) == true) {
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

		public bool RemoveAt(int index) => Remove(FindByIndex(index).Item);
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
				if (Is2Node(current)) {
					if (parent is null) {
						current.IsRed = true;
					} else {
						Node sibling = GetSibling(current, parent);
						if (sibling.IsRed) {
							if (parent.Right == sibling) {
								RotateLeft(parent);
							} else {
								RotateRight(parent);
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

						if (Is2Node(sibling)) {
							Merge2Nodes(parent);
						} else {
							TreeRotation rotation = GetRotation(parent, current, sibling);
							Node newGrandParent = Rotate(parent, rotation);
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

		public void Clear()
		{
			root_ = null;
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			foreach (var item in this) {
				array[arrayIndex++] = item;
			}
		}

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
		public Node FindNodeLowerBound(T item) => BinarySearch(item, true).node;
		public Node FindNodeUpperBound(T item) => BinarySearch(item, false).node;
		public T LowerBoundItem(T item) => BinarySearch(item, true).node.Item;
		public T UpperBoundItem(T item) => BinarySearch(item, false).node.Item;
		public int LowerBoundIndex(T item) => BinarySearch(item, true).index;
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

		private static int CountOf(Node node) => node is null ? 0 : node.Size;
		private static bool Is2Node(Node node) => IsNonNullBlack(node) && IsNullOrBlack(node.Left) && IsNullOrBlack(node.Right);
		private static bool Is4Node(Node node) => IsNonNullRed(node.Left) && IsNonNullRed(node.Right);
		private static bool IsNonNullRed(Node node) => node is null == false && node.IsRed;
		private static bool IsNonNullBlack(Node node) => node is null == false && !node.IsRed;
		private static bool IsNullOrBlack(Node node) => node is null || !node.IsRed;

		private static void Merge2Nodes(Node parent)
		{
			parent.IsRed = false;
			parent.Left.IsRed = true;
			parent.Right.IsRed = true;
		}

		private static void Split4Node(Node node)
		{
			node.IsRed = true;
			node.Left.IsRed = false;
			node.Right.IsRed = false;
		}

		private static Node GetSibling(Node node, Node parent)
		{
			return parent.Left == node ? parent.Right : parent.Left;
		}

		private static Node Rotate(Node node, TreeRotation rotation)
		{
			switch (rotation) {
				case TreeRotation.Right:
					node.Left.Left.IsRed = false;
					return RotateRight(node);

				case TreeRotation.Left:
					node.Right.Right.IsRed = false;
					return RotateLeft(node);

				case TreeRotation.RightLeft:
					return RotateRightLeft(node);

				case TreeRotation.LeftRight:
					return RotateLeftRight(node);

				default: throw new InvalidOperationException();
			}
		}

		private static Node RotateLeft(Node node)
		{
			Node child = node.Right;
			node.Right = child.Left;
			child.Left = node;
			return child;
		}

		private static Node RotateLeftRight(Node node)
		{
			Node child = node.Left;
			Node grandChild = child.Right;
			node.Left = grandChild.Right;
			grandChild.Right = node;
			child.Right = grandChild.Left;
			grandChild.Left = child;
			return grandChild;
		}

		private static Node RotateRight(Node node)
		{
			Node child = node.Left;
			node.Left = child.Right;
			child.Right = node;
			return child;
		}

		private static Node RotateRightLeft(Node node)
		{
			Node child = node.Right;
			Node grandChild = child.Left;
			node.Right = grandChild.Left;
			grandChild.Left = node;
			child.Left = grandChild.Right;
			grandChild.Right = child;
			return grandChild;
		}

		private static TreeRotation GetRotation(Node parent, Node current, Node sibling)
		{
			if (IsNonNullRed(sibling.Left)) {
				if (parent.Left == current) {
					return TreeRotation.RightLeft;
				}

				return TreeRotation.Right;
			} else {
				if (parent.Left == current) {
					return TreeRotation.Left;
				}

				return TreeRotation.LeftRight;
			}
		}

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

		private void InsertionBalance(Node current, ref Node parent, Node grandParent, Node greatGrandParent)
		{
			bool parentIsOnRight = grandParent.Right == parent;
			bool currentIsOnRight = parent.Right == current;
			Node newChildOfGreatGrandParent;
			if (parentIsOnRight == currentIsOnRight) {
				newChildOfGreatGrandParent = currentIsOnRight ? RotateLeft(grandParent) : RotateRight(grandParent);
			} else {
				newChildOfGreatGrandParent = currentIsOnRight ? RotateLeftRight(grandParent) : RotateRightLeft(grandParent);
				parent = greatGrandParent;
			}

			grandParent.IsRed = true;
			newChildOfGreatGrandParent.IsRed = false;
			ReplaceChildOrRoot(greatGrandParent, grandParent, newChildOfGreatGrandParent);
		}

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

		public struct Enumerator : IEnumerator<T>
		{
			private readonly RedBlackTree<T> tree_;
			private readonly Stack<Node> stack_;
			private readonly bool reverse_;
			private Node current_;

			internal Enumerator(RedBlackTree<T> set)
				: this(set, false, null)
			{
			}

			internal Enumerator(RedBlackTree<T> set, bool reverse, Node startNode)
			{
				tree_ = set;
				stack_ = new Stack<Node>(2 * Log2(tree_.Count + 1));
				current_ = null;
				reverse_ = reverse;
				if (startNode is null) {
					IntializeAll();
				} else {
					Intialize(startNode);
				}
			}

			private void IntializeAll()
			{
				var node = tree_.root_;
				while (node is null == false) {
					var next = reverse_ ? node.Right : node.Left;
					stack_.Push(node);
					node = next;
				}
			}

			private void Intialize(Node startNode)
			{
				if (startNode is null) {
					throw new InvalidOperationException(nameof(startNode) + "is null");
				}

				current_ = null;
				var node = startNode;
				var list = new List<Node>(Log2(tree_.Count + 1));
				var comparer = tree_.comparer_;
				if (reverse_) {
					while (node is null == false) {
						list.Add(node);
						var parent = node.Parent;
						if (parent is null || parent.Left == node) {
							node = parent;
							break;
						}

						node = parent;
					}

					while (node is null == false) {
						var parent = node.Parent;
						if (parent is null || parent.Right == node) {
							node = parent;
							break;
						}

						node = parent;
					}

					while (node is null == false) {
						if (comparer(startNode.Item, node.Item) >= 0) {
							list.Add(node);
						}
						node = node.Parent;
					}
				} else {
					while (node is null == false) {
						list.Add(node);
						var parent = node.Parent;
						if (parent is null || parent.Right == node) {
							node = parent;
							break;
						}

						node = parent;
					}

					while (node is null == false) {
						var parent = node.Parent;
						if (parent is null || parent.Left == node) {
							node = parent;
							break;
						}

						node = parent;
					}

					while (node is null == false) {
						if (comparer(startNode.Item, node.Item) <= 0) {
							list.Add(node);
						}
						node = node.Parent;
					}
				}

				list.Reverse();
				foreach (var n in list) {
					stack_.Push(n);
				}
			}

			private static int Log2(int num) => num == 0 ? 0 : BitOperations.Log2((uint)num) + 1;
			object System.Collections.IEnumerator.Current => Current;
			public T Current => current_ is null ? default : current_.Item;
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

			public void Reset() => throw new NotSupportedException();

			public void Dispose()
			{
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
				get { return _Left; }
				set
				{
					_Left = value;
					if (value is null == false) {
						value.Parent = this;
					}
					for (var cur = this; cur is null == false; cur = cur.Parent) {
						if (!cur.UpdateSize()) {
							break;
						}
						if (cur.Parent is null == false && cur.Parent.Left != cur && cur.Parent.Right != cur) {
							cur.Parent = null;
							break;
						}
					}
				}
			}

			private Node _Right;
			public Node Right
			{
				get { return _Right; }
				set
				{
					_Right = value;
					if (value is null == false) {
						value.Parent = this;
					}
					for (var cur = this; cur is null == false; cur = cur.Parent) {
						if (!cur.UpdateSize()) {
							break;
						}
						if (cur.Parent is null == false && cur.Parent.Left != cur && cur.Parent.Right != cur) {
							cur.Parent = null;
							break;
						}
					}
				}
			}
			public int Size { get; private set; } = 1;
			public Node(T item, bool isRed)
			{
				Item = item;
				IsRed = isRed;
			}

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
			public override string ToString() => $"Size = {Size}, Item = {Item}";
		}

		private enum TreeRotation : byte
		{
			Left = 1,
			Right = 2,
			RightLeft = 3,
			LeftRight = 4,
		}
	}
}

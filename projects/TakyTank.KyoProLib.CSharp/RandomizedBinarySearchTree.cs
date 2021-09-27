using System;
using System.Collections.Generic;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class RandomizedBinarySearchTree<T> : IEnumerable<T>, IReadOnlyCollection<T>
	{
		private static readonly Random random_ = new Random();

		private readonly IComparer<T> comparer_;
		private readonly T inf_;
		private readonly bool isMulti_;
		private Node root_;

		public RandomizedBinarySearchTree(bool isMulti = false, T inf = default(T))
			: this(Comparer<T>.Default, isMulti, inf) { }
		public RandomizedBinarySearchTree(Comparison<T> comaprison, bool isMulti = false, T inf = default(T))
			: this(Comparer<T>.Create(comaprison), isMulti, inf) { }
		public RandomizedBinarySearchTree(IComparer<T> comparer, bool isMulti = false, T inf = default(T))
		{
			comparer_ = comparer;
			isMulti_ = isMulti;
			inf_ = inf;
		}

		public T this[int index] => ElementAt(index);

		public int Count => CountOf(root_);
		public T Min => Count == 0 ? inf_ : ElementAt(0);
		public T Max => Count == 0 ? inf_ : ElementAt(Count - 1);

		public bool Contains(T value) => FindOn(root_, value) != null;
		public int CountOf(Node node) => node == null ? 0 : node.SubTreeSize;
		public int IndexOf(T v) => UpperBoundOn(root_, v) - LowerBoundOn(root_, v);
		public T ElementAt(int index) => FindByIndexOn(root_, index).Value;

		public void Clear() => root_ = null;
		public void Add(T value)
		{
			if (root_ is null) {
				root_ = new Node(value);
			} else {
				if (isMulti_ == false && FindOn(root_, value) != null) {
					return;
				}

				root_ = Insert(root_, value);
			}
		}

		public Node Insert(Node node, T value)
		{
			var index = LowerBoundOn(node, value);
			return InsertAt(index, node, value);
		}

		private Node InsertAt(int index, Node node, T value)
		{
			var (left, right) = Split(node, index);
			return Merge(Merge(left, new Node(value)), right);
		}

		public void Remove(T value) => root_ = RemoveOn(root_, value);
		public void RemoveAt(int index) => root_ = RemoveOnAt(root_, index);

		public Node RemoveOn(Node node, T value)
		{
			if (FindOn(node, value) is null) {
				return node;
			}

			return RemoveOnAt(node, LowerBoundOn(node, value));
		}

		public Node RemoveOnAt(Node node, int index)
		{
			var (left1, right1) = Split(node, index);
			var (_, right2) = Split(right1, 1);
			return Merge(left1, right2);
		}

		public Node FindOn(Node node, T target)
		{
			while (node is null == false) {
				var cmp = comparer_.Compare(node.Value, target);
				if (cmp > 0) {
					node = node.LeftChild;
				} else if (cmp < 0) {
					node = node.RightChild;
				} else {
					break;
				}
			}
			return node;
		}

		public Node FindByIndexOn(Node node, int index)
		{
			if (node is null) {
				return null;
			}

			var currentIndex = CountOf(node) - CountOf(node.RightChild) - 1;
			while (node is null == false) {
				if (currentIndex == index) {
					return node;
				}

				if (currentIndex > index) {
					node = node.LeftChild;
					currentIndex -= CountOf(node?.RightChild) + 1;
				} else {
					node = node.RightChild;
					currentIndex += CountOf(node?.LeftChild) + 1;
				}
			}

			return null;
		}

		public Node Merge(Node left, Node right)
		{
			if (left is null || right is null) {
				return left ?? right;
			}

			if ((double)CountOf(left) / (CountOf(left) + CountOf(right)) > random_.NextDouble()) {
				left.RightChild = Merge(left.RightChild, right);
				return Update(left);
			} else {
				right.LeftChild = Merge(left, right.LeftChild);
				return Update(right);
			}
		}

		public (Node left, Node right) Split(Node node, int index)
		{
			if (node is null) {
				return (null, null);
			}

			//[0, index), [index, n)
			if (index <= CountOf(node.LeftChild)) {
				var (left, right) = Split(node.LeftChild, index);
				node.LeftChild = right;
				return (left, Update(node));
			} else {
				var (left, right) = Split(node.RightChild, index - CountOf(node.LeftChild) - 1);
				node.RightChild = left;
				return (Update(node), right);
			}
		}

		public int LowerBound(T v) => LowerBoundOn(root_, v);
		public int UpperBound(T v) => UpperBoundOn(root_, v);
		public T LowerBoundValue(T v) => LowerBoundValueOn(root_, v);
		public T UpperBoundValue(T v) => UpperBoundValueOn(root_, v);

		public int UpperBoundOn(Node node, T value)
		{
			if (node is null) {
				return -1;
			}

			var ret = 0;
			while (node is null == false) {
				if (comparer_.Compare(node.Value, value) > 0) {
					node = node?.LeftChild;
				} else {
					ret += CountOf(node?.LeftChild) + 1;
					node = node?.RightChild;
				}
			}

			return ret;
		}

		public T UpperBoundValueOn(Node node, T value)
		{
			T ret = inf_;
			while (node is null == false) {
				if (comparer_.Compare(node.Value, value) > 0) {
					ret = node.Value;
					node = node?.LeftChild;
				} else {
					node = node?.RightChild;
				}
			}

			return ret;
		}

		public int LowerBoundOn(Node node, T value)
		{
			if (node is null) {
				return -1;
			}

			var ret = 0;
			while (node is null == false) {
				if (comparer_.Compare(node.Value, value) >= 0) {
					node = node?.LeftChild;
				} else {
					ret += CountOf(node?.LeftChild) + 1;
					node = node?.RightChild;
				}
			}

			return ret;
		}

		public T LowerBoundValueOn(Node node, T value)
		{
			T ret = inf_;
			while (node is null == false) {
				if (comparer_.Compare(node.Value, value) >= 0) {
					ret = node.Value;
					node = node?.LeftChild;
				} else {
					node = node?.RightChild;
				}
			}

			return ret;
		}

		private Node Update(Node node)
		{
			node.SubTreeSize = CountOf(node.LeftChild) + CountOf(node.RightChild) + 1;
			return node;
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
		public IEnumerator<T> GetEnumerator()
		{
			foreach (var v in Enumerate(root_)) {
				yield return v;
			};
		}

		public IEnumerable<T> Enumerate(Node node)
		{
			var ret = new List<T>();
			Enumerate(node, ret);
			return ret;
		}

		private void Enumerate(Node node, List<T> ret)
		{
			if (node is null) {
				return;
			}

			Enumerate(node.LeftChild, ret);
			ret.Add(node.Value);
			Enumerate(node.RightChild, ret);
		}

		public class Node
		{
			public T Value { get; set; }
			public Node LeftChild { get; set; }
			public Node RightChild { get; set; }
			public int SubTreeSize { get; set; }
			public Node(T value)
			{
				Value = value;
				SubTreeSize = 1;
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class DynamicLazySegmentTree<TData, TUpdate>
	{
		private readonly Stack<Node> pool_ = new Stack<Node>();
		private readonly long n_;
		private readonly Func<TData, TData, TData> operate_;
		private readonly Func<TData, TUpdate, long, TData> update_;
		private readonly TData unitData_;
		private readonly TUpdate identity_;
		private Node root_;

		public TData this[int index]
		{
			get => GetCore(root_, index, 0, n_);
			set => SetCore(ref root_, index, 0, n_, value);
		}

		public DynamicLazySegmentTree(
			long count,
			TData unitData,
			TUpdate unitUpdate,
			Func<TData, TData, TData> operate,
			Func<TData, TUpdate, long, TData> update,
			Func<TUpdate, TUpdate, TUpdate> compose)
		{
			n_ = count;
			unitData_ = unitData;
			identity_ = unitUpdate;
			operate_ = operate;
			update_ = update;
			root_ = null;

			Node.unitData_ = unitData;
			Node.identity_ = identity_;
			Node.operate_ = operate;
			Node.update_ = update;
			Node.compose_ = compose;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TData QueryAll()
			=> root_ is null == false ? root_.Value : unitData_;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TData Query(long left, long right)
			=> QueryCore(ref root_, left, right, 0, n_);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private TData QueryCore(ref Node node, long left, long right, long l, long r)
		{
			if (node is null || r <= left || right <= l) {
				return unitData_;
			}

			if (left <= l && r <= right) {
				return node.Value;
			}

			Propagate(node);
			long c = (l + r) >> 1;
			return operate_(
				QueryCore(ref node.Left, left, right, l, c),
				QueryCore(ref node.Right, left, right, c, r));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Update(long p, TUpdate f)
			=> UpdateCore(ref root_, p, 0, n_, f);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void UpdateCore(ref Node node, long p, long l, long r, TUpdate f)
		{
			if (node is null) {
				node = NewNode(unitData_, r - l);
			}

			if (r - l == 1) {
				node.Value = update_(node.Value, f, 1);
				return;
			}

			Propagate(node);
			long c = (l + r) >> 1;
			if (p < c) {
				UpdateCore(ref node.Left, p, l, c, f);
			} else {
				UpdateCore(ref node.Right, p, c, r, f);
			}

			node.Update();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Update(long left, long right, TUpdate f)
			=> UpdateCore(ref root_, left, right, 0, n_, f);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void UpdateCore(ref Node node, long left, long right, long l, long r, TUpdate f)
		{
			if (r <= left || right <= l) {
				return;
			}

			if (node is null) {
				node = NewNode(unitData_, r - l);
			}

			if (left <= l && r <= right) {
				node.AllApply(f);
				return;
			}

			Propagate(node);
			long c = (l + r) >> 1;
			UpdateCore(ref node.Left, left, right, l, c, f);
			UpdateCore(ref node.Right, left, right, c, r, f);
			node.Update();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Reset(long left, long right)
			=> ResetCore(ref root_, left, right, 0, n_);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ResetCore(ref Node node, long left, long right, long l, long r)
		{
			if (node is null || r <= left || right <= l) {
				return;
			}

			if (left <= l && r <= right) {
				pool_.Push(node);
				node = null;
				return;
			}

			Propagate(node);
			long c = (l + r) >> 1;
			ResetCore(ref node.Left, left, right, l, c);
			ResetCore(ref node.Right, left, right, c, r);
			node.Update();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SetCore(ref Node node, long p, long l, long r, TData x)
		{
			if (node is null) {
				node = NewNode(x, r - l);
			}

			if (r - l == 1) {
				node.Value = x;
				return;
			}

			Propagate(node);
			long c = (l + r) >> 1;
			if (p < c) {
				SetCore(ref node.Left, p, l, c, x);
			} else {
				SetCore(ref node.Right, p, c, r, x);
			}

			node.Update();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private TData GetCore(Node node, long p, long l, long r)
		{
			if (node is null) {
				return unitData_;
			}

			if (r - l == 1) {
				return node.Value;
			}

			Propagate(node);
			long c = (l + r) >> 1;
			if (p < c) {
				return GetCore(node.Left, p, l, c);
			} else {
				return GetCore(node.Right, p, c, r);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Propagate(Node node)
		{
			if (node.HasLazy == false) {
				return;
			}

			if (node.Size > 1) {
				if (node.Left is null) {
					node.Left = NewNode(unitData_, node.Size >> 1);
				}

				if (node.Right is null) {
					node.Right = NewNode(unitData_, (node.Size + 1) >> 1);
				}

				node.Left.AllApply(node.Lazy);
				node.Right.AllApply(node.Lazy);
			}

			node.Lazy = identity_;
			node.HasLazy = false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Node NewNode(TData value, long size)
		{
			if (pool_.Count > 0) {
				var node = pool_.Pop();
				node.Value = value;
				node.Size = size;
				node.Lazy = identity_;
				node.HasLazy = false;
				if (node.Left is null == false) {
					pool_.Push(node.Left);
					node.Left = null;
				}

				if (node.Right is null == false) {
					pool_.Push(node.Right);
					node.Right = null;
				}

				return node;
			} else {
				return new Node(value, size);
			}
		}

		private class Node
		{
			public static Func<TData, TData, TData> operate_;
			public static Func<TData, TUpdate, long, TData> update_;
			public static Func<TUpdate, TUpdate, TUpdate> compose_;
			public static TData unitData_;
			public static TUpdate identity_;

			public long Size;
			public TData Value;
			public TUpdate Lazy;
			public Node Left;
			public Node Right;
			public bool HasLazy;

			public Node(TData value, long size)
			{
				Value = value;
				Lazy = identity_;
				HasLazy = false;
				Size = size;
				Left = null;
				Right = null;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Update()
			{
				Value = operate_(
					Left is null == false ? Left.Value : unitData_,
					Right is null == false ? Right.Value : unitData_);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void AllApply(TUpdate f)
			{
				Value = update_(Value, f, Size);
				if (HasLazy) {
					Lazy = compose_(f, Lazy);
				} else {
					Lazy = f;
					HasLazy = true;
				}
			}
		}
	}
}

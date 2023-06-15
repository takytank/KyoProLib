﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class DynamicSegmentTree<T>
	{
		private readonly Stack<Node> _pool = new Stack<Node>();
		private readonly long _n;
		private static Func<T, T, T> _operate;
		private readonly T _identity;
		private Node _root;

		public T this[long index]
		{
			get => GetCore(_root, index, 0, _n);
			set => SetCore(ref _root, index, 0, _n, value);
		}

		public DynamicSegmentTree(long count, T identity, Func<T, T, T> operate)
		{
			_n = count;
			_identity = identity;
			_operate = operate;
			_root = null;

			Node._identity = identity;
			Node._operate = operate;
		}

		public DynamicSegmentTree(T[] values, T identity, Func<T, T, T> operate)
			: this(values.Length, identity, operate)
		{
			for (int i = 0; i < _n; i++) {
				Update(i, values[i]);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Update(int index, T value)
			=> SetCore(ref _root, index, 0, _n, value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T QueryAll()
			=> _root is null == false ? _root.Product : _identity;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Query(long left, long right) => QueryCore(_root, left, right, 0, _n);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Reset(long left, long right) => ResetCore(ref _root, left, right, 0, _n);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long MaxRight(long left, Predicate<T> satisfies)
		{
			T product = _identity;
			return MaxRightCore(_root, left, 0, _n, satisfies, ref product);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long MinLeft(long right, Predicate<T> satisfies)
		{
			T product = _identity;
			return MinLeftCore(_root, right, 0, _n, satisfies, ref product);
		}

		private void SetCore(ref Node node, long index, long sl, long sr, T value)
		{
			if (node is null) {
				node = NewNode(index, value);
				return;
			}

			if (node.Index == index) {
				node.Value = value;
				node.Update();
				return;
			}

			long c = (sl + sr) >> 1;
			if (index < c) {
				if (node.Index < index) {
					(node.Index, index) = (index, node.Index);
					(node.Value, value) = (value, node.Value);
				}

				SetCore(ref node.Left, index, sl, c, value);
			} else {
				if (index < node.Index) {
					(node.Index, index) = (index, node.Index);
					(node.Value, value) = (value, node.Value);
				}

				SetCore(ref node.Right, index, c, sr, value);
			}

			node.Update();
		}

		private T GetCore(Node node, long index, long sl, long sr)
		{
			if (node is null) {
				return _identity;
			}

			if (node.Index == index) {
				return node.Value;
			}

			long c = (sl + sr) >> 1;
			if (index < c) {
				return GetCore(node.Left, index, sl, c);
			} else {
				return GetCore(node.Right, index, c, sr);
			}
		}

		private T QueryCore(Node node, long il, long ir, long sl, long sr)
		{
			if (node is null || sr <= il || ir <= sl) {
				return _identity;
			}

			if (il <= sl && sr <= ir) {
				return node.Product;
			}

			long c = (sl + sr) >> 1;
			T result = QueryCore(node.Left, il, ir, sl, c);
			if (il <= node.Index && node.Index < ir) {
				result = _operate(result, node.Value);
			}

			return _operate(result, QueryCore(node.Right, il, ir, c, sr));
		}

		private void ResetCore(ref Node node, long il, long ir, long sl, long sr)
		{
			if (node is null || sr <= il || ir <= sl) {
				return;
			}

			if (il <= sl && sr <= ir) {
				_pool.Push(node);
				node = null;
				return;
			}

			long c = (sl + sr) >> 1;
			ResetCore(ref node.Left, il, ir, sl, c);
			ResetCore(ref node.Right, il, ir, c, sr);

			node.Update();
		}

		private long MaxRightCore(
			Node node, long l, long sl, long sr, Predicate<T> satisfies, ref T product)
		{
			if (node is null || sr <= l) {
				return _n;
			}

			if (satisfies(_operate(product, node.Product))) {
				product = _operate(product, node.Product);
				return _n;
			}

			long c = (sl + sr) >> 1;
			long result = MaxRightCore(node.Left, l, sl, c, satisfies, ref product);
			if (result != _n) {
				return result;
			}

			if (l <= node.Index) {
				product = _operate(product, node.Value);
				if (satisfies(product) == false) {
					return node.Index;
				}
			}

			return MaxRightCore(node.Right, l, c, sr, satisfies, ref product);
		}

		private long MinLeftCore(
			Node node, long r, long sl, long sr, Predicate<T> satisfies, ref T product)
		{
			if (node is null || r <= sl) {
				return 0;
			}

			if (satisfies(_operate(node.Product, product))) {
				product = _operate(node.Product, product);
				return 0;
			}

			long c = (sl + sr) >> 1;
			long result = MinLeftCore(node.Right, r, c, sr, satisfies, ref product);
			if (result != 0) {
				return result;
			}

			if (node.Index < r) {
				product = _operate(node.Value, product);
				if (satisfies(product) == false) {
					return node.Index + 1;
				}
			}

			return MinLeftCore(node.Left, r, sl, c, satisfies, ref product);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Node NewNode(long index, T value)
		{
			if (_pool.Count > 0) {
				var node = _pool.Pop();
				node.Index = index;
				node.Value = value;
				node.Product = value;
				if (node.Left is null == false) {
					_pool.Push(node.Left);
					node.Left = null;
				}

				if (node.Right is null == false) {
					_pool.Push(node.Right);
					node.Right = null;
				}

				return node;
			} else {
				return new Node(index, value);
			}
		}

		private class Node
		{
			public static Func<T, T, T> _operate;
			public static T _identity;

			public long Index;
			public T Value;
			public T Product;
			public Node Left;
			public Node Right;

			public Node(long index, T value)
			{
				Index = index;
				Value = value;
				Product = value;
				Left = null;
				Right = null;
			}

			public void Update()
			{
				Product = _operate(
					_operate(
						Left is null == false ? Left.Product : _identity,
						Value),
					Right is null == false ? Right.Product : _identity);
			}
		};

	}

	public class DynamicLazySegmentTree<TData, TUpdate>
	{
		private readonly Stack<Node> _pool = new Stack<Node>();
		private readonly long _n;
		private readonly Func<TData, TData, TData> _operate;
		private readonly Func<TData, TUpdate, long, TData> _update;
		private readonly TData _identity;
		private readonly TUpdate _identityMap;
		private Node _root;

		public TData this[int index]
		{
			get => GetCore(_root, index, 0, _n);
			set => SetCore(ref _root, index, 0, _n, value);
		}

		public DynamicLazySegmentTree(
			long count,
			TData identity,
			TUpdate identityMap,
			Func<TData, TData, TData> operate,
			Func<TData, TUpdate, long, TData> update,
			Func<TUpdate, TUpdate, TUpdate> compose)
		{
			_n = count;
			_identity = identity;
			_identityMap = identityMap;
			_operate = operate;
			_update = update;
			_root = null;

			Node._identity = identity;
			Node._identityMap = _identityMap;
			Node._operate = operate;
			Node._update = update;
			Node._compose = compose;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Update(long index, TUpdate f)
			=> UpdateCore(ref _root, index, 0, _n, f);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void UpdateCore(ref Node node, long index, long sl, long sr, TUpdate f)
		{
			if (node is null) {
				node = NewNode(_identity, sr - sl);
			}

			if (sr - sl == 1) {
				node.Value = _update(node.Value, f, 1);
				return;
			}

			Propagate(node);
			long c = (sl + sr) >> 1;
			if (index < c) {
				UpdateCore(ref node.Left, index, sl, c, f);
			} else {
				UpdateCore(ref node.Right, index, c, sr, f);
			}

			node.Update();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Update(long left, long right, TUpdate f)
			=> UpdateCore(ref _root, left, right, 0, _n, f);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void UpdateCore(ref Node node, long il, long ir, long sl, long sr, TUpdate f)
		{
			if (sr <= il || ir <= sl) {
				return;
			}

			if (node is null) {
				node = NewNode(_identity, sr - sl);
			}

			if (il <= sl && sr <= ir) {
				node.AllApply(f);
				return;
			}

			Propagate(node);
			long c = (sl + sr) >> 1;
			UpdateCore(ref node.Left, il, ir, sl, c, f);
			UpdateCore(ref node.Right, il, ir, c, sr, f);
			node.Update();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TData QueryAll()
			=> _root is null == false ? _root.Value : _identity;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TData Query(long left, long right)
			=> QueryCore(ref _root, left, right, 0, _n);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private TData QueryCore(ref Node node, long il, long ir, long sl, long sr)
		{
			if (node is null || sr <= il || ir <= sl) {
				return _identity;
			}

			if (il <= sl && sr <= ir) {
				return node.Value;
			}

			Propagate(node);
			long c = (sl + sr) >> 1;
			return _operate(
				QueryCore(ref node.Left, il, ir, sl, c),
				QueryCore(ref node.Right, il, ir, c, sr));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Reset(long left, long right)
			=> ResetCore(ref _root, left, right, 0, _n);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ResetCore(ref Node node, long il, long ir, long sl, long sr)
		{
			if (node is null || sr <= il || ir <= sl) {
				return;
			}

			if (il <= sl && sr <= ir) {
				_pool.Push(node);
				node = null;
				return;
			}

			Propagate(node);
			long c = (sl + sr) >> 1;
			ResetCore(ref node.Left, il, ir, sl, c);
			ResetCore(ref node.Right, il, ir, c, sr);
			node.Update();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SetCore(ref Node node, long index, long sl, long sr, TData x)
		{
			if (node is null) {
				node = NewNode(x, sr - sl);
			}

			if (sr - sl == 1) {
				node.Value = x;
				return;
			}

			Propagate(node);
			long c = (sl + sr) >> 1;
			if (index < c) {
				SetCore(ref node.Left, index, sl, c, x);
			} else {
				SetCore(ref node.Right, index, c, sr, x);
			}

			node.Update();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private TData GetCore(Node node, long index, long sl, long sr)
		{
			if (node is null) {
				return _identity;
			}

			if (sr - sl == 1) {
				return node.Value;
			}

			Propagate(node);
			long c = (sl + sr) >> 1;
			if (index < c) {
				return GetCore(node.Left, index, sl, c);
			} else {
				return GetCore(node.Right, index, c, sr);
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
					node.Left = NewNode(_identity, node.Size >> 1);
				}

				if (node.Right is null) {
					node.Right = NewNode(_identity, (node.Size + 1) >> 1);
				}

				node.Left.AllApply(node.Lazy);
				node.Right.AllApply(node.Lazy);
			}

			node.Lazy = _identityMap;
			node.HasLazy = false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Node NewNode(TData value, long size)
		{
			if (_pool.Count > 0) {
				var node = _pool.Pop();
				node.Value = value;
				node.Size = size;
				node.Lazy = _identityMap;
				node.HasLazy = false;
				if (node.Left is null == false) {
					_pool.Push(node.Left);
					node.Left = null;
				}

				if (node.Right is null == false) {
					_pool.Push(node.Right);
					node.Right = null;
				}

				return node;
			} else {
				return new Node(value, size);
			}
		}

		private class Node
		{
			public static Func<TData, TData, TData> _operate;
			public static Func<TData, TUpdate, long, TData> _update;
			public static Func<TUpdate, TUpdate, TUpdate> _compose;
			public static TData _identity;
			public static TUpdate _identityMap;

			public long Size;
			public TData Value;
			public TUpdate Lazy;
			public Node Left;
			public Node Right;
			public bool HasLazy;

			public Node(TData value, long size)
			{
				Value = value;
				Lazy = _identityMap;
				HasLazy = false;
				Size = size;
				Left = null;
				Right = null;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Update()
			{
				Value = _operate(
					Left is null == false ? Left.Value : _identity,
					Right is null == false ? Right.Value : _identity);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void AllApply(TUpdate f)
			{
				Value = _update(Value, f, Size);
				if (HasLazy) {
					Lazy = _compose(f, Lazy);
				} else {
					Lazy = f;
					HasLazy = true;
				}
			}
		}
	}
}

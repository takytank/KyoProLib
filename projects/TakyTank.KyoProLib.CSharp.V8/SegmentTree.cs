using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V8
{
	[DebuggerTypeProxy(typeof(SegmentTree<>.DebugView))]
	public class SegmentTree<T> : IEnumerable<T>
	{
		private readonly int n_;
		private readonly T unit_;
		private readonly T[] tree_;
		private readonly Func<T, T, T> operate_;

		public int Count { get; }
		public T Top => tree_[1];

		public T this[int index]
		{
			get => tree_[index + n_];
			set => Update(index, value);
		}

		public SegmentTree(int count, T unit, Func<T, T, T> operate)
		{
			operate_ = operate;
			unit_ = unit;

			Count = count;
			n_ = 1;
			while (n_ < count) {
				n_ <<= 1;
			}

			tree_ = new T[n_ << 1];
			for (int i = 0; i < tree_.Length; i++) {
				tree_[i] = unit;
			}
		}

		public SegmentTree(T[] src, T unit, Func<T, T, T> operate)
			: this(src.Length, unit, operate)
		{
			for (int i = 0; i < src.Length; ++i) {
				tree_[i + n_] = src[i];
			}

			for (int i = n_ - 1; i > 0; --i) {
				tree_[i] = operate_(tree_[i << 1], tree_[(i << 1) + 1]);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Update(int index, T value)
		{
			if (index >= Count) {
				return;
			}

			index += n_;
			tree_[index] = value;
			index >>= 1;
			while (index != 0) {
				tree_[index] = operate_(tree_[index << 1], tree_[(index << 1) + 1]);
				index >>= 1;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Query(Range range) => Query(range.Start.Value, range.End.Value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Query(int left, int right)
		{
			if (left > right || right < 0 || left >= Count) {
				return unit_;
			}

			int l = left + n_;
			int r = right + n_;
			T valL = unit_;
			T valR = unit_;
			while (l < r) {
				if ((l & 1) != 0) {
					valL = operate_(valL, tree_[l]);
					++l;
				}
				if ((r & 1) != 0) {
					--r;
					valR = operate_(tree_[r], valR);
				}

				l >>= 1;
				r >>= 1;
			}

			return operate_(valL, valR);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int FindLeftest(Range range, Func<T, bool> check)
			=> FindLeftest(range.Start.Value, range.End.Value, check);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int FindLeftest(int left, int right, Func<T, bool> check)
			=> FindLeftestCore(left, right, 1, 0, n_, check);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int FindLeftestCore(int left, int right, int v, int l, int r, Func<T, bool> check)
		{
			if (check(tree_[v]) == false || r <= left || right <= l || Count <= left) {
				return right;
			} else if (v >= n_) {
				return v - n_;
			} else {
				int lc = v << 1;
				int rc = (v << 1) + 1;
				int mid = (l + r) >> 1;
				int vl = FindLeftestCore(left, right, lc, l, mid, check);
				if (vl != right) {
					return vl;
				} else {
					return FindLeftestCore(left, right, rc, mid, r, check);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int FindRightest(Range range, Func<T, bool> check)
			=> FindRightest(range.Start.Value, range.End.Value, check);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int FindRightest(int left, int right, Func<T, bool> check)
			=> FindRightestCore(left, right, 1, 0, n_, check);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int FindRightestCore(int left, int right, int v, int l, int r, Func<T, bool> check)
		{
			if (check(tree_[v]) == false || r <= left || right <= l || Count <= left) {
				return left - 1;
			} else if (v >= n_) {
				return v - n_;
			} else {
				int lc = v << 1;
				int rc = (v << 1) + 1;
				int mid = (l + r) >> 1;
				int vr = FindRightestCore(left, right, rc, mid, r, check);
				if (vr != left - 1) {
					return vr;
				} else {
					return FindRightestCore(left, right, lc, l, mid, check);
				}
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
			for (int i = 0; i < Count; i++) {
				yield return this[i];
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		[DebuggerDisplay("data= {" + nameof(data_) + "}", Name = "{" + nameof(key_) + ",nq}")]
		private struct DebugItem
		{
			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			private readonly string key_;
			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			private readonly T data_;
			public DebugItem(int l, int r, T data)
			{
				if (r - l == 1) {
					key_ = $"[{l}]";
				} else {
					key_ = $"[{l}-{r})";
				}

				data_ = data;
			}
		}

		private class DebugView
		{
			private readonly SegmentTree<T> tree_;
			public DebugView(SegmentTree<T> tree)
			{
				tree_ = tree;
			}

			[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
			public DebugItem[] Items
			{
				get
				{
					var items = new List<DebugItem>(tree_.Count);
					int length = tree_.n_;
					while (length > 0) {
						int unit = tree_.n_ / length;
						for (int i = 0; i < length; i++) {
							int l = i * unit;
							int r = l + unit;
							if (l < tree_.Count) {
								int dataIndex = i + length;
								items.Add(new DebugItem(
									l,
									r,
									tree_.tree_[dataIndex]));
							}
						}

						length >>= 1;
					}

					return items.ToArray();
				}
			}
		}
	}

	[DebuggerTypeProxy(typeof(DualSegmentTree<>.DebugView))]
	public class DualSegmentTree<T> : IEnumerable<T>
	{
		private readonly int n_;
		private readonly int height_;
		private readonly T unit_;
		private readonly T[] tree_;
		private readonly bool[] should_;
		private readonly Func<T, T, T> operate_;

		public int Count { get; }

		public T this[int i] => Query(i);

		public DualSegmentTree(int count, T unit, Func<T, T, T> operate)
		{
			operate_ = operate;
			unit_ = unit;

			Count = count;
			n_ = 1;
			height_ = 0;
			while (n_ < count) {
				n_ <<= 1;
				++height_;
			}

			tree_ = new T[n_ << 1];
			should_ = new bool[n_ << 1];
			for (int i = 0; i < tree_.Length; i++) {
				tree_[i] = unit;
			}
		}

		public DualSegmentTree(IReadOnlyList<T> src, T unit, Func<T, T, T> operate)
			: this(src.Count, unit, operate)
		{
			for (int i = 0; i < src.Count; i++) {
				tree_[i + n_] = src[i];
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Query(int i)
		{
			int l = 0;
			int r = n_;
			int k = 1;
			while (r - l > 1) {
				Propagate(k);
				int m = (l + r) / 2;
				if (i < m) {
					r = m;
					k <<= 1;
				} else {
					l = m;
					k = (k << 1) + 1;
				}
			}

			return tree_[k];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Update(Range range, T value)
			=> Update(range.Start.Value, range.End.Value, value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Update(int left, int right, T value)
		{
			if (left > right || right < 0 || left >= Count) {
				return;
			}

			int l = left + n_;
			int r = right + n_;
			PropagateTopDown(l);
			PropagateTopDown(r - 1);
			while (l < r) {
				if ((l & 1) != 0) {
					tree_[l] = operate_(value, tree_[l]);
					should_[l] = true;
					++l;
				}

				if ((r & 1) != 0) {
					--r;
					tree_[r] = operate_(value, tree_[r]);
					should_[r] = true;
				}

				l >>= 1;
				r >>= 1;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int FindLeftest(Range range, Func<T, bool> check)
			=> FindLeftest(range.Start.Value, range.End.Value, check);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int FindLeftest(int left, int right, Func<T, bool> check)
			=> FindLeftestCore(left, right, 1, 0, n_, check);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int FindLeftestCore(int left, int right, int v, int l, int r, Func<T, bool> check)
		{
			Propagate(v);
			if (check(tree_[v]) == false || r <= left || right <= l || Count <= left) {
				return right;
			} else if (v >= n_) {
				return v - n_;
			} else {
				int lc = v << 1;
				int rc = (v << 1) + 1;
				int mid = (l + r) >> 1;
				int vl = FindLeftestCore(left, right, lc, l, mid, check);
				if (vl != right) {
					return vl;
				} else {
					return FindLeftestCore(left, right, rc, mid, r, check);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int FindRightest(Range range, Func<T, bool> check)
			=> FindRightest(range.Start.Value, range.End.Value, check);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int FindRightest(int left, int right, Func<T, bool> check)
			=> FindRightestCore(left, right, 1, 0, n_, check);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int FindRightestCore(int left, int right, int v, int l, int r, Func<T, bool> check)
		{
			Propagate(v);
			if (check(tree_[v]) == false || r <= left || right <= l || Count <= left) {
				return left - 1;
			} else if (v >= n_) {
				return v - n_;
			} else {
				int lc = v << 1;
				int rc = (v << 1) + 1;
				int mid = (l + r) >> 1;
				int vr = FindRightestCore(left, right, rc, mid, r, check);
				if (vr != left - 1) {
					return vr;
				} else {
					return FindRightestCore(left, right, lc, l, mid, check);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void PropagateTopDown(int v)
		{
			for (int i = height_; i > 0; i--) {
				Propagate(v >> i);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Propagate(int v)
		{
			if (should_[v]) {
				if (v < n_) {
					int lc = v << 1;
					int rc = (v << 1) + 1;
					tree_[lc] = operate_(tree_[v], tree_[lc]);
					should_[lc] = true;
					tree_[rc] = operate_(tree_[v], tree_[rc]);
					should_[rc] = true;
					tree_[v] = unit_;
				}

				should_[v] = false;
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
			for (int i = 0; i < Count; i++) {
				yield return this[i];
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		[DebuggerDisplay("data= {" + nameof(data_) + "}", Name = "{" + nameof(key_) + ",nq}")]
		private struct DebugItem
		{
			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			private readonly string key_;
			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			private readonly T data_;
			public DebugItem(int l, int r, T data)
			{
				if (r - l == 1) {
					key_ = $"[{l}]";
				} else {
					key_ = $"[{l}-{r})";
				}

				data_ = data;
			}
		}

		private class DebugView
		{
			private readonly DualSegmentTree<T> tree_;
			public DebugView(DualSegmentTree<T> tree)
			{
				tree_ = tree;
			}

			[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
			public DebugItem[] Items
			{
				get
				{
					var items = new List<DebugItem>(tree_.Count);
					int length = tree_.n_;
					while (length > 0) {
						int unit = tree_.n_ / length;
						for (int i = 0; i < length; i++) {
							int l = i * unit;
							int r = l + unit;
							if (l < tree_.Count) {
								int dataIndex = i + length;
								items.Add(new DebugItem(
									l,
									r,
									tree_.tree_[dataIndex]));
							}
						}

						length >>= 1;
					}

					return items.ToArray();
				}
			}
		}
	}

	[DebuggerTypeProxy(typeof(LazySegmentTree<,>.DebugView))]
	public class LazySegmentTree<TData, TUpdate> : IEnumerable<TData>
	{
		private readonly int n_;
		private readonly TData[] data_;
		private readonly TUpdate[] lazy_;
		private readonly Func<TData, TData, TData> operate_;
		private readonly Func<TData, TUpdate, int, TData> update_;
		private readonly Func<TUpdate, TUpdate, TUpdate> compose_;
		private readonly TData unitData_;
		private readonly TUpdate unitUpdate_;

		public int Count { get; }

		public TData this[int index] => Query(index);

		public LazySegmentTree(
			int count,
			TData unitData,
			TUpdate unitUpdate,
			Func<TData, TData, TData> operate,
			Func<TData, TUpdate, int, TData> update,
			Func<TUpdate, TUpdate, TUpdate> compose)
		{
			Count = count;
			n_ = 1;
			while (n_ < count) {
				n_ <<= 1;
			}

			data_ = new TData[n_ << 1];
			for (int i = 0; i < data_.Length; i++) {
				data_[i] = unitData;
			}

			lazy_ = new TUpdate[n_ << 1];
			for (int i = 0; i < lazy_.Length; i++) {
				lazy_[i] = unitUpdate;
			}

			unitData_ = unitData;
			unitUpdate_ = unitUpdate;
			operate_ = operate;
			update_ = update;
			compose_ = compose;
		}

		public LazySegmentTree(
			IReadOnlyList<TData> src,
			TData unitData,
			TUpdate unitUpdate,
			Func<TData, TData, TData> operate,
			Func<TData, TUpdate, int, TData> update,
			Func<TUpdate, TUpdate, TUpdate> compose)
			: this(src.Count, unitData, unitUpdate, operate, update, compose)
		{
			for (int i = 0; i < src.Count; i++) {
				data_[i + n_] = src[i];
			}

			for (int i = n_ - 1; i > 0; --i) {
				data_[i] = operate_(data_[i << 1], data_[(i << 1) + 1]);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Update(int v, TUpdate value)
			=> Update(v, v + 1, value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Update(Range range, TUpdate value)
			=> Update(range.Start.Value, range.End.Value, value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Update(int left, int right, TUpdate value)
			=> UpdateCore(left, right, 1, 0, n_, value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void UpdateCore(int left, int right, int v, int l, int r, TUpdate value)
		{
			if (left <= l && r <= right) {
				Propagate(v, l, r, ref value);
			} else if (left < r && l < right) {
				int lc = v << 1;
				int rc = (v << 1) + 1;
				int mid = (l + r) >> 1;
				Propagate(lc, l, mid, v);
				UpdateCore(left, right, lc, l, mid, value);
				Propagate(rc, mid, r, v);
				UpdateCore(left, right, rc, mid, r, value);
				data_[v] = operate_(data_[lc], data_[rc]);
				lazy_[v] = unitUpdate_;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TData Query(int v) => Query(v, v + 1);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TData Query(Range range) => Query(range.Start.Value, range.End.Value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TData Query(int left, int right)
			=> QueryCore(left, right, 1, 0, n_);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private TData QueryCore(int left, int right, int v, int l, int r)
		{
			if (left <= l && r <= right) {
				return data_[v];
			} else if (r <= left || right <= l) {
				return unitData_;
			} else {
				int lc = v << 1;
				int rc = (v << 1) + 1;
				int mid = (l + r) >> 1;
				Propagate(lc, l, mid, v);
				Propagate(rc, mid, r, v);
				lazy_[v] = unitUpdate_;
				return operate_(
					QueryCore(left, right, lc, l, mid),
					QueryCore(left, right, rc, mid, r)
				);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int FindLeftest(Range range, Func<TData, bool> check)
			=> FindLeftest(range.Start.Value, range.End.Value, check);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int FindLeftest(int left, int right, Func<TData, bool> check)
			=> FindLeftestCore(left, right, 1, 0, n_, check);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int FindLeftestCore(int left, int right, int v, int l, int r, Func<TData, bool> check)
		{
			if (check(data_[v]) == false || r <= left || right <= l || Count <= left) {
				return right;
			} else if (v >= n_) {
				return v - n_;
			} else {
				int lc = v << 1;
				int rc = (v << 1) + 1;
				int mid = (l + r) >> 1;
				Propagate(lc, l, mid, v);
				Propagate(rc, mid, r, v);
				lazy_[v] = unitUpdate_;

				int vl = FindLeftestCore(left, right, lc, l, mid, check);
				if (vl != right) {
					return vl;
				} else {
					return FindLeftestCore(left, right, rc, mid, r, check);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int FindRightest(Range range, Func<TData, bool> check)
			=> FindRightest(range.Start.Value, range.End.Value, check);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int FindRightest(int left, int right, Func<TData, bool> check)
			=> FindRightestCore(left, right, 1, 0, n_, check);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int FindRightestCore(int left, int right, int v, int l, int r, Func<TData, bool> check)
		{
			if (check(data_[v]) == false || r <= left || right <= l || Count <= left) {
				return left - 1;
			} else if (v >= n_) {
				return v - n_;
			} else {
				int lc = v << 1;
				int rc = (v << 1) + 1;
				int mid = (l + r) >> 1;
				Propagate(lc, l, mid, v);
				Propagate(rc, mid, r, v);
				lazy_[v] = unitUpdate_;

				int vr = FindRightestCore(left, right, rc, mid, r, check);
				if (vr != left - 1) {
					return vr;
				} else {
					return FindRightestCore(left, right, lc, l, mid, check);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Propagate(int v, int l, int r, int p)
			=> Propagate(v, l, r, ref lazy_[p]);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Propagate(int v, int l, int r, ref TUpdate value)
		{
			data_[v] = update_(data_[v], value, r - l);
			lazy_[v] = compose_(value, lazy_[v]);
		}

		public IEnumerator<TData> GetEnumerator()
		{
			for (int i = 0; i < Count; i++) {
				yield return this[i];
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		[DebuggerDisplay("data= {" + nameof(data_) + "}, lazy= {" + nameof(lazy_) + "}", Name = "{" + nameof(key_) + ",nq}")]
		private struct DebugItem
		{
			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			private readonly string key_;
			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			private readonly TData data_;
			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			private readonly TUpdate lazy_;
			public DebugItem(int l, int r, TData data, TUpdate lazy)
			{
				if (r - l == 1) {
					key_ = $"[{l}]";
				} else {
					key_ = $"[{l}-{r})";
				}

				data_ = data;
				lazy_ = lazy;
			}
		}

		private class DebugView
		{
			private readonly LazySegmentTree<TData, TUpdate> tree_;
			public DebugView(LazySegmentTree<TData, TUpdate> tree)
			{
				tree_ = tree;
			}

			[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
			public DebugItem[] Items
			{
				get
				{
					var items = new List<DebugItem>(tree_.Count);
					int length = tree_.n_;
					while (length > 0) {
						int unit = tree_.n_ / length;
						for (int i = 0; i < length; i++) {
							int l = i * unit;
							int r = l + unit;
							if (l < tree_.Count) {
								int dataIndex = i + length;
								items.Add(new DebugItem(
									l,
									r,
									tree_.data_[dataIndex],
									tree_.lazy_[dataIndex]));
							}
						}

						length >>= 1;
					}

					return items.ToArray();
				}
			}
		}
	};
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace TakyTank.KyoProLib.CSharp
{
	/// <summary>セグメント木</summary>
	/// <typeparam name="T">載せるモノイドの型</typeparam>
	[DebuggerTypeProxy(typeof(SegmentTree<>.DebugView))]
	public class SegmentTree<T> : IEnumerable<T>
	{
		/// <summary>count以上になる最小の2冪の数</summary>
		/// <remarks>
		/// 木の葉の個数に等しい。
		/// </remarks>
		private readonly int _n;

		/// <summary>モノイドの単位元</summary>
		private readonly T _unit;

		/// <summary>セグ木の完全二分木に対応する配列</summary>
		/// <remarks>
		/// 親子や葉のインデックス計算を楽にするため、根を[1]に置く。
		/// そのため、[0]は使わない。
		/// </remarks>
		private readonly T[] _tree;

		/// <summary>モノイドの二項演算に対応するデリゲート</summary>
		/// <remarks>
		/// TODO このデリゲート呼び出しのオーバーヘッドのせいで、
		/// モノイドを構造体として渡す実装方式に比べてパフォーマンスが落ちる。
		/// ただ、ぱっと作るにはデリゲート方式の方が楽なので、なんとも。
		/// </remarks>
		private readonly Func<T, T, T> _operate;

		public int Count { get; }
		public T Top => _tree[1];

		public T this[int index]
		{
			get => _tree[index + _n];
			set => Update(index, value);
		}

		public SegmentTree(int count, T unit, Func<T, T, T> operate)
		{
			_operate = operate;
			_unit = unit;

			Count = count;
			_n = 1;
			while (_n < count) {
				_n <<= 1;
			}

			// 葉の数がNの完全に分岐の頂点数はN*2-1
			// つまり、[0]を使わない場合でも、2倍の長さを確保しておけば足りる。
			_tree = new T[_n << 1];
			for (int i = 0; i < _tree.Length; i++) {
				_tree[i] = unit;
			}
		}

		public SegmentTree(T[] src, T unit, Func<T, T, T> operate)
			: this(src.Length, unit, operate)
		{
			for (int i = 0; i < src.Length; ++i) {
				_tree[i + _n] = src[i];
			}

			for (int i = _n - 1; i > 0; --i) {
				_tree[i] = _operate(_tree[i << 1], _tree[(i << 1) | 1]);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Update(int index, T value)
		{
			if (index >= Count) {
				return;
			}

			index += _n;
			_tree[index] = value;
			index >>= 1;
			while (index != 0) {
				_tree[index] = _operate(_tree[index << 1], _tree[(index << 1) | 1]);
				index >>= 1;
			}
		}

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//public T Query(Range range) => Query(range.Start.Value, range.End.Value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Query(int left, int right)
		{
			if (left > right || right < 0 || left >= Count) {
				return _unit;
			}

			int l = left + _n;
			int r = right + _n;
			T valL = _unit;
			T valR = _unit;
			while (l < r) {
				if ((l & 1) != 0) {
					valL = _operate(valL, _tree[l]);
					++l;
				}
				if ((r & 1) != 0) {
					--r;
					valR = _operate(_tree[r], valR);
				}

				l >>= 1;
				r >>= 1;
			}

			return _operate(valL, valR);
		}

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//public int FindLeftest(Range range, Predicate<T> check)
		//	=> FindLeftest(range.Start.Value, range.End.Value, check);

		/// <summary>
		/// [left, right)の範囲で条件を満たす一番左の要素のインデックスを返す
		/// </summary>
		/// <remarks>
		/// 最終的に返すのは単一要素のインデックスだが、探索中は範囲で見るため、
		/// [left, right)の時点で条件を満たす必要がある。
		/// つまり、演算をMaxにして、一番左にあるX以上の数の位置を求めることは出来るが、
		/// 演算をSumにして、合計がX以下になる[left, Y)のYを求めることは出来ない。
		/// </remarks>
		/// <param name="left">探索範囲[L, R)のL</param>
		/// <param name="right">探索範囲[L, R)のR</param>
		/// <param name="check">
		/// 条件を満たすかを判定するデリゲート。
		/// 引数には、インデックスではなく、モノイドが渡される。
		/// </param>
		/// <returns>
		/// 条件を満たす一番左の要素のインデックス。
		/// 条件を満たす要素が無い場合はrightが返る。
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int FindLeftest(int left, int right, Predicate<T> check)
			=> FindLeftestCore(left, right, 1, 0, _n, check);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int FindLeftestCore(int left, int right, int v, int l, int r, Predicate<T> check)
		{
			if (check(_tree[v]) == false || r <= left || right <= l || Count <= left) {
				// 条件を満たさないとき、解はこの範囲よりも右にあるとする。
				return right;
			} else if (v >= _n) {
				// 葉に辿り着いたらそれが答え。
				// 葉から元の配列のインデックスに直すには、葉の数を引けばよい。
				return v - _n;
			} else {
				int lc = v << 1;
				int rc = (v << 1) | 1;
				int mid = (l + r) >> 1;
				// 左の子の範囲で条件を満たすものを見つけたら右は見る必要がない。
				// なので、先に左を見る。
				int vl = FindLeftestCore(left, right, lc, l, mid, check);
				if (vl != right) {
					return vl;
				} else {
					return FindLeftestCore(left, right, rc, mid, r, check);
				}
			}
		}

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//public int FindRightest(Range range, Predicate<T> check)
		//	=> FindRightest(range.Start.Value, range.End.Value, check);

		/// <summary>
		/// [left, right)の範囲で条件を満たす一番右の要素のインデックスを返す
		/// </summary>
		/// <remarks>
		/// 詳細はFindLeftestと同じ。
		/// </remarks>
		/// <param name="left">探索範囲[L, R)のL</param>
		/// <param name="right">探索範囲[L, R)のR</param>
		/// <param name="check">
		/// 条件を満たすかを判定するデリゲート。
		/// 引数には、インデックスではなく、モノイドが渡される。
		/// </param>
		/// <returns>
		/// 条件を満たす一番右の要素のインデックス。
		/// 条件を満たす要素が無い場合は left - 1 が返る。
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int FindRightest(int left, int right, Predicate<T> check)
			=> FindRightestCore(left, right, 1, 0, _n, check);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int FindRightestCore(int left, int right, int v, int l, int r, Predicate<T> check)
		{
			if (check(_tree[v]) == false || r <= left || right <= l || Count <= left) {
				return left - 1;
			} else if (v >= _n) {
				return v - _n;
			} else {
				int lc = v << 1;
				int rc = (v << 1) | 1;
				int mid = (l + r) >> 1;
				int vr = FindRightestCore(left, right, rc, mid, r, check);
				if (vr != left - 1) {
					return vr;
				} else {
					return FindRightestCore(left, right, lc, l, mid, check);
				}
			}
		}

		/// <summary>
		/// satisfies(operate([l], [l + 1], ..., [r - 1]))がtrueとなる最大のrを返す
		/// </summary>
		/// <param name="l">探索範囲[L, Count)のL</param>
		/// <param name="satisfies">
		/// 条件を満たすかを判定するデリゲート。
		/// 判定結果は、区間に対して単調である必要がある。
		/// 引数には、インデックスではなく、モノイドが渡される。
		/// </param>
		/// <returns>
		/// 条件を満たす最大のR。
		/// 満たさない場合はCountが返る。
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int MaxRight(int l, Predicate<T> satisfies)
		{
			if (l == Count) {
				return Count;
			}

			l += _n;
			var sum = _unit;
			do {
				while (l % 2 == 0) {
					l >>= 1;
				}

				if (satisfies(_operate(sum, _tree[l])) == false) {
					while (l < _n) {
						l <<= 1;
						var temp = _operate(sum, _tree[l]);
						if (satisfies(temp)) {
							sum = temp;
							++l;
						}
					}

					return l - _n;
				}

				sum = _operate(sum, _tree[l]);
				++l;
			} while ((l & -l) != l);

			return Count;
		}

		/// <summary>
		/// satisfies(operate([l], [l + 1], ..., [r - 1]))がtrueとなる最小のlを返す
		/// </summary>
		/// <remarks>
		/// </remarks>
		/// <param name="r">探索範囲[0, R)のR</param>
		/// <param name="satisfies">
		/// 条件を満たすかを判定するデリゲート。
		/// 判定結果は、区間に対して単調である必要がある。
		/// 引数には、インデックスではなく、モノイドが渡される。
		/// </param>
		/// <returns>
		/// 条件を満たす最小のL。
		/// 満たさない場合はRが返る、
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int MinLeft(int r, Predicate<T> satisfies)
		{
			if (r == 0) {
				return 0;
			}

			r += _n;
			var sum = _unit;
			do {
				--r;
				while (r > 1 && (r % 2) != 0) {
					r >>= 1;
				}

				if (satisfies(_operate(_tree[r], sum)) == false) {
					while (r < _n) {
						r = (r << 1) | 1;
						var temp = _operate(_tree[r], sum);
						if (satisfies(temp)) {
							sum = temp;
							--r;
						}
					}

					return r + 1 - _n;
				}

				sum = _operate(_tree[r], sum);
			} while ((r & -r) != r);

			return 0;
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
					int length = tree_._n;
					while (length > 0) {
						int unit = tree_._n / length;
						for (int i = 0; i < length; i++) {
							int l = i * unit;
							int r = l + unit;
							if (l < tree_.Count) {
								int dataIndex = i + length;
								items.Add(new DebugItem(
									l,
									r,
									tree_._tree[dataIndex]));
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
		private readonly int _n;
		private readonly int _height;
		private readonly T _unit;
		private readonly T[] _tree;
		private readonly bool[] _should;
		private readonly Func<T, T, T> _operate;

		public int Count { get; }

		public T this[int i]
		{
			get => Query(i);
			set
			{
				PropagateTopDown(i + _n);
				_tree[i + _n] = value;
			}
		}

		public DualSegmentTree(int count, T unit, Func<T, T, T> operate)
		{
			_operate = operate;
			_unit = unit;

			Count = count;
			_n = 1;
			_height = 0;
			while (_n < count) {
				_n <<= 1;
				++_height;
			}

			_tree = new T[_n << 1];
			_should = new bool[_n << 1];
			for (int i = 0; i < _tree.Length; i++) {
				_tree[i] = unit;
			}
		}

		public DualSegmentTree(IReadOnlyList<T> src, T unit, Func<T, T, T> operate)
			: this(src.Count, unit, operate)
		{
			for (int i = 0; i < src.Count; i++) {
				_tree[i + _n] = src[i];
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Query(int i)
		{
			int l = 0;
			int r = _n;
			int k = 1;
			while (r - l > 1) {
				Propagate(k);
				int m = (l + r) >> 1;
				if (i < m) {
					r = m;
					k <<= 1;
				} else {
					l = m;
					k = (k << 1) + 1;
				}
			}

			return _tree[k];
		}

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//public void Update(Range range, T value)
		//	=> Update(range.Start.Value, range.End.Value, value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Update(int left, int right, T value)
		{
			if (left > right || right < 0 || left >= Count) {
				return;
			}

			int l = left + _n;
			int r = right + _n;
			PropagateTopDown(l);
			PropagateTopDown(r - 1);
			while (l < r) {
				if ((l & 1) != 0) {
					_tree[l] = _operate(value, _tree[l]);
					_should[l] = true;
					++l;
				}

				if ((r & 1) != 0) {
					--r;
					_tree[r] = _operate(value, _tree[r]);
					_should[r] = true;
				}

				l >>= 1;
				r >>= 1;
			}
		}

		// 葉以外の部分には、区間演算の結果ではなく、遅延評価分が格納されているので、
		// FindLeftestやFindRightestは動作しない。

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void PropagateTopDown(int v)
		{
			for (int i = _height; i > 0; i--) {
				Propagate(v >> i);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Propagate(int v)
		{
			if (_should[v]) {
				if (v < _n) {
					int lc = v << 1;
					int rc = (v << 1) | 1;
					_tree[lc] = _operate(_tree[v], _tree[lc]);
					_should[lc] = true;
					_tree[rc] = _operate(_tree[v], _tree[rc]);
					_should[rc] = true;
					_tree[v] = _unit;
				}

				_should[v] = false;
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
					int length = tree_._n;
					while (length > 0) {
						int unit = tree_._n / length;
						for (int i = 0; i < length; i++) {
							int l = i * unit;
							int r = l + unit;
							if (l < tree_.Count) {
								int dataIndex = i + length;
								items.Add(new DebugItem(
									l,
									r,
									tree_._tree[dataIndex]));
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
		where TUpdate : IEquatable<TUpdate>
	{
		private readonly int _n;
		private readonly TData[] _data;
		private readonly TUpdate[] _lazy;
		private readonly Func<TData, TData, TData> _operate;
		private readonly Func<TData, TUpdate, int, TData> _update;

		/// <summary>遅延評価分を合成するデリゲート</summary>
		/// <remarks>
		/// (f, g) => h で、h(x) = (f・g)(x) となる。
		/// すなわち、第一引数の方が新しい更新分で、後に適用される。
		/// 演算がupdateの場合は、fを返せばよい。
		/// </remarks>
		private readonly Func<TUpdate, TUpdate, TUpdate> _compose;

		private readonly TData _unitData;
		private readonly TUpdate _unitUpdate;

		public int Count { get; }

		public TData this[int index]
		{
			get => Query(index);
			set
			{
				Down(index);
				_data[index + _n] = value;
				Up(index);
			}
		}

		public LazySegmentTree(
			int count,
			TData unitData,
			TUpdate unitUpdate,
			Func<TData, TData, TData> operate,
			Func<TData, TUpdate, int, TData> update,
			Func<TUpdate, TUpdate, TUpdate> compose)
		{
			Count = count;
			_n = 1;
			while (_n < count) {
				_n <<= 1;
			}

			_data = new TData[_n << 1];
			for (int i = 0; i < _data.Length; i++) {
				_data[i] = unitData;
			}

			_lazy = new TUpdate[_n << 1];
			for (int i = 0; i < _lazy.Length; i++) {
				_lazy[i] = unitUpdate;
			}

			_unitData = unitData;
			_unitUpdate = unitUpdate;
			_operate = operate;
			_update = update;
			_compose = compose;
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
				_data[i + _n] = src[i];
			}

			for (int i = _n - 1; i > 0; --i) {
				_data[i] = _operate(_data[i << 1], _data[(i << 1) | 1]);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Update(int v, TUpdate value)
			=> Update(v, v + 1, value);
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//public void Update(Range range, TUpdate value)
		//	=> Update(range.Start.Value, range.End.Value, value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Update(int left, int right, TUpdate value)
			=> UpdateCore(left, right, 1, 0, _n, value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void UpdateCore(int left, int right, int v, int l, int r, TUpdate value)
		{
			if (left <= l && r <= right) {
				Propagate(v, l, r, ref value);
			} else if (left < r && l < right) {
				int lc = v << 1;
				int rc = (v << 1) | 1;
				int mid = (l + r) >> 1;
				Propagate(lc, l, mid, v);
				UpdateCore(left, right, lc, l, mid, value);
				Propagate(rc, mid, r, v);
				UpdateCore(left, right, rc, mid, r, value);
				_data[v] = _operate(_data[lc], _data[rc]);
				_lazy[v] = _unitUpdate;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TData Query(int v) => Query(v, v + 1);
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//public TData Query(Range range) => Query(range.Start.Value, range.End.Value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TData Query(int left, int right)
			=> QueryCore(left, right, 1, 0, _n);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private TData QueryCore(int left, int right, int v, int l, int r)
		{
			if (left <= l && r <= right) {
				return _data[v];
			} else if (r <= left || right <= l) {
				return _unitData;
			} else {
				int lc = v << 1;
				int rc = (v << 1) | 1;
				int mid = (l + r) >> 1;
				Propagate(lc, l, mid, v);
				Propagate(rc, mid, r, v);
				_lazy[v] = _unitUpdate;
				return _operate(
					QueryCore(left, right, lc, l, mid),
					QueryCore(left, right, rc, mid, r)
				);
			}
		}

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//public int FindLeftest(Range range, Func<TData, bool> check)
		//	=> FindLeftest(range.Start.Value, range.End.Value, check);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int FindLeftest(int left, int right, Func<TData, bool> check)
			=> FindLeftestCore(left, right, 1, 0, _n, check);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int FindLeftestCore(int left, int right, int v, int l, int r, Func<TData, bool> check)
		{
			if (check(_data[v]) == false || r <= left || right <= l || Count <= left) {
				return right;
			} else if (v >= _n) {
				return v - _n;
			} else {
				int lc = v << 1;
				int rc = (v << 1) | 1;
				int mid = (l + r) >> 1;
				Propagate(lc, l, mid, v);
				Propagate(rc, mid, r, v);
				_lazy[v] = _unitUpdate;

				int vl = FindLeftestCore(left, right, lc, l, mid, check);
				if (vl != right) {
					return vl;
				} else {
					return FindLeftestCore(left, right, rc, mid, r, check);
				}
			}
		}

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//public int FindRightest(Range range, Func<TData, bool> check)
		//	=> FindRightest(range.Start.Value, range.End.Value, check);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int FindRightest(int left, int right, Func<TData, bool> check)
			=> FindRightestCore(left, right, 1, 0, _n, check);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int FindRightestCore(int left, int right, int v, int l, int r, Func<TData, bool> check)
		{
			if (check(_data[v]) == false || r <= left || right <= l || Count <= left) {
				return left - 1;
			} else if (v >= _n) {
				return v - _n;
			} else {
				int lc = v << 1;
				int rc = (v << 1) | 1;
				int mid = (l + r) >> 1;
				Propagate(lc, l, mid, v);
				Propagate(rc, mid, r, v);
				_lazy[v] = _unitUpdate;

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
			=> Propagate(v, l, r, ref _lazy[p]);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Propagate(int v, int l, int r, ref TUpdate value)
		{
			if (value.Equals(_unitUpdate)) {
				return;
			}

			_data[v] = _update(_data[v], value, r - l);
			_lazy[v] = _compose(value, _lazy[v]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Up(int v)
		{
			v += _n;
			v >>= 1;
			while (v > 0) {
				_data[v] = _operate(_data[v << 1], _data[(v << 1) | 1]);
				v >>= 1;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Down(int v) => DownCore(v, v + 1, 1, 0, _n);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void DownCore(int left, int right, int v, int l, int r)
		{
			if ((left <= l && r <= right) || (r <= left || right <= l)) {
				return;
			} else {
				int lc = v << 1;
				int rc = (v << 1) | 1;
				int mid = (l + r) >> 1;
				Propagate(lc, l, mid, v);
				Propagate(rc, mid, r, v);
				_lazy[v] = _unitUpdate;
				DownCore(left, right, lc, l, mid);
				DownCore(left, right, rc, mid, r);
			}
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
					int length = tree_._n;
					while (length > 0) {
						int unit = tree_._n / length;
						for (int i = 0; i < length; i++) {
							int l = i * unit;
							int r = l + unit;
							if (l < tree_.Count) {
								int dataIndex = i + length;
								items.Add(new DebugItem(
									l,
									r,
									tree_._data[dataIndex],
									tree_._lazy[dataIndex]));
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

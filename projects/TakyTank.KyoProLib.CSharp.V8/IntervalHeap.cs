using System;
using System.Runtime.CompilerServices;

namespace TakyTank.KyoProLib.CSharp.V8
{
	/// <summary>両端優先度付きキュー</summary>
	public class IntervalHeap<T> where T : IComparable<T>
	{
		/// <summary>区間ヒープ構造の本体</summary>
		/// <remarks>
		/// 木構造の各ノードに最大側と最小側のペア(区間)を持つが、これを2つのインデックスに割り当てる。
		/// すなわち、根が[0]と[1]で、左の子が[2]と[3]、右の子が[4]と[5]である。
		/// この時、各ペアの偶数の方を最大側、奇数の方を最小側とする。
		/// また、親の[最小, 最大]は子の[最小, 最大]を包含するという関係を常に保つ。
		/// それにより、偶数インデックスだけ見ると最大ヒープに、奇数インデックスだけ見ると最小ヒープになる。
		/// </remarks>
		private T[] _heap;

		public int Count { get; private set; } = 0;
		// 最大・最小の取得はO(1)
		public T Min => _heap.Length < 2 ? _heap[0] : _heap[1]; // 1要素しか無い場合は最大側に入っている。
		public T Max => _heap[0];

		public IntervalHeap() : this(0) { }

		public IntervalHeap(int capacity)
		{
			if (capacity == 0) {
				// chokudaiサーチで使うことが多く、深さ分のインスタンスが生成されるため、
				// 通常のHeapQueueに比べてデフォルトは小さめにしておく。
				capacity = 128;
			}

			_heap = new T[capacity];
		}

		public IntervalHeap(ReadOnlySpan<T> values)
			: this(values.Length)
		{
			Count = values.Length;
			values.CopyTo(_heap);
			for (int i = _heap.Length - 1; i >= 0; --i) {
				// ペアの奇数側の方が小さかったら偶数側と入れ替え
				if ((i & 1) != 0 && _heap[i - 1].CompareTo(_heap[i]) < 0) {
					(_heap[i - 1], _heap[i]) = (_heap[i], _heap[i - 1]);
				}

				int v = Down(i);
				Up(v, i);
			}
		}

		/// <summary>
		/// キューに要素を O(log N) で追加
		/// </summary>
		public void Enqueue(T x)
		{
			if (_heap.Length == Count) {
				Extend(_heap.Length * 2);
			}

			// 末尾に追加して必要なところまで上げる
			int v = Count;
			_heap[Count] = x;
			++Count;

			Up(v);
		}

		/// <summary>
		/// キューから最小値を O(log N) で取得
		/// </summary>
		public T DequeueMin()
		{
			T ret;
			int last = Count - 1;
			// 1個のときは最小値も最大ヒープ側にある。
			// Down(1)が出来ない2個のときも一緒に特別処理。
			if (Count < 3) {
				ret = _heap[last];
				--Count;
			} else {
				// 最小キューの先頭を抜き、そこに末尾の要素を持ってきた後、適切な位置に来るようにDownとUpを行う。
				// すなわちswapしてからそれぞれ処理すればよい。
				(_heap[1], _heap[last]) = (_heap[last], _heap[1]);
				ret = _heap[last];
				--Count;
				int v = Down(1);
				Up(v);
			}

			return ret;
		}

		/// <summary>
		/// キューから最大値を O(log N) で取得
		/// </summary>
		public T DequeueMax()
		{
			T ret;
			int last = Count - 1;
			// 1個のときはDown(0)が出来ないので特別処理。
			if (Count < 2) {
				ret = _heap[last];
				--Count;
			} else {
				// Minと同様。
				(_heap[0], _heap[last]) = (_heap[last], _heap[0]);
				ret = _heap[last];
				--Count;
				int v = Down(0);
				Up(v);
			}

			return ret;
		}

		/// <summary>
		/// 指定したノードを下げれるところまで下げる
		/// </summary>
		/// <param name="v">対象ノードのインデックス</param>
		/// <returns>最終的な位置のインデックス</returns>
		private int Down(int v)
		{
			// 最大ヒープと最小ヒープの折り返しは葉側で行うので、移動は下げてから上げる順序になる。
			// どちらのヒープに居るかで、最初に処理を分岐しておく。

			int n = Count;
			if ((v & 1) != 0) {
				// 最小ヒープ側
				while ((v << 1) + 1 < n) {
					// 子のうち小さい方を選択。ここで計算しているcは右の子の最小側のインデックス。
					int c = (v << 1) + 3;
					if (n <= c || _heap[c - 2].CompareTo(_heap[c]) < 0) {
						// 左の子を見ることにする。
						c -= 2;
					}

					if (c < n && _heap[c].CompareTo(_heap[v]) < 0) {
						(_heap[v], _heap[c]) = (_heap[c], _heap[v]);
						v = c;
					} else {
						break;
					}
				}
			} else {
				// 最大ヒープ側
				while ((v << 1) + 2 < n) {
					// 子のうち大きい方を選択。ここで計算しているcは右の子の最大側インデックス。
					int c = (v << 1) + 4;
					if (n <= c || _heap[c].CompareTo(_heap[c - 2]) < 0) {
						// 左の子を見ることにする。
						c -= 2;
					}

					if (c < n && _heap[v].CompareTo(_heap[c]) < 0) {
						(_heap[v], _heap[c]) = (_heap[c], _heap[v]);
						v = c;
					} else {
						break;
					}
				}
			}

			return v;
		}

		/// <summary>
		/// 指定したノードを上げれるところまで上げる
		/// </summary>
		/// <param name="v">対象ノードのインデックス</param>
		/// <param name="root">最大でどこまで上げるか。ペアのうち奇数側のインデックスを指定する必要がある。</param>
		/// <returns>最終的な位置のインデックス</returns>
		private int Up(int v, int root = 1)
		{
			// v & ~1 は v が含まれるペアのうち偶数のノードのインデックスを、
			// v | 1 は奇数のノードのインデックスを表す。
			// 偶数側の方が小さかったら入れ替える。
			if ((v | 1) < Count && _heap[v & ~1].CompareTo(_heap[v | 1]) < 0) {
				(_heap[v & ~1], _heap[v | 1]) = (_heap[v | 1], _heap[v & ~1]);
				v ^= 1;
			}

			// 最大側の親より大きかったら再帰的に上げる
			int p = Parent(v);
			while (root < v && _heap[p].CompareTo(_heap[v]) < 0) {
				(_heap[p], _heap[v]) = (_heap[v], _heap[p]);
				v = p;
				p = Parent(v);
			}

			// 最小側の親より小さかったら再帰的に上げる
			p = Parent(v) | 1;
			while (root < v && _heap[v].CompareTo(_heap[p]) < 0) {
				(_heap[p], _heap[v]) = (_heap[v], _heap[p]);
				v = p;
				p = Parent(v) | 1;
			}

			return v;
		}

		/// <summary>偶数側(最大側)の親のインデックスを返す</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int Parent(int v) => ((v >> 1) - 1) & ~1;

		// Compare関数がinline展開されず、直後のswap等を合わせた最適化もされないため、
		// 1割程度性能が悪化してしまう。
		/*
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int Compare(int x, int y) => _heap[x].CompareTo(_heap[y]);
		*/

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Extend(int newSize)
		{
			var newHeap = new T[newSize];
			_heap.CopyTo(newHeap, 0);
			_heap = newHeap;
		}
	}

	public class IntervalHeap<TObject, TPriority> where TPriority : IComparable<TPriority>
	{
		private readonly Func<TObject, TPriority> _selector;
		private TObject[] _heap;

		public int Count { get; private set; } = 0;
		public TObject Min => _heap.Length < 2 ? _heap[0] : _heap[1];
		public TObject Max => _heap[0];

		public IntervalHeap(Func<TObject, TPriority> selector) : this(0, selector) { }

		public IntervalHeap(int capacity, Func<TObject, TPriority> selector)
		{
			if (capacity == 0) {
				capacity = 128;
			}

			_selector = selector;
			_heap = new TObject[capacity];
		}

		public IntervalHeap(ReadOnlySpan<TObject> values, Func<TObject, TPriority> selector)
			: this(values.Length, selector)
		{
			Count = values.Length;
			values.CopyTo(_heap);
			for (int i = _heap.Length - 1; i >= 0; --i) {
				if ((i & 1) != 0 && _selector(_heap[i - 1]).CompareTo(_selector(_heap[i])) < 0) {
					(_heap[i - 1], _heap[i]) = (_heap[i], _heap[i - 1]);
				}

				int v = Down(i);
				Up(v, i);
			}
		}

		public void Enqueue(TObject x)
		{
			if (_heap.Length == Count) {
				Extend(_heap.Length * 2);
			}

			int v = Count;
			_heap[Count] = x;
			++Count;

			Up(v);
		}

		public TObject DequeueMin()
		{
			TObject ret;
			int last = Count - 1;
			if (Count < 3) {
				ret = _heap[last];
				--Count;
			} else {
				(_heap[1], _heap[last]) = (_heap[last], _heap[1]);
				ret = _heap[last];
				--Count;
				int v = Down(1);
				Up(v);
			}

			return ret;
		}

		public TObject DequeueMax()
		{
			TObject ret;
			int last = Count - 1;
			if (Count < 2) {
				ret = _heap[last];
				--Count;
			} else {
				(_heap[0], _heap[last]) = (_heap[last], _heap[0]);
				ret = _heap[last];
				--Count;
				int v = Down(0);
				Up(v);
			}

			return ret;
		}

		private int Down(int v)
		{
			int n = Count;
			if ((v & 1) != 0) {
				// min heap
				while ((v << 1) + 1 < n) {
					int c = (v << 1) + 3;
					if (n <= c || _selector(_heap[c - 2]).CompareTo(_selector(_heap[c])) < 0) {
						c -= 2;
					}

					if (c < n && _selector(_heap[c]).CompareTo(_selector(_heap[v])) < 0) {
						(_heap[v], _heap[c]) = (_heap[c], _heap[v]);
						v = c;
					} else {
						break;
					}
				}
			} else {
				// max heap
				while ((v << 1) + 2 < n) {
					int c = (v << 1) + 4;
					if (n <= c || _selector(_heap[c]).CompareTo(_selector(_heap[c - 2])) < 0) {
						c -= 2;
					}

					if (c < n && _selector(_heap[v]).CompareTo(_selector(_heap[c])) < 0) {
						(_heap[v], _heap[c]) = (_heap[c], _heap[v]);
						v = c;
					} else {
						break;
					}
				}
			}

			return v;
		}

		private int Up(int v, int root = 1)
		{
			if ((v | 1) < Count && _selector(_heap[v & ~1]).CompareTo(_selector(_heap[v | 1])) < 0) {
				(_heap[v & ~1], _heap[v | 1]) = (_heap[v | 1], _heap[v & ~1]);
				v ^= 1;
			}

			int p = Parent(v);
			while (root < v && _selector(_heap[p]).CompareTo(_selector(_heap[v])) < 0) {
				// max heap
				(_heap[p], _heap[v]) = (_heap[v], _heap[p]);
				v = p;
				p = Parent(v);
			}

			p = Parent(v) | 1;
			while (root < v && _selector(_heap[v]).CompareTo(_selector(_heap[p])) < 0) {
				// min heap
				(_heap[p], _heap[v]) = (_heap[v], _heap[p]);
				v = p;
				p = Parent(v) | 1;
			}

			return v;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int Parent(int v) => ((v >> 1) - 1) & ~1;

		// Compare、Prirority共にinline展開されず、2割ほど性能が悪化する。
		// TODO AHCでの使用がメインなので看過出来ないため書きするが、どうにかしたい……
		/*
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int Compare(int x, int y) => Priority(x).CompareTo(Priority(y));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private TPriority Priority(int index) => _selector(_heap[index]);
		*/

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Extend(int newSize)
		{
			var newHeap = new TObject[newSize];
			_heap.CopyTo(newHeap, 0);
			_heap = newHeap;
		}
	}
}

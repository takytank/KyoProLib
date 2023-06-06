using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V8
{
	public class IntervalHeap<T> where T : IComparable<T>
	{
		private T[] _heap;

		public int Count { get; private set; } = 0;
		public T Min => _heap.Length < 2u ? _heap[0] : _heap[1];
		public T Max => _heap[0];

		public IntervalHeap() : this(0) { }

		public IntervalHeap(int capacity)
		{
			if (capacity == 0) {
				capacity = 1024;
			}

			_heap = new T[capacity];
		}

		public IntervalHeap(ReadOnlySpan<T> values)
			: this(values.Length)
		{
			Count = values.Length;
			values.CopyTo(_heap);
			for (int i = _heap.Length - 1; i >= 0; i--) {
				if ((i & 1) != 0 && _heap[i - 1].CompareTo(_heap[i]) < 0) {
					(_heap[i - 1], _heap[i]) = (_heap[i], _heap[i - 1]);
				}

				int k = Down(i);
				Up(k, i);
			}
		}

		public void Enqueue(T x)
		{
			if (_heap.Length == Count) {
				Extend(_heap.Length * 2);
			}

			int k = Count;
			_heap[Count] = x;
			++Count;

			Up(k);
		}

		public T DequeueMin()
		{
			T ret;
			int last = Count - 1;
			if (Count < 3u) {
				ret = _heap[last];
				--Count;
			} else {
				(_heap[1], _heap[last]) = (_heap[last], _heap[1]);
				ret = _heap[last];
				--Count;
				int k = Down(1);
				Up(k);
			}

			return ret;
		}

		public T DequeueMax()
		{
			T ret;
			int last = Count - 1;
			if (Count < 2u) {
				ret = _heap[last];
				--Count;
			} else {
				(_heap[0], _heap[last]) = (_heap[last], _heap[0]);
				ret = _heap[last];
				--Count;
				int k = Down(0);
				Up(k);
			}

			return ret;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int Parent(int k) => ((k >> 1) - 1) & ~1;

		private int Down(int k)
		{
			int n = Count;
			if ((k & 1) != 0) {
				// min heap
				while (2 * k + 1 < n) {
					int c = 2 * k + 3;
					if (n <= c || _heap[c - 2].CompareTo(_heap[c]) < 0) {
						c -= 2;
					}

					if (c < n && _heap[c].CompareTo(_heap[k]) < 0) {
						(_heap[k], _heap[c]) = (_heap[c], _heap[k]);
						k = c;
					} else {
						break;
					}
				}
			} else {
				// max heap
				while (2 * k + 2 < n) {
					int c = 2 * k + 4;
					if (n <= c || _heap[c].CompareTo(_heap[c - 2]) < 0) {
						c -= 2;
					}

					if (c < n && _heap[k].CompareTo(_heap[c]) < 0) {
						(_heap[k], _heap[c]) = (_heap[c], _heap[k]);
						k = c;
					} else {
						break;
					}
				}
			}
			return k;
		}

		private int Up(int k, int root = 1)
		{
			if ((k | 1) < Count && _heap[k & ~1].CompareTo(_heap[k | 1]) < 0) {
				(_heap[k & ~1], _heap[k | 1]) = (_heap[k | 1], _heap[k & ~1]);
				k ^= 1;
			}

			int p = Parent(k);
			while (root < k && _heap[p].CompareTo(_heap[k]) < 0) {
				// max heap
				(_heap[p], _heap[k]) = (_heap[k], _heap[p]);
				k = p;
				p = Parent(k);
			}

			p = Parent(k) | 1;
			while (root < k && _heap[k].CompareTo(_heap[p]) < 0) {
				// min heap
				(_heap[p], _heap[k]) = (_heap[k], _heap[p]);
				k = p;
				p = Parent(k) | 1;
			}

			return k;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Extend(int newSize)
		{
			var newHeap = new T[newSize];
			_heap.CopyTo(newHeap, 0);
			_heap = newHeap;
		}
	}
}

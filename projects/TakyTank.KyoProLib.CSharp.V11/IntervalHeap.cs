using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V11
{
	public class IntervalHeap<T> where T : IComparable<T>
	{
		private T[] _heap;

		public int Count { get; private set; } = 0;
		public T Min => _heap.Length < 2 ? _heap[0] : _heap[1];
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
			for (int i = _heap.Length - 1; i >= 0; --i) {
				if ((i & 1) != 0 && _heap[i - 1].CompareTo(_heap[i]) < 0) {
					(_heap[i - 1], _heap[i]) = (_heap[i], _heap[i - 1]);
				}

				int v = Down(i);
				Up(v, i);
			}
		}

		public void Enqueue(T x)
		{
			if (_heap.Length == Count) {
				Extend(_heap.Length * 2);
			}

			int v = Count;
			_heap[Count] = x;
			++Count;

			Up(v);
		}

		public T DequeueMin()
		{
			T ret;
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

		public T DequeueMax()
		{
			T ret;
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int Parent(int v) => ((v >> 1) - 1) & ~1;

		private int Down(int v)
		{
			int n = Count;
			if ((v & 1) != 0) {
				// min heap
				while ((v << 1) + 1 < n) {
					int c = (v << 1) + 3;
					if (n <= c || _heap[c - 2].CompareTo(_heap[c]) < 0) {
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
				// max heap
				while ((v << 1) + 2 < n) {
					int c = (v << 1) + 4;
					if (n <= c || _heap[c].CompareTo(_heap[c - 2]) < 0) {
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

		private int Up(int v, int root = 1)
		{
			if ((v | 1) < Count && _heap[v & ~1].CompareTo(_heap[v | 1]) < 0) {
				(_heap[v & ~1], _heap[v | 1]) = (_heap[v | 1], _heap[v & ~1]);
				v ^= 1;
			}

			int p = Parent(v);
			while (root < v && _heap[p].CompareTo(_heap[v]) < 0) {
				// max heap
				(_heap[p], _heap[v]) = (_heap[v], _heap[p]);
				v = p;
				p = Parent(v);
			}

			p = Parent(v) | 1;
			while (root < v && _heap[v].CompareTo(_heap[p]) < 0) {
				// min heap
				(_heap[p], _heap[v]) = (_heap[v], _heap[p]);
				v = p;
				p = Parent(v) | 1;
			}

			return v;
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

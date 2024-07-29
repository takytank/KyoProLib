using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace TakyTank.KyoProLib.CSharp
{
	public class HeapQueue<T> where T : IComparable<T>
	{
		private readonly int reverseFactor_;
		private T[] heap_;

		public T Top => heap_[0];
		public int Count { get; private set; } = 0;

		public HeapQueue(bool reverses = false)
			: this(1024, reverses) { }

		public HeapQueue(
			int capacity,
			bool reverses = false)
		{
			if (capacity == 0) {
				capacity = 1024;
			}

			heap_ = new T[capacity > 0 ? capacity : 1];
			reverseFactor_ = reverses ? -1 : 1;
		}

		public void Enqueue(T item)
		{
			if (heap_.Length == Count) {
				Extend(heap_.Length * 2);
			}

			heap_[Count] = item;
			++Count;

			int c = Count - 1;
			while (c > 0) {
				int p = (c - 1) >> 1;
				if (Compare(heap_[p], item) < 0) {
					heap_[c] = heap_[p];
					c = p;
				} else {
					break;
				}
			}

			heap_[c] = item;
		}

		public T Dequeue()
		{
			T ret = heap_[0];
			int n = Count - 1;

			var item = heap_[n];
			int p = 0;
			int c = (p << 1) + 1;
			while (c < n) {
				if (c != n - 1 && Compare(heap_[c + 1], heap_[c]) > 0) {
					++c;
				}

				if (Compare(item, heap_[c]) < 0) {
					heap_[p] = heap_[c];
					p = c;
					c = (p << 1) + 1;
				} else {
					break;
				}
			}

			heap_[p] = item;
			Count--;

			return ret;
		}

		public void Clear() => Count = 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int Compare(T x, T y)
			=> x.CompareTo(y) * reverseFactor_;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Extend(int newSize)
		{
			var newHeap = new T[newSize];
			heap_.CopyTo(newHeap, 0);
			heap_ = newHeap;
		}
	}

	public struct PriorityPair<TPriority, TItem> : IComparable<PriorityPair<TPriority, TItem>>
			where TPriority : IComparable<TPriority>
	{
		public TPriority Priority { get; set; }
		public TItem Item { get; set; }

		public PriorityPair(TPriority priority, TItem item)
		{
			Priority = priority;
			Item = item;
		}

		public int CompareTo(PriorityPair<TPriority, TItem> target)
			=> Priority.CompareTo(target.Priority);

		public override string ToString()
			=> $"{Priority} {Item}";
	}

	public class HeapQueueP<TPriority, TItem> where TPriority : IComparable<TPriority>
	{
		private readonly int reverseFactor_;
		private PriorityPair<TPriority, TItem>[] heap_;

		public PriorityPair<TPriority, TItem> Top => heap_[0];
		public int Count { get; private set; } = 0;

		public HeapQueueP(bool reverses = false)
			: this(1024, reverses) { }

		public HeapQueueP(
			int capacity,
			bool reverses = false)
		{
			if (capacity == 0) {
				capacity = 1024;
			}

			heap_ = new PriorityPair<TPriority, TItem>[capacity > 0 ? capacity : 1];
			reverseFactor_ = reverses ? -1 : 1;
		}

		public void Enqueue(TPriority priority, TItem item)
		{
			var pair = new PriorityPair<TPriority, TItem>(priority, item);
			if (heap_.Length == Count) {
				Extend(heap_.Length * 2);
			}

			heap_[Count] = pair;
			++Count;

			int c = Count - 1;
			while (c > 0) {
				int p = (c - 1) >> 1;
				if (Compare(heap_[p].Priority, priority) < 0) {
					heap_[c] = heap_[p];
					c = p;
				} else {
					break;
				}
			}

			heap_[c] = pair;
		}

		public PriorityPair<TPriority, TItem> Dequeue()
		{
			PriorityPair<TPriority, TItem> ret = heap_[0];
			int n = Count - 1;

			var item = heap_[n];
			int p = 0;
			int c = (p << 1) + 1;
			while (c < n) {
				if (c != n - 1 && Compare(heap_[c + 1].Priority, heap_[c].Priority) > 0) {
					++c;
				}

				if (Compare(item.Priority, heap_[c].Priority) < 0) {
					heap_[p] = heap_[c];
					p = c;
					c = (p << 1) + 1;
				} else {
					break;
				}
			}

			heap_[p] = item;
			Count--;

			return ret;
		}

		public void Clear() => Count = 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int Compare(TPriority x, TPriority y)
			=> x.CompareTo(y) * reverseFactor_;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Extend(int newSize)
		{
			var newHeap = new PriorityPair<TPriority, TItem>[newSize];
			heap_.CopyTo(newHeap, 0);
			heap_ = newHeap;
		}
	}

	public class HeapQueueG<T>
	{
		private readonly int reverseFactor_;
		private readonly Comparison<T> comparison_;
		private T[] heap_;

		public T Top => heap_[0];
		public int Count { get; private set; } = 0;

		public HeapQueueG(bool reverses = false)
			: this(Comparer<T>.Default, reverses) { }

		public HeapQueueG(int capacity, bool reverses = false)
			: this(capacity, Comparer<T>.Default.Compare, reverses) { }

		public HeapQueueG(IComparer<T> comparer, bool reverses = false)
			: this(1024, comparer.Compare, reverses) { }

		public HeapQueueG(Comparison<T> comparison, bool reverses = false)
			: this(1024, comparison, reverses) { }

		public HeapQueueG(
			int capacity,
			Comparison<T> comparison,
			bool reverses = false)
		{
			if (capacity == 0) {
				capacity = 1024;
			}

			heap_ = new T[capacity > 0 ? capacity : 1];
			comparison_ = comparison;
			reverseFactor_ = reverses ? -1 : 1;
		}

		public void Enqueue(T item)
		{
			if (heap_.Length == Count) {
				Extend(heap_.Length * 2);
			}

			heap_[Count] = item;
			++Count;

			int c = Count - 1;
			while (c > 0) {
				int p = (c - 1) >> 1;
				if (Compare(heap_[p], item) < 0) {
					heap_[c] = heap_[p];
					c = p;
				} else {
					break;
				}
			}

			heap_[c] = item;
		}

		public T Dequeue()
		{
			T ret = heap_[0];
			int n = Count - 1;

			var item = heap_[n];
			int p = 0;
			int c = (p << 1) + 1;
			while (c < n) {
				if (c != n - 1 && Compare(heap_[c + 1], heap_[c]) > 0) {
					++c;
				}

				if (Compare(item, heap_[c]) < 0) {
					heap_[p] = heap_[c];
					p = c;
					c = (p << 1) + 1;
				} else {
					break;
				}
			}

			heap_[p] = item;
			Count--;

			return ret;
		}

		public void Clear() => Count = 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int Compare(T x, T y)
			=> comparison_(x, y) * reverseFactor_;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Extend(int newSize)
		{
			var newHeap = new T[newSize];
			heap_.CopyTo(newHeap, 0);
			heap_ = newHeap;
		}
	}

	public class HeapQueue<TObject, TPriority> where TPriority : IComparable<TPriority>
	{
		private readonly Func<TObject, TPriority> selector_;
		private readonly int reverseFactory_;
		private TObject[] heap_;

		public TObject Top => heap_[0];
		public int Count { get; private set; }

		public HeapQueue(
			Func<TObject, TPriority> selector,
			bool reverses = false)
			: this(1024, selector, reverses) { }

		public HeapQueue(
			int capacity,
			Func<TObject, TPriority> selector,
			bool reverses = false)
		{
			if (capacity == 0) {
				capacity = 1024;
			}

			heap_ = new TObject[capacity];
			selector_ = selector;
			reverseFactory_ = reverses ? -1 : 1;
		}

		public void Enqueue(TObject item)
		{
			if (heap_.Length == Count) {
				Extend(heap_.Length * 2);
			}

			var priority = selector_(item);
			heap_[Count] = item;
			++Count;

			int c = Count - 1;
			while (c > 0) {
				int p = (c - 1) >> 1;
				if (Compare(Priority(p), priority) < 0) {
					heap_[c] = heap_[p];
					c = p;
				} else {
					break;
				}
			}

			heap_[c] = item;
		}

		public TObject Dequeue()
		{
			TObject ret = heap_[0];
			int n = Count - 1;

			var item = heap_[n];
			var priority = Priority(n);
			int p = 0;
			int c = (p << 1) + 1;
			while (c < n) {
				if (c != n - 1 && Compare(Priority(c + 1), Priority(c)) > 0) {
					++c;
				}

				if (Compare(priority, Priority(c)) < 0) {
					heap_[p] = heap_[c];
					p = c;
					c = (p << 1) + 1;
				} else {
					break;
				}
			}

			heap_[p] = item;
			Count--;

			return ret;
		}

		public void Clear() => Count = 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private TPriority Priority(int index)
			=> selector_(heap_[index]);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int Compare(TPriority x, TPriority y)
			=> x.CompareTo(y) * reverseFactory_;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Extend(int newSize)
		{
			var newHeap = new TObject[newSize];
			heap_.CopyTo(newHeap, 0);
			heap_ = newHeap;
		}
	}
}
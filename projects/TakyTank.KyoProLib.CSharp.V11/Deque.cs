using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V11
{
	public class Deque<T> : IEnumerable<T>
	{
		private int capacity_;
		private T[] ringBuffer_;
		private int front_;
		private int back_;

		public int Count { get; private set; } = 0;
		public T Front => ringBuffer_[front_];
		public T Back => ringBuffer_[back_ != -1 ? back_ : back_ + capacity_];

		public T this[int index]
		{
			get
			{
				int target = index + front_;
				if (target >= capacity_) {
					target -= capacity_;
				}

				return ringBuffer_[target];
			}
		}

		public Deque() : this(1024) { }
		public Deque(int capacity)
		{
			if (capacity == 0) {
				capacity = 1024;
			}

			capacity_ = capacity;
			ringBuffer_ = new T[capacity];
			front_ = 0;
			back_ = -1;
		}

		public void Clear()
		{
			front_ = 0;
			back_ = -1;
			Count = 0;
		}

		public void PushFront(T item)
		{
			if (Count == capacity_) {
				Extend(capacity_ * 2);
			}

			int index = front_ - 1;
			if (index < 0) {
				index += capacity_;
			}

			ringBuffer_[index] = item;
			front_ = index;

			Count++;
		}

		public void PushBack(T item)
		{
			if (Count == capacity_) {
				Extend(capacity_ * 2);
			}

			int index = back_ + 1;
			if (index >= capacity_) {
				index -= capacity_;
			}

			ringBuffer_[index] = item;
			back_ = index;

			Count++;
		}

		public T PopFront()
		{
			if (Count == 0) {
				return default(T);
			}

			T ret = ringBuffer_[front_];
			front_ += 1;
			if (front_ >= capacity_) {
				front_ -= capacity_;
			}

			Count--;

			return ret;
		}

		public T PopBack()
		{
			if (Count == 0) {
				return default(T);
			}

			if (back_ < 0) {
				back_ += capacity_;
			}
			T ret = ringBuffer_[back_];

			back_ -= 1;
			if (back_ < 0) {
				back_ += capacity_;
			}

			Count--;

			return ret;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Extend(int newSize)
		{
			var newBuffer = new T[newSize];
			if (Count > 0) {
				if (front_ <= back_) {
					Array.Copy(ringBuffer_, front_, newBuffer, 0, Count);
				} else {
					int prevLength = ringBuffer_.Length - front_;
					Array.Copy(ringBuffer_, front_, newBuffer, 0, prevLength);
					Array.Copy(ringBuffer_, 0, newBuffer, prevLength, back_ + 1);
				}
			}

			ringBuffer_ = newBuffer;
			capacity_ = newSize;
			front_ = 0;
			back_ = Count - 1;
		}

		public IEnumerator<T> GetEnumerator()
		{
			for (int i = 0; i < Count; i++) {
				yield return this[i];
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}

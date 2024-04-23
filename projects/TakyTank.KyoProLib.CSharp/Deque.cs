using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class Deque<T> : IEnumerable<T>
	{
		private int _capacity;
		private T[] _ringBuffer;
		private int _front;
		private int _back;

		public int Count { get; private set; } = 0;
		public T Front => _ringBuffer[_front];
		public T Back => _ringBuffer[_back != -1 ? _back : _back + _capacity];

		public T this[int index]
		{
			get
			{
				int target = index + _front;
				if (target >= _capacity) {
					target -= _capacity;
				}

				return _ringBuffer[target];
			}
		}

		public Deque() : this(1024) { }
		public Deque(int capacity)
		{
			if (capacity == 0) {
				capacity = 1024;
			}

			_capacity = capacity;
			_ringBuffer = new T[capacity];
			_front = 0;
			_back = -1;
		}

		public void Clear()
		{
			_front = 0;
			_back = -1;
			Count = 0;

		}

		public void PushFront(T item)
		{
			if (Count == _capacity) {
				Extend(_capacity * 2);
			}

			int index = _front - 1;
			if (index < 0) {
				index += _capacity;
			}

			_ringBuffer[index] = item;
			_front = index;

			Count++;
		}

		public void PushBack(T item)
		{
			if (Count == _capacity) {
				Extend(_capacity * 2);
			}

			int index = _back + 1;
			if (index >= _capacity) {
				index -= _capacity;
			}

			_ringBuffer[index] = item;
			_back = index;

			Count++;
		}

		public T PopFront()
		{
			if (Count == 0) {
				return default(T);
			}

			T ret = _ringBuffer[_front];
			_front += 1;
			if (_front >= _capacity) {
				_front -= _capacity;
			}

			Count--;

			return ret;
		}

		public T PopBack()
		{
			if (Count == 0) {
				return default(T);
			}

			if (_back < 0) {
				_back += _capacity;
			}
			T ret = _ringBuffer[_back];

			_back -= 1;
			if (_back < 0) {
				_back += _capacity;
			}

			Count--;

			return ret;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Extend(int newSize)
		{
			var newBuffer = new T[newSize];
			if (Count > 0) {
				if (_front <= _back) {
					Array.Copy(_ringBuffer, _front, newBuffer, 0, Count);
				} else {
					int prevLength = _ringBuffer.Length - _front;
					Array.Copy(_ringBuffer, _front, newBuffer, 0, prevLength);
					Array.Copy(_ringBuffer, 0, newBuffer, prevLength, _back + 1);
				}
			}

			_ringBuffer = newBuffer;
			_capacity = newSize;
			_front = 0;
			_back = Count - 1;
		}

		public IEnumerator<T> GetEnumerator()
		{
			for (int i = 0; i < Count; i++) {
				yield return this[i];
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	public class ReverseableDeque<T> : IEnumerable<T>
	{
		private int _capacity;
		private T[] _ringBuffer;
		private int _front;
		private int _back;
		private bool _reverses = false;

		public int Count { get; private set; } = 0;
		public T Front => _reverses == false
			? _ringBuffer[_front]
			: _ringBuffer[_back != -1 ? _back : _back + _capacity];
		public T Back => _reverses == false
			? _ringBuffer[_back != -1 ? _back : _back + _capacity]
			: _ringBuffer[_front];

		public T this[int index]
		{
			get
			{
				if (_reverses) {
					index = Count - 1 - index;
				}

				int target = index + _front;
				if (target >= _capacity) {
					target -= _capacity;
				}

				return _ringBuffer[target];
			}
		}

		public ReverseableDeque() : this(1024) { }
		public ReverseableDeque(int capacity)
		{
			if (capacity == 0) {
				capacity = 1024;
			}

			_capacity = capacity;
			_ringBuffer = new T[capacity];
			_front = 0;
			_back = -1;
		}

		public void Reverse()
		{
			_reverses = !_reverses;
		}

		public void Clear()
		{
			_reverses = false;
			_front = 0;
			_back = -1;
			Count = 0;
		}

		public void PushFront(T item)
		{
			if (_reverses == false) {
				PushFrontCore(item);
			} else {
				PushBackCore(item);
			}
		}

		public void PushBack(T item)
		{
			if (_reverses == false) {
				PushBackCore(item);
			} else {
				PushFrontCore(item);
			}
		}

		public T PopFront()
		{
			if (_reverses == false) {
				return PopFrontCore();
			} else {
				return PopBackCore();
			}
		}

		public T PopBack()
		{
			if (_reverses == false) {
				return PopBackCore();
			} else {
				return PopFrontCore();
			}
		}

		private void PushFrontCore(T item)
		{
			if (Count == _capacity) {
				Extend(_capacity * 2);
			}

			int index = _front - 1;
			if (index < 0) {
				index += _capacity;
			}

			_ringBuffer[index] = item;
			_front = index;

			Count++;
		}

		private void PushBackCore(T item)
		{
			if (Count == _capacity) {
				Extend(_capacity * 2);
			}

			int index = _back + 1;
			if (index >= _capacity) {
				index -= _capacity;
			}

			_ringBuffer[index] = item;
			_back = index;

			Count++;
		}

		private T PopFrontCore()
		{
			if (Count == 0) {
				return default(T);
			}

			T ret = _ringBuffer[_front];
			_front += 1;
			if (_front >= _capacity) {
				_front -= _capacity;
			}

			Count--;

			return ret;
		}

		private T PopBackCore()
		{
			if (Count == 0) {
				return default(T);
			}

			if (_back < 0) {
				_back += _capacity;
			}
			T ret = _ringBuffer[_back];

			_back -= 1;
			if (_back < 0) {
				_back += _capacity;
			}

			Count--;

			return ret;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Extend(int newSize)
		{
			var newBuffer = new T[newSize];
			if (Count > 0) {
				if (_front <= _back) {
					Array.Copy(_ringBuffer, _front, newBuffer, 0, Count);
				} else {
					int prevLength = _ringBuffer.Length - _front;
					Array.Copy(_ringBuffer, _front, newBuffer, 0, prevLength);
					Array.Copy(_ringBuffer, 0, newBuffer, prevLength, _back + 1);
				}
			}

			_ringBuffer = newBuffer;
			_capacity = newSize;
			_front = 0;
			_back = Count - 1;
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

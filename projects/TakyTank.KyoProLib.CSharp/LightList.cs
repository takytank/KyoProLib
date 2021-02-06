using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class LightList<T>
	{
		private T[] values_;
		private int count_;

		public int Count => count_;

		public T this[int index]
		{
			get => values_[index];
			set => values_[index] = value;
		}

		public LightList(int capacity = 4)
		{
			values_ = new T[capacity];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T Ref(int index) => ref values_[index];

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(T value)
		{
			if (count_ < values_.Length) {
				values_[count_] = value;
				++count_;
			} else {
				var newArray = new T[count_ * 2];
				Array.Copy(values_, newArray, count_);
				values_ = newArray;
				values_[count_] = value;
				++count_;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Remove()
		{
			if (count_ > 0) {
				--count_;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear() => count_ = 0;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Reverse() => Array.Reverse(values_, 0, count_);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Span<T> AsSpan() => values_.AsSpan().Slice(0, Count);
	}
}

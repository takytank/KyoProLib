using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CS8
{
	public class SlidingWindowAggregation<T>
	{
		private readonly Stack<T> frontValues_;
		private readonly Stack<T> frontAggregates_;
		private readonly Stack<T> backValues_;
		private readonly Func<T, T, T> merge_;

		private T backAggregated_;

		public bool IsEmpty => frontValues_.Count == 0 && backValues_.Count == 0;
		public int Count => frontValues_.Count + backValues_.Count;

		public SlidingWindowAggregation(Func<T, T, T> merge) : this(1024, merge) { }
		public SlidingWindowAggregation(int capacity, Func<T, T, T> merge)
		{
			merge_ = merge;
			frontValues_ = new Stack<T>(capacity);
			frontAggregates_ = new Stack<T>(capacity);
			backValues_ = new Stack<T>(capacity);
		}

		public T Fold()
		{
			if (frontAggregates_.Count == 0) {
				return backAggregated_;
			} else if (backValues_.Count == 0) {
				return frontAggregates_.Peek();
			} else {
				return merge_(frontAggregates_.Peek(), backAggregated_);
			}
		}

		public void Push(T value)
		{
			if (backValues_.Count == 0) {
				backValues_.Push(value);
				backAggregated_ = value;
			} else {
				backValues_.Push(value);
				backAggregated_ = merge_(backAggregated_, value);
			}
		}

		public T Pop()
		{
			Move();
			frontAggregates_.Pop();
			return frontValues_.Pop();
		}

		public T Peek()
		{
			Move();
			return frontValues_.Peek();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Move()
		{
			if (frontValues_.Count == 0) {
				var back = backValues_.Pop();
				frontValues_.Push(back);
				frontAggregates_.Push(back);
				while (backValues_.Count > 0) {
					back = backValues_.Pop();
					frontValues_.Push(back);
					frontAggregates_.Push(merge_(back, frontAggregates_.Peek()));
				}
			}
		}
	}
}

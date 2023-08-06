using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V11
{
	public class SlidingWindowAggregation<T>
	{
		private readonly Stack<T> frontAggregates_;
		private readonly Stack<T> backValues_;
		private readonly Func<T, T, T> merge_;

		private T backAggregated_;

		public bool IsEmpty => frontAggregates_.Count == 0 && backValues_.Count == 0;
		public int Count { get; private set; }

		public SlidingWindowAggregation(Func<T, T, T> merge) : this(1024, merge) { }
		public SlidingWindowAggregation(int capacity, Func<T, T, T> merge)
		{
			merge_ = merge;
			frontAggregates_ = new Stack<T>(capacity);
			backValues_ = new Stack<T>(capacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Push(T value)
		{
			if (backValues_.Count == 0) {
				backValues_.Push(value);
				backAggregated_ = value;
			} else {
				backValues_.Push(value);
				backAggregated_ = merge_(backAggregated_, value);
			}

			++Count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Pop()
		{
			Move();
			frontAggregates_.Pop();
			--Count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Move()
		{
			if (frontAggregates_.Count == 0) {
				frontAggregates_.Push(backValues_.Pop());
				while (backValues_.Count > 0) {
					frontAggregates_.Push(merge_(backValues_.Pop(), frontAggregates_.Peek()));
				}
			}
		}
	}
}

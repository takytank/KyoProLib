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
		private readonly Stack<T> backAggregates_;
		private readonly Func<T, T, T> op_;

		public bool IsEmpty => frontValues_.Count == 0 && backValues_.Count == 0;
		public int Count => frontValues_.Count + backValues_.Count;

		public SlidingWindowAggregation(Func<T, T, T> op) : this(1024, op) { }
		public SlidingWindowAggregation(int capacity, Func<T, T, T> op)
		{
			op_ = op;
			frontValues_ = new Stack<T>(capacity);
			frontAggregates_ = new Stack<T>(capacity);
			backValues_ = new Stack<T>(capacity);
			backAggregates_ = new Stack<T>(capacity);
		}

		public T Fold()
		{
			if (frontAggregates_.Count == 0) {
				return backAggregates_.Peek();
			} else if (backAggregates_.Count == 0) {
				return frontAggregates_.Peek();
			} else {
				return op_(frontAggregates_.Peek(), backAggregates_.Peek());
			}
		}

		public void Push(T x)
		{
			if (backValues_.Count == 0) {
				backValues_.Push(x);
				backAggregates_.Push(x);
			} else {
				backValues_.Push(x);
				backAggregates_.Push(op_(backAggregates_.Peek(), x));
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
				backAggregates_.Pop();
				frontValues_.Push(back);
				frontAggregates_.Push(back);
				while (backValues_.Count > 0) {
					back = backValues_.Pop();
					backAggregates_.Pop();
					frontValues_.Push(back);
					frontAggregates_.Push(op_(back, frontAggregates_.Peek()));
				}
			}
		}
	}
}

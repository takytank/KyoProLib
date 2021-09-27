using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V8
{
	public class Doubling
	{
		private readonly int n_;
		private readonly int[,] indexes_;
		private readonly int k_;

		public Doubling(
			int n,
			long maxStep)
		{
			n_ = n;
			k_ = 0;
			while (maxStep > 0) {
				++k_;
				maxStep >>= 1;
			}

			indexes_ = new int[k_, n];
		}

		public Doubling(
			int[] transitions,
			long maxStep)
			: this(transitions.Length, maxStep)
		{
			Initialize(transitions);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Initialize(int[] transitions)
			=> Initialize(x => transitions[x]);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Initialize(Func<int, int> transit)
		{
			for (int i = 0; i < n_; i++) {
				indexes_[0, i] = transit(i);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Build()
		{
			for (int i = 0; i < k_ - 1; i++) {
				for (int j = 0; j < n_; j++) {
					if (indexes_[i, j] < 0) {
						indexes_[i + 1, j] = -1;
					} else {
						indexes_[i + 1, j] = indexes_[i, indexes_[i, j]];
					}
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Query(int s, long length)
		{
			int t = s;
			for (int i = k_ - 1; i >= 0; i--) {
				if (((1 << i) & length) != 0) {
					t = indexes_[i, t];
				}
			}

			return t;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long LowerBound(int s, int t)
		{
			int current = s;
			long ret = 0;
			for (int i = k_ - 1; i >= 0; i--) {
				int temp = indexes_[i, current];
				if (temp >= 0 && temp < t) {
					current = temp;
					ret += 1L << i;
				}
			}

			return ret + 1;
		}
	}

	public class Doubling<T>
		where T : struct, IComparable<T>
	{
		private readonly int n_;
		private readonly int[,] indexes_;
		private readonly T[,] values_;
		private readonly int k_;
		private readonly T unit_;
		private readonly Func<T, T, T> merge_;

		public Doubling(
			int n,
			long maxStep,
			T unit,
			Func<T, T, T> merge)
		{
			n_ = n;
			unit_ = unit;
			merge_ = merge;
			k_ = 0;
			while (maxStep > 0) {
				++k_;
				maxStep >>= 1;
			}

			indexes_ = new int[k_, n];
			values_ = new T[k_, n];
			MemoryMarshal.CreateSpan<T>(ref values_[0, 0], values_.Length).Fill(unit);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Initialize(int[] transitions, T[] initialValues)
			=> Initialize(x => transitions[x], x => initialValues[x]);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Initialize(Func<int, int> transit, Func<int, T> initialize)
		{
			for (int i = 0; i < n_; i++) {
				indexes_[0, i] = transit(i);
				values_[0, i] = initialize(i);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Build()
		{
			for (int i = 0; i < k_ - 1; i++) {
				for (int j = 0; j < n_; j++) {
					if (indexes_[i, j] < 0) {
						indexes_[i + 1, j] = -1;
					} else {
						indexes_[i + 1, j] = indexes_[i, indexes_[i, j]];
						values_[i + 1, j] = merge_(
							values_[i, j],
							values_[i, indexes_[i, j]]);
					}
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Query(int s, long length)
		{
			int t = s;
			var ret = unit_;
			for (int i = k_ - 1; i >= 0; i--) {
				if (((1 << i) & length) != 0) {
					ret = merge_(ret, values_[i, t]);
					t = indexes_[i, t];
				}
			}

			return ret;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long FindRightest(int s, Func<T, bool> checker)
		{
			int t = s;
			long ret = 0;
			T current = unit_;
			for (int i = k_ - 1; i >= 0; i--) {
				int tempIndex = t = indexes_[i, t];
				T tempValue = merge_(current, values_[i, t]);
				if (tempIndex >= 0 && checker(tempValue)) {
					current = tempValue;
					t = tempIndex;
					ret += 1L << i;
				}
			}

			return ret;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long LowerBound(int s, T value)
		{
			int t = s;
			long ret = 0;
			T current = unit_;
			for (int i = k_ - 1; i >= 0; i--) {
				int tempIndex = t = indexes_[i, t];
				T tempValue = merge_(current, values_[i, t]);
				if (tempIndex >= 0 && tempValue.CompareTo(value) < 0) {
					current = tempValue;
					t = indexes_[i, t];
					ret += 1L << i;
				}
			}

			return ret + 1;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class JagList1<T> where T : struct
	{
		private readonly int n_;
		private readonly List<T> tempValues_;
		private T[] values_;

		public int Count => n_;
		public List<T> Raw => tempValues_;
		public T[] Values => values_;

		public JagList1(int n)
		{
			n_ = n;
			tempValues_ = new List<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(T value) => tempValues_.Add(value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Build()
		{
			values_ = tempValues_.ToArray();
		}
	}

	public class JagList2<T> where T : struct
	{
		private readonly int n_;
		private readonly List<T>[] tempValues_;
		private T[][] values_;

		public int Count => n_;
		public List<T>[] Raw => tempValues_;
		public T[][] Values => values_;
		public T[] this[int index] => values_[index];

		public JagList2(int n)
		{
			n_ = n;
			tempValues_ = new List<T>[n];
			for (int i = 0; i < n; ++i) {
				tempValues_[i] = new List<T>();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(int i, T value) => tempValues_[i].Add(value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Build()
		{
			values_ = new T[n_][];
			for (int i = 0; i < values_.Length; ++i) {
				values_[i] = tempValues_[i].ToArray();
			}
		}
	}
}

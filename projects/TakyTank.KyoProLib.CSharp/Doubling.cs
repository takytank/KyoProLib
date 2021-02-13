using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class Doubling<T>
	{
		private readonly int[,] indexes_;
		private readonly T[,] values_;
		private readonly int k_;
		private readonly T unit_;
		private readonly Func<T, T, T> merge_;

		public Doubling(
			int n,
			long m,
			Func<int, int> to,
			T unit,
			Func<int, T> initial,
			Func<T, T, T> merge)
		{
			unit_ = unit;
			merge_ = merge;
			long temp = m;
			k_ = 0;
			while (temp > 0) {
				++k_;
				temp >>= 1;
			}

			indexes_ = new int[k_, n];
			for (int i = 0; i < n; i++) {
				indexes_[0, i] = to(i);
			}

			values_ = new T[k_, n];
			for (int i = 0; i < k_; i++) {
				if (i != 0) {
					for (int j = 0; j < n; j++) {
						values_[i, j] = unit;
					}
				} else {
					for (int j = 0; j < n; j++) {
						values_[i, j] = initial(j);
					}
				}
			}

			for (int i = 1; i < k_; i++) {
				for (int j = 0; j < n; j++) {
					indexes_[i, j] = indexes_[i - 1, indexes_[i - 1, j]];
					values_[i, j] = merge(
						values_[i - 1, j],
						values_[i - 1, indexes_[i - 1, j]]);
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
	}
}

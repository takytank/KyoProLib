using System;
using System.Collections.Generic;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class Lucas
	{
		public static bool IsOdd(long n, long k)
			=> (n & k) == k;

		private readonly int p_;
		private readonly int[,] combinations_;
		public Lucas(int p, bool doesPreProcess = true)
		{
			p_ = p;
			if (doesPreProcess) {
				combinations_ = Initialize(p);
			}
		}

		private int[,] Initialize(int p)
		{
			var combinations = new int[p, p];
			combinations[0, 0] = 1;
			for (int i = 1; i < p; i++) {
				combinations[i, 0] = 1;
				for (int j = i; j > 0; j--) {
					combinations[i, j] = (combinations[i - 1, j - 1] + combinations[i - 1, j]) % p;
				}
			}

			return combinations;
		}

		public long Combination(int n, int k)
		{
			long ret = 1;
			while (n > 0) {
				int ni = n % p_;
				int ki = k % p_;
				if (combinations_ is null == false) {
					ret *= combinations_[ni, ki];
				} else {
					long temp = 1;
					for (int i = 0; i < ki; i++) {
						temp *= (ni - i);
						temp /= (i + 1);
					}

					ret *= temp % p_;
				}

				ret %= p_;
				n /= p_;
				k /= p_;
			}

			return ret;
		}
	}
}

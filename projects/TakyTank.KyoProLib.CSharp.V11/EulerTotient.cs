using System;
using System.Collections.Generic;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V11
{
	public static class EulerTotient
	{
		public static long Phi(long n)
		{
			long ret = n;
			for (long i = 2; i * i <= n; i++) {
				if (n % i == 0) {
					ret /= i;
					ret *= i - 1;
					while (n % i == 0) {
						n /= i;
					}
				}
			}

			if (n != 1) {
				ret = ret / n * (n - 1);
			}

			return ret;
		}

		public static long[] PhiTable(long n)
		{
			var table = new long[n];
			for (int i = 0; i < n; i++) {
				table[i] = i;
			}

			for (int i = 2; i < n; i++) {
				if (table[i] == i) {
					for (int j = i; j < n; j += i) {
						table[j] = table[j] / i * (i - 1);
					}
				}
			}

			return table;
		}
	}
}

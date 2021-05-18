using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public static class HadamardMatrix
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool[,] CreatePow2(int n)
			=> CreatePow2Core(Pow(2, n));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool[,] CreatePow2Core(int m)
		{
			var ret = new bool[m, m];
			if (m == 1) {
				ret[0, 0] = true;
				return ret;
			}

			int mm = m / 2;
			var temp = CreatePow2Core(mm);
			for (int i = 0; i < mm; i++) {
				for (int j = 0; j < mm; j++) {
					ret[i, j] = temp[i, j];
					ret[i + mm, j] = temp[i, j];
					ret[i, j + mm] = temp[i, j];
					ret[i + mm, j + mm] = !temp[i, j];
				}
			}

			return ret;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int Pow(int n, int k)
		{
			int ret = 1;
			int mul = n;
			while (k > 0) {
				if ((k & 1) != 0) {
					ret *= mul;
				}

				k >>= 1;
				mul *= mul;
			}

			return ret;
		}
	}
}

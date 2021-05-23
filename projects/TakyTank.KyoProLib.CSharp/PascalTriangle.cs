using System;
using System.Collections.Generic;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public static class PascalTriangle
	{
		private static long[,] c_;
		public static void Initialize(int n)
		{
			c_ = new long[n + 1, n + 1];
			for (int i = 0; i <= n; i++) {
				c_[i, 0] = 1;
				for (int j = 1; j <= i; j++) {
					c_[i, j] = (c_[i - 1, j - 1] + c_[i - 1, j]);
				}
			}
		}

		public static long Combination(long n, long k)
		{
			if (n < k || (n < 0 || k < 0)) {
				return 0;
			}

			return c_[n, k];
		}
	}

	public static class PascalTriangleRatio
	{
		private static readonly double div_ = 2;
		private static double[,] c_;
		public static void Initialize(int n)
		{
			c_ = new double[n + 1, n + 1];
			c_[0, 0] = 1;

			for (int i = 1; i <= n; i++) {
				c_[i, 0] = c_[i - 1, 0] / div_;
				for (int j = 1; j <= i; j++) {
					c_[i, j] = (c_[i - 1, j - 1] + c_[i - 1, j]) / div_;
				}
			}
		}

		public static double Combination(long n, long k)
		{
			if (n < k || (n < 0 || k < 0)) {
				return 0;
			}

			return c_[n, k];
		}
	}
}

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public static class DP
	{
		public static int LevenshteinDistance<T>(ReadOnlySpan<T> a, ReadOnlySpan<T> b)
			where T : IComparable<T>
		{
			int n = a.Length;
			int m = b.Length;

			var dp = new int[n + 1, m + 1];
			MemoryMarshal.CreateSpan(ref dp[0, 0], dp.Length).Fill(int.MaxValue);
			dp[0, 0] = 0;
			for (int i = 0; i <= n; i++) {
				for (int j = 0; j <= m; j++) {
					if (i > 0 && j > 0) {
						dp[i, j] = dp[i - 1, j - 1] + ((a[i - 1].CompareTo(b[j - 1]) == 0) ? 0 : 1);
					}

					if (i > 0) {
						dp[i, j] = Math.Min(dp[i, j], dp[i - 1, j] + 1);
					}

					if (j > 0) {
						dp[i, j] = Math.Min(dp[i, j], dp[i, j - 1] + 1);
					}
				}
			}

			return dp[n, m];
		}

		public static (int distance, List<T> edited) LevenshteinDistanceWith<T>(ReadOnlySpan<T> a, ReadOnlySpan<T> b)
			where T : IComparable<T>
		{
			int n = a.Length;
			int m = b.Length;

			var dp = new int[n + 1, m + 1];
			MemoryMarshal.CreateSpan(ref dp[0, 0], dp.Length).Fill(int.MaxValue);
			dp[0, 0] = 0;
			var prev = new int[n + 1, m + 1];
			MemoryMarshal.CreateSpan(ref prev[0, 0], dp.Length).Fill(-1);
			for (int i = 0; i <= n; i++) {
				for (int j = 0; j <= m; j++) {
					if (i > 0 && j > 0) {
						dp[i, j] = dp[i - 1, j - 1] + ((a[i - 1].CompareTo(b[j - 1]) == 0) ? 0 : 1);
						prev[i, j] = 0;
					}

					if (i > 0 && dp[i, j] > dp[i - 1, j] + 1) {
						dp[i, j] = dp[i - 1, j] + 1;
						prev[i, j] = 1;
					}

					if (j > 0 && dp[i, j] > dp[i, j - 1] + 1) {
						dp[i, j] = dp[i, j - 1] + 1;
						prev[i, j] = 2;
					}
				}
			}

			var list = new List<T>();
			int ii = n;
			int jj = m;
			while (ii > 0 && jj > 0) {
				switch (prev[ii, jj]) {
					case 0:
						list.Add(a[ii - 1]);
						--ii;
						--jj;
						break;
					case 1:
						list.Add(a[ii - 1]);
						--ii;
						break;
					case 2:
						--jj;
						break;
					default:
						break;
				}
			}

			list.Reverse();

			return (dp[n, m], list);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class TravelingSalesmanProblem
	{
		private readonly int n_;
		private readonly long inf_;
		private readonly long[,] distances_;
		public TravelingSalesmanProblem(int n, long inf = 1000000000000000L)
		{
			n_ = n;
			inf_ = inf;
			distances_ = new long[n_, n_];
			MemoryMarshal.CreateSpan<long>(ref distances_[0, 0], distances_.Length).Fill(inf);
			for (int i = 0; i < n_; i++) {
				distances_[i, i] = 0;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge(int u, int v, long distance) => distances_[u, v] = distance;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long MinDistance(int start = -1)
		{
			var dp = new long[1 << n_, n_];
			MemoryMarshal.CreateSpan<long>(ref dp[0, 0], dp.Length).Fill(inf_);
			if (start == -1) {
				for (int i = 0; i < n_; i++) {
					dp[1 << i, i] = 0;
				}
			} else {
				dp[1 << start, start] = 0;
			}

			for (int flag = 0; flag < 1 << n_; flag++) {
				for (int i = 0; i < n_; i++) {
					if ((flag & (1 << i)) == 0) {
						continue;
					}

					for (int j = 0; j < n_; j++) {
						int nextFlag = flag | (1 << j);
						if (nextFlag == flag) {
							continue;
						}

						dp[nextFlag, j] = Math.Min(
							dp[nextFlag, j],
							dp[flag, i] + distances_[i, j]);
					}
				}
			}

			long ans = inf_;
			for (int i = 0; i < n_; i++) {
				ans = Math.Min(ans, dp[(1 << n_) - 1, i]);
			}

			return ans >= inf_ ? -1 : ans;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class MaximumIndependentSet
	{
		private readonly int _n;
		private readonly long[] _ngFlags;

		public long[] Weights { get; private set; }

		public MaximumIndependentSet(int n)
		{
			_n = n;
			_ngFlags = new long[_n];

			Weights = new long[_n];
			for (int i = 0; i < _n; i++) {
				Weights[i] = 1;
			}
		}

		public void AddEdge(int u, int v)
		{
			_ngFlags[u] |= 1L << v;
			_ngFlags[v] |= 1L << u;
		}

		public long Calculate()
		{
			int n1 = _n / 2;
			int n2 = _n - n1;
			long np1 = 1L << n1;
			long np2 = 1L << n2;

			var dp = new long[np1];
			for (int i = 0; i < np1; i++) {
				bool ok = true;
				long temp = 0;
				for (int j = 0; j < n1; j++) {
					if ((i & (1 << j)) != 0) {
						temp += Weights[j];
						dp[i] = Math.Max(dp[i], dp[i ^ (1 << j)]);
						if ((i & _ngFlags[j]) != 0) {
							ok = false;
						}
					}
				}

				if (ok) {
					dp[i] = Math.Max(dp[i], temp);
				}
			}

			long ret = long.MinValue;
			for (int i = 0; i < np2; i++) {
				bool ok = true;
				long temp = 0;
				long okFlag = np1 - 1;
				long flag = ((long)i) << n1;
				for (int j = 0; j < n2; j++) {
					if ((i & (1 << j)) != 0) {
						int jj = j + n1;
						temp += Weights[jj];
						okFlag &= ~_ngFlags[jj];
						if ((flag & _ngFlags[jj]) != 0) {
							ok = false;
						}
					}
				}

				if (ok) {
					ret = Math.Max(ret, temp + dp[okFlag]);
				}
			}

			return ret;
		}
	}
}

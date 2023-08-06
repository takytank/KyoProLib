using System;
using System.Collections.Generic;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V11
{
	public class ParallelBinarySearch
	{
		public static long[] SearchNGOKLong(int n, long ng, long ok)
		{
			var ngs = new long[n];
			var oks = new long[n];
			for (int i = 0; i < n; ++i) {
				ngs[i] = ng;
				oks[i] = ok;
			}

			bool continues = true;
			while (continues) {
				continues = false;
				var targets = new List<(long mid, int i)>();
				for (int i = 0; i < n; ++i) {
					if (oks[i] - ngs[i] > 1) {
						long mid = (oks[i] + ngs[i]) >> 1;
						targets.Add((mid, i));

					}
				}

				if (targets.Count > 0) {
					continues = true;
					targets.Sort();
				}
			}

			return oks;
		}
	}
}

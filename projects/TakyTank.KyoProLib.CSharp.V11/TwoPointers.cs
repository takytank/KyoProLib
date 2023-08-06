using System;
using System.Collections.Generic;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V11
{
	public static class TwoPointers
	{
		public static void Run(
			int n,
			Func<bool> satisfies,
			Action<int, int> extendR,
			Action<int, int> shortenL)
		{
			int l = 0;
			for (int r = 0; r < n; r++) {
				extendR(l, r);

				while (satisfies() && l <= r) {
					shortenL(l, r);
					++l;
				}
			}
		}

		public static void InverseRun(
			int n,
			Func<bool> satisfies,
			Action<int, int> extendL,
			Action<int, int> shortenR)
		{
			int r = n - 1;
			for (int l = n - 1; l >= 0; l--) {
				extendL(l, r);
				while (satisfies() && l <= r) {
					shortenR(l, r);
					--r;
				}
			}
		}
	}
}

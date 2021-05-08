using System;
using System.Collections.Generic;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
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
	}
}

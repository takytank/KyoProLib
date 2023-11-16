using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TakyTank.KyoProLib.CSharp.V11
{
	public static class Combination
	{
		public static IEnumerable<long> All(int n, int r)
		{
			long bit = (1L << r) - 1;
			do {
				yield return bit;
			} while (Next(ref bit, n));
		}

		public static bool Next(ref long bit, int n)
		{
			long x = bit & -bit;
			long y = bit + x;
			bit = (((bit & ~y) / x) >> 1) | y;
			return bit < (1L << n);
		}
	}
}

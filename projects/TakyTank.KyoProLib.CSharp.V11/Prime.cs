using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V11
{
	public static class Prime
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsPrime(long value)
		{
			if (value < 2) {
				return false;
			}

			for (long i = 2; i * i <= value; i++) {
				if (value % i == 0) {
					return false;
				}
			}

			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static HashSet<long> PrimeFactor(long value)
		{
			var factors = new HashSet<long>();
			for (long i = 2; i * i <= value; ++i) {
				if (value % i == 0) {
					factors.Add(i);
					while (value % i == 0) {
						value /= i;
					}
				}
			}

			if (value != 1) {
				factors.Add(value);
			}

			return factors;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Dictionary<long, int> PrimeFactors(long value)
		{
			var factors = new Dictionary<long, int>();
			for (long i = 2; i * i <= value; ++i) {
				while (value % i == 0) {
					if (factors.ContainsKey(i) == false) {
						factors[i] = 1;
					} else {
						factors[i] += 1;
					}

					value /= i;
				}
			}

			if (value != 1) {
				factors[value] = 1;
			}

			return factors;
		}
	}
}

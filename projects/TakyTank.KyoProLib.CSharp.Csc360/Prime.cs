using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.Csc360
{
	public static class Prime
	{
		static readonly HashSet<long> smallPrimes_
			= new HashSet<long> { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37 };

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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long Mod(long x, long p)
		{
			x %= p;
			if (x < 0) {
				x += p;
			}

			return x;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static long Gcd(long a, long b)
		{
			if (b == 0) {
				return a;
			}

			return Gcd(b, a % b);
		}
	}
}

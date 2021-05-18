using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public static class Divisor
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static HashSet<long> Divisors(long value)
		{
			var divisors = new HashSet<long>();
			for (long i = 1; i * i <= value; ++i) {
				if (value % i == 0) {
					divisors.Add(i);
					if (i != value / i) {
						divisors.Add(value / i);
					}
				}
			}

			return divisors;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long Gcd(long a, long b)
		{
			if (b == 0) {
				return a;
			}

			return Gcd(b, a % b);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long Gcd(long[] values)
		{
			if (values.Length == 1) {
				return values[0];
			}

			long gcd = values[0];
			for (int i = 1; i < values.Length; ++i) {
				if (gcd == 1) {
					return gcd;
				}
				gcd = Gcd(values[i], gcd);
			}

			return gcd;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long Lcm(long a, long b)
		{
			return a / Gcd(a, b) * b;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long Lcm(long[] values, long limit)
		{
			if (values.Length == 1) {
				return values[0];
			}

			long lcm = values[0];
			for (int i = 1; i < values.Length; i++) {
				lcm = Lcm(lcm, values[i]);
				if (lcm > limit || lcm < 0) {
					return -1;
				}
			}

			return lcm;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static List<long> PrimeFactorsToDivisors(
			Dictionary<long, int> factors,
			bool sorts = false)
		{
			var count = factors.Keys.Count;
			var divisors = new List<long>();
			if (count == 0) {
				return divisors;
			}

			var keys = factors.Keys.ToArray();

			void Dfs(int c, long v)
			{
				if (c == count) {
					divisors.Add(v);
					return;
				}

				Dfs(c + 1, v);
				for (int i = 0; i < factors[keys[c]]; i++) {
					v *= keys[c];
					Dfs(c + 1, v);
				}
			}

			Dfs(0, 1);

			if (sorts) {
				divisors.Sort();
			}

			return divisors;
		}
	}
}

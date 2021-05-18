using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.Core31
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

		static readonly long[] sprpBase1 = { 126401071349994536 };
		static readonly long[] sprpBase2 = { 336781006125, 9639812373923155 };
		static readonly long[] sprpBase3 = { 2, 2570940, 211991001, 3749873356 };
		static readonly long[] sprpBase4 = { 2, 325, 9375, 28178, 450775, 9780504, 1795265022 };
		public static bool IsPrimeByMillerRabin(long value)
		{
			if (value < 2) {
				return false;
			}

			if (smallPrimes_.Contains(value)) {
				return true;
			}

			long d = value - 1;
			int count2 = 0;
			while (d % 2 == 0) {
				d /= 2;
				count2++;
			}

			long[] v = value <= 291831L ? sprpBase1
				: value <= 1050535501L ? sprpBase2
				: value <= 47636622961201 ? sprpBase3
				: sprpBase4;

			foreach (var a in v) {
				if (a == value) {
					return true;
				}

				long temp = PowMod(a, d, value);
				if (temp == 1) {
					continue;
				}

				bool ok = true;
				for (int r = 0; r < count2; r++) {
					if (temp == value - 1) {
						ok = false;
						break;
					}

					temp = PowMod(temp, 2, value);
				}

				if (ok) {
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
		public static Dictionary<long, int> PrimeFactorsByPollardsRho(long value)
		{
			static long Next(long x, long p)
			{
				return (long)(((BigInteger)x * x + 1) % p);
			}

			static long FindFactor(long n)
			{
				if (n % 2 == 0) {
					return 2;
				}

				if (IsPrimeByMillerRabin(n)) {
					return n;
				}

				int seed = 0;
				while (true) {
					++seed;
					long x = seed % n;
					long y = Next(x, n);
					long d = 1;
					while (d == 1) {
						x = Next(x, n);
						y = Next(Next(y, n), n);
						d = Gcd(Math.Abs(x - y), n);
					}

					if (d == n) {
						continue;
					}

					return d;
				}
			}

			var ret = new Dictionary<long, int>();
			var que = new Queue<long>();
			que.Enqueue(value);
			while (que.Count > 0) {
				var target = que.Dequeue();
				if (target == 1) {
					continue;
				}

				if (IsPrimeByMillerRabin(target)) {
					if (ret.ContainsKey(target)) {
						ret[target]++;
					} else {
						ret.Add(target, 1);
					}

					continue;
				}

				long f = FindFactor(target);
				que.Enqueue(f);
				que.Enqueue(target / f);
			}

			return ret;
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
		public static long PowMod(long x, long n, long p)
		{
			if (p == 1) {
				return 0;
			}

			if (p < int.MaxValue) {
				var barrett = new BarrettReduction((uint)p);
				uint r = 1;
				uint y = (uint)Mod(x, p);
				while (0 < n) {
					if ((n & 1) != 0) {
						r = barrett.Multilply(r, y);
					}

					y = barrett.Multilply(y, y);
					n >>= 1;
				}

				return r;
			} else {
				BigInteger ret = 1;
				BigInteger mul = x % p;
				while (n != 0) {
					if ((n & 1) == 1) {
						ret = ret * mul % p;
					}
					mul = mul * mul % p;
					n >>= 1;
				}

				return (long)ret;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static long Gcd(long a, long b)
		{
			if (b == 0) {
				return a;
			}

			return Gcd(b, a % b);
		}

		public class BarrettReduction
		{
			private readonly ulong im_;
			public uint Mod { get; private set; }

			public BarrettReduction(uint m)
			{
				Mod = m;
				im_ = unchecked((ulong)-1) / m + 1;
			}

			public uint Multilply(uint a, uint b)
			{
				ulong z = a;
				z *= b;
				if (!Bmi2.X64.IsSupported) {
					return (uint)(z % Mod);
				}

				var x = Bmi2.X64.MultiplyNoFlags(z, im_);
				var v = unchecked((uint)(z - x * Mod));
				if (Mod <= v) {
					v += Mod;
				}

				return v;
			}
		}
	}
}

﻿using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;

namespace TakyTank.KyoProLib.CSharp.V8
{
	public static class PrimeEx
	{
		static readonly HashSet<long> smallPrimes_
			= new HashSet<long> { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37 };

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

			long[] v = value < 291831L ? sprpBase1
				: value < 1050535501L ? sprpBase2
				: value < 47636622961201 ? sprpBase3
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
		public static long CountPrimeLucy(long n)
		{
			if (n <= 1) {
				return 0;
			}

			long nSqrt = FloorSqrt(n);

			var larges = new long[nSqrt + 1];
			for (int i = 1; i <= nSqrt; i++) {
				larges[i] = n / i - 1;
			}

			var smalls = new long[n / nSqrt];
			for (int i = 1; i < n / nSqrt; i++) {
				smalls[i] = i - 1;
			}

			for (long p = 2; p <= nSqrt; p++) {
				if (p < smalls.Length) {
					if (smalls[p] <= smalls[p - 1]) {
						continue;
					}
				} else {
					if (larges[n / p] <= smalls[p - 1]) {
						continue;
					}
				}

				long pc = smalls[p - 1];
				long q = p * p;
				for (int i = 1; i <= nSqrt; i++) {
					if (n / i < q) {
						break;
					}

					long ip = i * p;
					long cur = (larges.Length <= ip ? smalls[n / ip] : larges[ip]) - pc;
					if (larges.Length <= i) {
						smalls[n / i] -= cur;
					} else {
						larges[i] -= cur;
					}
				}

				for (int i = smalls.Length - 1; i >= 0; i--) {
					if (i < q) {
						break;
					}

					long cur = smalls[i / p] - pc;
					smalls[i] -= cur;

				}
			}

			return larges[1];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long FloorSqrt(long value)
		{
			if (value < 0) {
				return -1;
			}

			long ok = 0;
			long ng = 3000000000;
			while (ng - ok > 1) {
				long mid = (ng + ok) / 2;
				if (mid * mid <= value) {
					ok = mid;
				} else {
					ng = mid;
				}
			}

			return ok;
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

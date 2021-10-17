using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V8
{
	public static class Modular
	{
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
		public static long InverseMod(long a, long p)
		{
			var (_, x, _) = ExtendedEuclidean(a, p);
			return Mod(x, p);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long Pow(long n, long k, long p)
		{
			if (p == 1) {
				return 0;
			}

			long ret = 1;
			long mul = n % p;
			while (k != 0) {
				if ((k & 1) == 1) {
					ret = ret * mul % p;
				}
				mul = mul * mul % p;
				k >>= 1;
			}

			return ret;
		}

		// ax + by = gcd(a, b)
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static (long gcd, long x, long y) ExtendedEuclidean(long a, long b)
		{
			if (b == 0) {
				return (a, 1, 0);
			}

			var (gcd, y, x) = ExtendedEuclidean(b, a % b);
			y -= a / b * x;
			return (gcd, x, y);
		}

		// g=gcd(a,b),xa=g(mod b)
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static (long g, long x) InverseGcd(long a, long b)
		{
			a = Mod(a, b);
			if (a == 0) {
				return (b, 0);
			}

			long s = b;
			long t = a;
			long m0 = 0;
			long m1 = 1;
			long u;
			while (true) {
				if (t == 0) {
					if (m0 < 0) {
						m0 += b / s;
					}

					return (s, m0);
				}

				u = s / t;
				s -= t * u;
				m0 -= m1 * u;

				if (s == 0) {
					if (m1 < 0) {
						m1 += b / t;
					}

					return (t, m1);
				}

				u = t / s;
				t -= s * u;
				m1 -= m0 * u;
			}
		}

		// a^x = b (mod p)
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long ModLog(long a, long b, long p, bool includesZero = true)
		{
			long gcd = 1;
			for (long i = p; i > 0; i >>= 1) {
				gcd = gcd * a % p;
			}

			gcd = Gcd(gcd, p);

			long t = includesZero ? 1 : a % p;
			long c = includesZero ? 0 : 1;
			for (; t % gcd > 0; c++) {
				if (t == b) {
					return c;
				}

				t = t * a % p;
			}

			if (b % gcd > 0) {
				return -1;
			}

			var dic = new Dictionary<long, long>();
			if (includesZero) {
				dic[1] = 0;
			}

			long sqrtP = (long)Math.Sqrt(p) + 1;

			long baby = 1;
			for (int i = 0; i < sqrtP; i++) {
				baby = baby * a % p;
				if (dic.ContainsKey(baby) == false) {
					dic[baby] = i + 1;
				}
			}

			if (dic.ContainsKey(b)) {
				return dic[b];
			}

			long giant = InverseMod(Pow(a, sqrtP, p), p);
			for (int i = 1; i <= sqrtP; i++) {
				b = b * giant % p;
				if (dic.ContainsKey(b)) {
					return dic[b] + i * sqrtP;
				}
			}

			return -1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long Garner(Span<long> values, Span<long> mods)
		{
			long coefficient = 1;
			long x = values[0] % mods[0];
			for (int i = 1; i < values.Length; i++) {
				coefficient *= mods[i - 1];
				long t = (long)((BigInteger)(values[i] - x)
					* InverseMod(coefficient, mods[i])
					% mods[i]);
				if (t < 0) {
					t += mods[i];
				}

				x += t * coefficient;
			}

			return x;
		}

		// x=y(mod p = lcm of mods) on x=valus[i](mod[i])
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static (long y, long p) ChineseRemainderTheorem(long[] values, long[] mods)
		{
			long r0 = 0, m0 = 1;
			for (int i = 0; i < mods.Length; i++) {
				long r1 = Mod(values[i], mods[i]);
				long m1 = mods[i];
				if (m0 < m1) {
					(r0, r1) = (r1, r0);
					(m0, m1) = (m1, m0);
				}
				if (m0 % m1 == 0) {
					if (r0 % m1 != r1) {
						return (0, 0);
					}

					continue;
				}
				var (g, im) = InverseGcd(m0, m1);

				long u1 = (m1 / g);
				if ((r1 - r0) % g != 0) {
					return (0, 0);
				}

				long x = (r1 - r0) / g % u1 * im % u1;
				r0 += x * m0;
				m0 *= u1;
				if (r0 < 0) {
					r0 += m0;
				}
			}

			return (r0, m0);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static long Gcd(long a, long b)
		{
			if (b == 0) {
				return a;
			}

			return Gcd(b, a % b);
		}

		/*
		// a^x = b (mod p)
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long ModLog(long a, long b, long p)
		{
			long gcd = 1;
			for (long i = p; i > 0; i >>= 1) {
				gcd = gcd * a % p;
			}

			gcd = Gcd(gcd, p);

			long t = 1;
			long c = 0;
			for (; t % gcd > 0; c++) {
				if (t == b) {
					return c;
				}

				t = t * a % p;
			}

			if (b % gcd > 0) {
				return -1;
			}

			t /= gcd;
			b /= gcd;

			long n = p / gcd;
			long h = 0;
			long giant = 1;
			for (; h * h < n; h++) {
				giant = giant * a % n;
			}

			var dic = new Dictionary<long, long>();
			dic[1] = 0;
			for (long s = 0; s < h; ++s) {
				b = b * a % n;
				if (dic.ContainsKey(b) == false) {
					dic[b] = s + 1;
				}
			}

			b = t;
			for (long s = 0; s < n;) {
				b = b * giant % n;
				s += h;
				if (dic.ContainsKey(b)) {
					return c + s - dic[b];
				}
			}

			return -1;
		}*/
	}
}

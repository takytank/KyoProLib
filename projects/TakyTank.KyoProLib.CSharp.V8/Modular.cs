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
		public static long ModLogCoprime(long a, long b, long p, bool includesZero = true)
		{
			if (p == 1) {
				return includesZero ? 0 : 1;
			}
			a %= p;
			b %= p;
			if (b == 1 && includesZero) {
				return 0;
			}

			if (a == 0) {
				return b == 0 ? 1 : -1;
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

			long giant = InverseMod(Pow(a, sqrtP, p), p);
			for (int i = 0; i <= sqrtP; i++) {
				if (dic.ContainsKey(b)) {
					return dic[b] + i * sqrtP;
				}

				b = b * giant % p;
			}

			return -1;
		}

		// a^x = b (mod p)
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long ModLogArbitary(long a, long b, long p, bool includesZero = true)
		{
			if (p == 1) {
				return includesZero ? 0 : 1;
			}

			a %= p;
			b %= p;
			long powA = includesZero ? 1 : a % p;
			int d = includesZero ? 0 : 1;
			for (long i = p; i > 0; i >>= 1) {
				if (powA == b) {
					return d;
				}

				powA = powA * a % p;
				++d;
			}

			long gcd = Gcd(powA, p);
			if (b % gcd > 0) {
				return -1;
			}

			p /= gcd;
			b *= InverseMod(powA, p);
			var ret = ModLogCoprime(a, b, p, includesZero);

			return ret == -1 ? -1 : ret + d;
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
	}
}

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public static class Integer
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long Sqrt(long value)
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
		public static long Sqrt2(long value)
		{
			long v0 = value / 2;
			if (v0 == 0) {
				return value;
			}

			long v1 = (v0 + value / v0) >> 1;
			while (v1 < v0) {
				v0 = v1;
				v1 = (v0 + value / v0) >> 1;
			}

			return v0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long Floor(long numerator, long denominator)
			=> numerator >= 0 ? numerator / denominator : (numerator - denominator + 1) / denominator;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long Ceiling(long numerator, long denominator)
			=> numerator >= 0 ? (numerator + denominator - 1) / denominator : numerator / denominator;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static decimal Sqrt(decimal x, decimal epsilon = 0.0M)
		{
			if (x < 0) {
				return -1;
			}

			decimal current = (decimal)Math.Sqrt((double)x);
			decimal previous;
			do {
				previous = current;
				if (previous == 0.0M) {
					return 0;
				}

				current = (previous + x / previous) / 2;
			}
			while (Math.Abs(previous - current) > epsilon);

			return current;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Pow(int n, int k)
			=> (int)Pow((long)n, (long)k);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long Pow(long n, long k)
		{
			long ret = 1;
			long mul = n;
			while (k > 0) {
				if ((k & 1) != 0) {
					ret *= mul;
				}

				k >>= 1;
				mul *= mul;
			}

			return ret;
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Dictionary<long, (long first, long length)> QuotientRange(long n)
		{
			var nk = new Dictionary<long, (long first, long length)>();
			long l = 1;
			while (l <= n) {
				long r = n / (n / l) + 1;
				nk[n / l] = (l, r - l);
				l = r;
			}

			return nk;
		}

		// Σ0~(n-1){floor((a + i + b) / m}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long FloorSum(long n, long m, long a, long b)
		{
			long ans = 0;
			while (true) {
				if (a >= m) {
					ans += (n - 1) * n * (a / m) / 2;
					a %= m;
				}

				if (b >= m) {
					ans += n * (b / m);
					b %= m;
				}

				long yMax = (a * n + b) / m;
				long xMax = yMax * m - b;
				if (yMax == 0) {
					return ans;
				}

				ans += (n - (xMax + a - 1) / a) * yMax;
				(n, m, a, b) = (yMax, a, m, (a - xMax % a) % a);
			}
		}
	}
}

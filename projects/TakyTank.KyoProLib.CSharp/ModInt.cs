using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public struct ModInt
	{
		//public const long P = 1000000007;
		public const long P = 998244353;
		//public const long P = 2;
		public const long ROOT = 3;

		// (924844033, 5)
		// (998244353, 3)
		// (1012924417, 5)
		// (167772161, 3)
		// (469762049, 3)
		// (1224736769, 3)

		private long value_;

		public static ModInt New(long value, bool mods) => new ModInt(value, mods);
		public ModInt(long value) => value_ = value;
		public ModInt(long value, bool mods)
		{
			if (mods) {
				value %= P;
				if (value < 0) {
					value += P;
				}
			}

			value_ = value;
		}

		public static ModInt operator +(ModInt lhs, ModInt rhs)
		{
			lhs.value_ = (lhs.value_ + rhs.value_) % P;
			return lhs;
		}
		public static ModInt operator +(long lhs, ModInt rhs)
		{
			rhs.value_ = (lhs + rhs.value_) % P;
			return rhs;
		}
		public static ModInt operator +(ModInt lhs, long rhs)
		{
			lhs.value_ = (lhs.value_ + rhs) % P;
			return lhs;
		}

		public static ModInt operator -(ModInt lhs, ModInt rhs)
		{
			lhs.value_ = (P + lhs.value_ - rhs.value_) % P;
			return lhs;
		}
		public static ModInt operator -(long lhs, ModInt rhs)
		{
			rhs.value_ = (P + lhs - rhs.value_) % P;
			return rhs;
		}
		public static ModInt operator -(ModInt lhs, long rhs)
		{
			lhs.value_ = (P + lhs.value_ - rhs) % P;
			return lhs;
		}

		public static ModInt operator *(ModInt lhs, ModInt rhs)
		{
			lhs.value_ = lhs.value_ * rhs.value_ % P;
			return lhs;
		}
		public static ModInt operator *(long lhs, ModInt rhs)
		{
			rhs.value_ = lhs * rhs.value_ % P;
			return rhs;
		}
		public static ModInt operator *(ModInt lhs, long rhs)
		{
			lhs.value_ = lhs.value_ * rhs % P;
			return lhs;
		}

		public static ModInt operator /(ModInt lhs, ModInt rhs)
			=> lhs * Inverse(rhs);

		public static implicit operator ModInt(long n) => new ModInt(n, true);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ModInt Inverse(ModInt value) => Pow(value, P - 2);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ModInt Pow(ModInt value, long k) => Pow(value.value_, k);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ModInt Pow(long value, long k)
		{
			long ret = 1;
			while (k > 0) {
				if ((k & 1) != 0) {
					ret = ret * value % P;
				}

				value = value * value % P;
				k >>= 1;
			}

			return new ModInt(ret);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long ToLong() => value_;
		public override string ToString() => value_.ToString();
	}

	public struct DModInt
	{
		public static long P { get; set; } = 1000000007;

		private long value_;

		public static DModInt New(long value, bool mods) => new DModInt(value, mods);
		public DModInt(long value) => value_ = value;
		public DModInt(long value, bool mods)
		{
			if (mods) {
				value %= P;
				if (value < 0) {
					value += P;
				}
			}

			value_ = value;
		}

		public static DModInt operator +(DModInt lhs, DModInt rhs)
		{
			lhs.value_ += rhs.value_;
			if (lhs.value_ >= P) {
				lhs.value_ -= P;
			}
			return lhs;
		}
		public static DModInt operator +(long lhs, DModInt rhs)
		{
			rhs.value_ += lhs;
			if (rhs.value_ >= P) {
				rhs.value_ -= P;
			}
			return rhs;
		}
		public static DModInt operator +(DModInt lhs, long rhs)
		{
			lhs.value_ += rhs;
			if (lhs.value_ >= P) {
				lhs.value_ -= P;
			}
			return lhs;
		}

		public static DModInt operator -(DModInt lhs, DModInt rhs)
		{
			lhs.value_ -= rhs.value_;
			if (lhs.value_ < 0) {
				lhs.value_ += P;
			}
			return lhs;
		}
		public static DModInt operator -(long lhs, DModInt rhs)
		{
			rhs.value_ -= lhs;
			if (rhs.value_ < 0) {
				rhs.value_ += P;
			}
			return rhs;
		}
		public static DModInt operator -(DModInt lhs, long rhs)
		{
			lhs.value_ -= rhs;
			if (lhs.value_ < 0) {
				lhs.value_ += P;
			}
			return lhs;
		}

		public static DModInt operator *(DModInt lhs, DModInt rhs)
			=> new DModInt(lhs.value_ * rhs.value_ % P);
		public static DModInt operator *(long lhs, DModInt rhs)
			=> new DModInt(lhs * rhs.value_ % P);
		public static DModInt operator *(DModInt lhs, long rhs)
			=> new DModInt(lhs.value_ * rhs % P);

		public static DModInt operator /(DModInt lhs, DModInt rhs)
			=> lhs * Inverse(rhs);

		public static implicit operator DModInt(long n) => new DModInt(n, true);

		public static DModInt Inverse(DModInt value)
		{
			if (Gcd(value.value_, P) != 1) {
				throw new Exception($"GCD of {value.value_} and {P} is not 1");
			}

			var (_, x, _) = ExtendedEuclidean(value.value_, P);
			x %= P;
			if (x < 0) {
				x += P;
			}

			return x;

			long Gcd(long a, long b)
			{
				if (b == 0) {
					return a;
				}

				return Gcd(b, a % b);
			}

			(long gcd, long x, long y) ExtendedEuclidean(long a, long b)
			{
				if (b == 0) {
					return (a, 1, 0);
				}

				var (gcd, yy, xx) = ExtendedEuclidean(b, a % b);
				yy -= a / b * xx;
				return (gcd, xx, yy);
			}
		}

		public static DModInt Pow(DModInt value, long k) => Pow(value.value_, k);
		public static DModInt Pow(long value, long k)
		{
			long ret = 1;
			while (k > 0) {
				if ((k & 1) != 0) {
					ret = ret * value % P;
				}

				value = value * value % P;
				k >>= 1;
			}

			return new DModInt(ret);
		}

		public long ToLong() => value_;
		public override string ToString() => value_.ToString();
	}
}

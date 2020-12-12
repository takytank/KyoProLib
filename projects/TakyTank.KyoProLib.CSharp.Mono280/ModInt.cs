using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.Mono280
{
	public struct ModInt
	{
		//public const long P = 1000000007;
		public const long P = 998244353;
		public const long ROOT = 3;

		// (924844033, 5)
		// (998244353, 3)
		// (1012924417, 5)
		// (167772161, 3)
		// (469762049, 3)
		// (1224736769, 3)

		private long value_;

		public ModInt(long value)
		{
			value_ = value;
		}

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
		{
			long exp = P - 2;
			while (exp > 0) {
				if (exp % 2 > 0) {
					lhs *= rhs;
				}

				rhs *= rhs;
				exp /= 2;
			}

			return lhs;
		}

		public static implicit operator ModInt(long n)
		{
			return new ModInt(n, true);
		}

		public static ModInt Inverse(ModInt value)
		{
			return Pow(value, P - 2);
		}

		public static ModInt Pow(ModInt value, long k)
		{
			return Pow(value.value_, k);
		}

		public static ModInt Pow(long value, long k)
		{
			long ret = 1;
			for (k %= P - 1; k > 0; k >>= 1, value = value * value % P) {
				if ((k & 1) == 1) {
					ret = ret * value % P;
				}
			}
			return new ModInt(ret);
		}

		public long ToLong()
		{
			return value_;
		}

		public override string ToString()
		{
			return value_.ToString();
		}
	}
}

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.Core31
{
	public struct DModInt
	{
		private static ulong _ip = 18446743945;
		private static long _p = 1000000007;
		public static long P
		{
			get => _p;
			set
			{
				_p = value;
				_ip = unchecked((ulong)-1) / (ulong)value + 1;
			}
		}

		private long _value;

		public static DModInt New(long value, bool mods) => new DModInt(value, mods);
		public DModInt(long value) => _value = value;
		public DModInt(long value, bool mods)
		{
			if (mods) {
				value %= P;
				if (value < 0) {
					value += P;
				}
			}

			_value = value;
		}

		public static DModInt operator +(DModInt lhs, DModInt rhs)
		{
			lhs._value += rhs._value;
			if (lhs._value >= P) {
				lhs._value -= P;
			}
			return lhs;
		}
		public static DModInt operator +(long lhs, DModInt rhs)
		{
			rhs._value += lhs;
			if (rhs._value >= P) {
				rhs._value -= P;
			}
			return rhs;
		}
		public static DModInt operator +(DModInt lhs, long rhs)
		{
			lhs._value += rhs;
			if (lhs._value >= P) {
				lhs._value -= P;
			}
			return lhs;
		}

		public static DModInt operator -(DModInt lhs, DModInt rhs)
		{
			lhs._value -= rhs._value;
			if (lhs._value < 0) {
				lhs._value += P;
			}
			return lhs;
		}
		public static DModInt operator -(long lhs, DModInt rhs)
		{
			rhs._value -= lhs;
			if (rhs._value < 0) {
				rhs._value += P;
			}
			return rhs;
		}
		public static DModInt operator -(DModInt lhs, long rhs)
		{
			lhs._value -= rhs;
			if (lhs._value < 0) {
				lhs._value += P;
			}
			return lhs;
		}

		public static DModInt operator *(DModInt lhs, DModInt rhs)
			=> new DModInt(Mul(lhs._value, rhs._value));
		public static DModInt operator *(long lhs, DModInt rhs)
			=> new DModInt(Mul(lhs, rhs._value));
		public static DModInt operator *(DModInt lhs, long rhs)
			=> new DModInt(Mul(lhs._value, rhs));

		public static DModInt operator /(DModInt lhs, DModInt rhs)
			=> lhs * Inverse(rhs);

		public static implicit operator DModInt(long n) => new DModInt(n, true);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static DModInt Inverse(DModInt value)
		{
			if (Gcd(value._value, P) != 1) {
				throw new Exception($"GCD of {value._value} and {P} is not 1");
			}

			var (_, x, _) = ExtendedEuclidean(value._value, P);
			x %= P;
			if (x < 0) {
				x += P;
			}

			return x;

			static long Gcd(long a, long b)
			{
				if (b == 0) {
					return a;
				}

				return Gcd(b, a % b);
			}

			static (long gcd, long x, long y) ExtendedEuclidean(long a, long b)
			{
				if (b == 0) {
					return (a, 1, 0);
				}

				var (gcd, y, x) = ExtendedEuclidean(b, a % b);
				y -= a / b * x;
				return (gcd, x, y);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static DModInt Pow(DModInt value, long k) => Pow(value._value, k);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static DModInt Pow(long value, long k)
		{
			long ret = 1;
			while (k > 0) {
				if ((k & 1) != 0) {
					ret = Mul(ret, value);
				}

				value = Mul(value, value);
				k >>= 1;
			}

			return new DModInt(ret);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static long Mul(long a, long b)
		{
			long z = a;
			z *= b;
			if (Bmi2.X64.IsSupported == false) {
				return (uint)(z % _p);
			}

			var x = Bmi2.X64.MultiplyNoFlags((ulong)z, _ip);
			var v = unchecked((uint)((ulong)z - x * (ulong)_p));
			if ((uint)_p <= v) {
				v += (uint)_p;
			}

			return v;
		}
		public long ToLong() => _value;
		public override string ToString() => _value.ToString();
	}
}

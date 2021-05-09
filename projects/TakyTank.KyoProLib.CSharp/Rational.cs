using System;
using System.Numerics;

namespace TakyTank.KyoProLib.CSharp
{
	public struct Rational : IEquatable<Rational>, IComparable<Rational>
	{
		private readonly int sign_;
		private long n_;
		private long d_;

		public long Numerator => n_;
		public long Denominator => d_;
		public double ToDouble() => Sign * (double)n_ / d_;
		public bool IsZero => n_ == 0;

		public int Sign => sign_;

		public Rational(long numerator, long denominator = 1)
		{
			System.Diagnostics.Debug.Assert(denominator != 0);
			if (numerator == 0) {
				sign_ = 1;
				n_ = 0;
				d_ = 1;
			} else {
				sign_ = (numerator ^ denominator) >= 0 ? 1 : -1;
				n_ = Math.Abs(numerator);
				d_ = Math.Abs(denominator);
				Reduce();
			}
		}

		public static Rational operator -(Rational x) => new Rational(-x.n_, x.d_);
		public static Rational operator +(Rational lhs, Rational rhs)
		{
			var lcm = Lcm(lhs.d_, rhs.d_);
			var l = lhs.n_ * (lcm / lhs.d_) * lhs.sign_;
			var r = rhs.n_ * (lcm / rhs.d_) * rhs.sign_;
			return new Rational(l + r, lcm);
		}
		public static Rational operator -(Rational lhs, Rational rhs)
		{
			var c = Lcm(lhs.d_, rhs.d_);
			var x1a = lhs.n_ * (c / lhs.d_) * lhs.sign_;
			var x2a = rhs.n_ * (c / rhs.d_) * rhs.sign_;
			return new Rational(x1a - x2a, c);
		}
		public static Rational operator *(Rational lhs, Rational rhs)
			=> new Rational(lhs.n_ * lhs.sign_ * rhs.n_ * rhs.sign_, lhs.d_ * rhs.d_);
		public static Rational operator /(Rational x1, Rational x2)
			=> new Rational(x1.n_ * x1.sign_ * x2.d_, x2.n_ * x2.sign_ * x1.d_);
		public static bool operator ==(Rational x1, Rational x2) => x1.Equals(x2);
		public static bool operator !=(Rational x1, Rational x2) => !x1.Equals(x2);
		public static bool operator >(Rational x1, Rational x2) => x1.CompareTo(x2) > 0;
		public static bool operator <(Rational x1, Rational x2) => x1.CompareTo(x2) < 0;
		public static bool operator >=(Rational x1, Rational x2) => x1.CompareTo(x2) >= 0;
		public static bool operator <=(Rational x1, Rational x2) => x1.CompareTo(x2) <= 0;

		public static implicit operator Rational(long n) => new Rational(n, 1);

		public Rational Inverse() => new Rational(d_ * sign_, n_);

		public bool Equals(Rational other)
			=> sign_ == other.sign_ && n_ == other.n_ && d_ == other.d_;
		public override bool Equals(object obj) => obj is Rational x && Equals(x);
		public int CompareTo(Rational other)
			=> (sign_ * n_ * other.d_).CompareTo(other.sign_ * other.n_ * d_);

		public override int GetHashCode() => HashCode.Combine(sign_, n_, d_);
		public override string ToString() => n_ == 0
			? "0"
			: $"{(sign_ >= 0 ? "" : "-")}{n_}/{d_}";

		private void Reduce()
		{
			var c = Gcd(n_, d_);
			n_ /= c;
			d_ /= c;
		}

		private static long Gcd(long m, long n)
		{
			var x1 = Math.Max(m, n);
			var x2 = Math.Min(m, n);
			while (true) {
				var mod = x1 % x2;
				if (mod == 0) {
					return x2;
				}

				x1 = x2;
				x2 = mod;
			}
		}

		private static long Lcm(long m, long n)
		{
			var gcd = Gcd(m, n);
			return m / gcd * n;
		}
	}

	public struct BigRational : IEquatable<BigRational>, IComparable<BigRational>
	{
		private readonly int sign_;
		private BigInteger n_;
		private BigInteger d_;

		public BigInteger Numerator => n_;
		public BigInteger Denominator => d_;
		public double ToDouble() => Sign * (double)n_ / (double)d_;
		public bool IsZero => n_ == 0;

		public int Sign => sign_;

		public BigRational(BigInteger numerator, BigInteger denominator)
		{
			System.Diagnostics.Debug.Assert(denominator != 0);
			if (numerator == 0) {
				sign_ = 1;
				n_ = 0;
				d_ = 1;
			} else {
				sign_ = (numerator ^ denominator) >= 0 ? 1 : -1;
				n_ = numerator >= 0 ? numerator : -numerator;
				d_ = denominator >= 0 ? denominator : -denominator;
				Reduce();
			}
		}

		public static BigRational operator -(BigRational x) => new BigRational(-x.n_, x.d_);
		public static BigRational operator +(BigRational lhs, BigRational rhs)
		{
			var lcm = Lcm(lhs.d_, rhs.d_);
			var l = lhs.n_ * (lcm / lhs.d_) * lhs.sign_;
			var r = rhs.n_ * (lcm / rhs.d_) * rhs.sign_;
			return new BigRational(l + r, lcm);
		}
		public static BigRational operator -(BigRational lhs, BigRational rhs)
		{
			var c = Lcm(lhs.d_, rhs.d_);
			var x1a = lhs.n_ * (c / lhs.d_) * lhs.sign_;
			var x2a = rhs.n_ * (c / rhs.d_) * rhs.sign_;
			return new BigRational(x1a - x2a, c);
		}
		public static BigRational operator *(BigRational lhs, BigRational rhs)
			=> new BigRational(lhs.n_ * lhs.sign_ * rhs.n_ * rhs.sign_, lhs.d_ * rhs.d_);
		public static BigRational operator /(BigRational x1, BigRational x2)
			=> new BigRational(x1.n_ * x1.sign_ * x2.d_, x2.n_ * x2.sign_ * x1.d_);
		public static bool operator ==(BigRational x1, BigRational x2) => x1.Equals(x2);
		public static bool operator !=(BigRational x1, BigRational x2) => !x1.Equals(x2);
		public static bool operator >(BigRational x1, BigRational x2) => x1.CompareTo(x2) > 0;
		public static bool operator <(BigRational x1, BigRational x2) => x1.CompareTo(x2) < 0;
		public static bool operator >=(BigRational x1, BigRational x2) => x1.CompareTo(x2) >= 0;
		public static bool operator <=(BigRational x1, BigRational x2) => x1.CompareTo(x2) <= 0;

		public static implicit operator BigRational(long n) => new BigRational(n, 1);
		public static implicit operator BigRational(BigInteger n) => new BigRational(n, 1);

		public BigRational Inverse() => new BigRational(d_ * sign_, n_);

		public bool Equals(BigRational other)
			=> sign_ == other.sign_ && n_ == other.n_ && d_ == other.d_;

		public override bool Equals(object obj) => obj is BigRational x && Equals(x);
		public int CompareTo(BigRational other)
			=> (sign_ * n_ * other.d_).CompareTo(other.sign_ * other.n_ * d_);
		public override int GetHashCode() => HashCode.Combine(sign_, n_, d_);
		public override string ToString() => n_ == 0
			? "0"
			: $"{(sign_ >= 0 ? "" : "-")}{n_}/{d_}";

		private void Reduce()
		{
			var c = Gcd(n_, d_);
			n_ /= c;
			d_ /= c;
		}

		private static BigInteger Gcd(BigInteger m, BigInteger n)
		{
			var x1 = m >= n ? m : n;
			var x2 = m <= n ? m : n;
			while (true) {
				var mod = x1 % x2;
				if (mod == 0) {
					return x2;
				}

				x1 = x2;
				x2 = mod;
			}
		}

		private static BigInteger Lcm(BigInteger m, BigInteger n)
		{
			var gcd = Gcd(m, n);
			return m / gcd * n;
		}
	}
}

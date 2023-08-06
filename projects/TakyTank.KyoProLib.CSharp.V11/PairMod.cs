using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V11
{
	public struct PairMod
	{
		public const long P = 3;

		private static PairMod[] factorial_;
		private static PairMod[] inverseFactorial_;
		private static long[] modInverses_;
		public static void InitializeFactorial(int n)
		{
			modInverses_ = new long[P + 1];
			modInverses_[1] = 1;
			for (int i = 2; i <= P; i++) {
				modInverses_[i] = (P - modInverses_[P % i] * (P / i)) % P;
			}

			factorial_ = new PairMod[n + 1];
			inverseFactorial_ = new PairMod[n + 1];
			factorial_[0] = factorial_[1] = 1;
			inverseFactorial_[0] = inverseFactorial_[1] = Inverse(1);
			for (int i = 2; i <= n; i++) {
				factorial_[i] = factorial_[i - 1] * i;
				inverseFactorial_[i] = Inverse(factorial_[i]);
			}
		}

		private long mod_;
		private long pow_;

		public PairMod(long value)
		{
			int pow = 0;
			while (value % P == 0) {
				++pow;
				value /= P;
			}

			pow_ = pow;
			mod_ = value % P;
		}

		public PairMod(long mod, long pow)
		{
			mod_ = mod;
			pow_ = pow;
		}

		public static PairMod operator *(PairMod lhs, PairMod rhs)
		{
			lhs.mod_ = lhs.mod_ * rhs.mod_ % P;
			lhs.pow_ += rhs.pow_;
			return lhs;
		}
		public static PairMod operator *(long lhs, PairMod rhs) => rhs * lhs;
		public static PairMod operator *(PairMod lhs, long rhs)
		{
			int offset = 0;
			while (rhs % P == 0) {
				++offset;
				rhs /= P;
			}

			lhs.mod_ = lhs.mod_ * rhs % P;
			lhs.pow_ += offset;
			return lhs;
		}

		public static implicit operator PairMod(long n) => new PairMod(n);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static PairMod Inverse(PairMod value)
			=> new PairMod(modInverses_[value.mod_], -1 * value.pow_);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static PairMod Pow(long n, long k)
		{
			long pow = 0;
			while (n % P == 0) {
				++pow;
				n /= P;
			}

			long mod = 1;
			while (k > 0) {
				if ((k & 1) != 0) {
					mod = mod * n % P;
				}

				n = n * n % P;
				k >>= 1;
			}

			pow *= k;

			return new PairMod(mod, pow);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static PairMod Combination(int n, int k)
			=> factorial_[n] * inverseFactorial_[k] * inverseFactorial_[n - k];

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long ToLong() => pow_ > 0 ? 0 : mod_;
		public override string ToString() => $"{mod_} {pow_}";
	}
}

using System;
using System.Collections.Generic;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public static class ModCounting
	{
		private const long p_ = ModInt.P;

		private static ModInt[] factorial_;
		private static ModInt[] inverseFactorial_;
		private static ModInt[] inverse_;
		private static ModInt[] montmort_;

		public static void InitializeFactorial(long max, bool withInverse = false)
		{
			if (withInverse) {
				factorial_ = new ModInt[max + 1];
				inverseFactorial_ = new ModInt[max + 1];
				inverse_ = new ModInt[max + 1];

				factorial_[0] = factorial_[1] = 1;
				inverseFactorial_[0] = inverseFactorial_[1] = 1;
				inverse_[1] = 1;
				for (int i = 2; i <= max; i++) {
					factorial_[i] = factorial_[i - 1] * i;
					inverse_[i] = p_ - inverse_[p_ % i] * (p_ / i);
					inverseFactorial_[i] = inverseFactorial_[i - 1] * inverse_[i];
				}
			} else {
				factorial_ = new ModInt[max + 1];
				inverseFactorial_ = new ModInt[max + 1];

				factorial_[0] = factorial_[1] = 1;
				for (int i = 2; i <= max; i++) {
					factorial_[i] = factorial_[i - 1] * i;
				}

				inverseFactorial_[max] = new ModInt(1) / factorial_[max];
				for (long i = max - 1; i >= 0; i--) {
					inverseFactorial_[i] = inverseFactorial_[i + 1] * (i + 1);
				}
			}
		}

		public static void InitializeMontmort(long max)
		{
			montmort_ = new ModInt[Math.Max(3, max + 1)];
			montmort_[0] = 1;
			montmort_[1] = 0;
			for (int i = 2; i < max + 1; i++) {
				montmort_[i] = (i - 1) * (montmort_[i - 1] + montmort_[i - 2]);
			}
		}

		public static ModInt Factorial(long n)
		{
			if (n < 0) {
				return 0;
			}

			return factorial_[n];
		}

		public static ModInt InverseFactorial(long n)
		{
			if (n < 0) {
				return 0;
			}

			return inverseFactorial_[n];
		}

		public static ModInt Inverse(long n)
		{
			if (n < 0) {
				return 0;
			}

			return inverse_[n];
		}

		public static ModInt Montmort(long n)
		{
			if (n < 0) {
				return 0;
			}

			return montmort_[n];
		}

		public static ModInt Permutation(long n, long k)
		{
			if (n < k || (n < 0 || k < 0)) {
				return 0;
			}

			return factorial_[n] * inverseFactorial_[n - k];
		}

		public static ModInt RepeatedPermutation(long n, long k)
		{
			long ret = 1;
			for (k %= p_ - 1; k > 0; k >>= 1, n = n * n % p_) {
				if ((k & 1) == 1) {
					ret = ret * n % p_;
				}
			}

			return ret;
		}

		public static ModInt Combination(long n, long k)
		{
			if (n < k || (n < 0 || k < 0)) {
				return 0;
			}

			return factorial_[n] * inverseFactorial_[k] * inverseFactorial_[n - k];
		}

		public static ModInt CombinationK(long n, long k)
		{
			ModInt ret = 1;
			for (int i = 0; i < k; i++) {
				ret *= (n - i) % p_;
				ret *= inverse_[i + 1];
			}

			return ret;
		}

		public static ModInt HomogeneousProduct(long n, long k)
		{
			if (n < 0 || k < 0) {
				return 0;
			}

			return Combination(n + k - 1, k);
		}
	}

	public static class VModCounting
	{
		private static long p_ = 1000000007;

		private static long[] factorial_;
		private static long[] inverseFactorial_;
		private static long[] inverse_;
		private static long[] montmort_;

		public static void InitializeFactorial(long max, long p, bool withInverse = false)
		{
			p_ = p;

			if (withInverse) {
				factorial_ = new long[max + 1];
				inverseFactorial_ = new long[max + 1];
				inverse_ = new long[max + 1];

				factorial_[0] = factorial_[1] = 1;
				inverseFactorial_[0] = inverseFactorial_[1] = 1;
				inverse_[1] = 1;
				for (int i = 2; i <= max; i++) {
					factorial_[i] = factorial_[i - 1] * i % p_;
					inverse_[i] = p_ - inverse_[p_ % i] * (p_ / i) % p_;
					inverseFactorial_[i] = inverseFactorial_[i - 1] * inverse_[i] % p_;
				}
			} else {
				factorial_ = new long[max + 1];
				inverseFactorial_ = new long[max + 1];

				factorial_[0] = factorial_[1] = 1;
				for (int i = 2; i <= max; i++) {
					factorial_[i] = factorial_[i - 1] * i % p_;
				}

				long value = factorial_[max];
				long k = p_ - 2;
				long temp = 1;
				for (k %= p_ - 1; k > 0; k >>= 1, value = value * value % p_) {
					if ((k & 1) == 1) {
						temp = temp * value % p_;
					}
				}

				inverseFactorial_[max] = temp;
				for (long i = max - 1; i >= 0; i--) {
					inverseFactorial_[i] = inverseFactorial_[i + 1] * (i + 1) % p_;
				}
			}
		}

		public static void InitializeMontmort(long max)
		{
			montmort_ = new long[Math.Max(3, max + 1)];
			montmort_[0] = 1;
			montmort_[1] = 0;
			for (int i = 2; i < max + 1; i++) {
				montmort_[i] = (i - 1) * (montmort_[i - 1] + montmort_[i - 2]);
			}
		}

		public static long Factorial(long n)
		{
			if (n < 0) {
				return 0;
			}

			return factorial_[n];
		}

		public static long InverseFactorial(long n)
		{
			if (n < 0) {
				return 0;
			}

			return inverseFactorial_[n];
		}

		public static long Inverse(long n)
		{
			if (n < 0) {
				return 0;
			}

			return inverse_[n];
		}

		public static long Montmort(long n)
		{
			if (n < 0) {
				return 0;
			}

			return montmort_[n];
		}

		public static long Permutation(long n, long k)
		{
			if (n < k || (n < 0 || k < 0)) {
				return 0;
			}

			return factorial_[n] * inverseFactorial_[n - k] % p_;
		}

		public static long RepeatedPermutation(long n, long k)
		{
			long ret = 1;
			for (k %= p_ - 1; k > 0; k >>= 1, n = n * n % p_) {
				if ((k & 1) == 1) {
					ret = ret * n % p_;
				}
			}

			return ret;
		}

		public static long Combination(long n, long k)
		{
			if (n < k || (n < 0 || k < 0)) {
				return 0;
			}

			return factorial_[n] * (inverseFactorial_[k] * inverseFactorial_[n - k] % p_) % p_;
		}

		public static long CombinationK(long n, long k)
		{
			long ret = 1;
			for (int i = 0; i < k; i++) {
				ret = (ret * ((n - i) % p_)) % p_;
				ret = (ret * inverse_[i + 1]) % p_;
			}

			return ret;
		}

		public static long HomogeneousProduct(long n, long k)
		{
			if (n < 0 || k < 0) {
				return 0;
			}

			return Combination(n + k - 1, k);
		}
	}
}

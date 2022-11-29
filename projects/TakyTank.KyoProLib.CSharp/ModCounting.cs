using System;
using System.Collections.Generic;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public static class ModCounting
	{
		private const long _p = ModInt.P;

		private static ModInt[] _factorial;
		private static ModInt[] _inverseFactorial;
		private static ModInt[] _inverse;
		private static ModInt[] _montmort;

		public static void InitializeFactorial(long max, bool withInverse = false)
		{
			if (withInverse) {
				_factorial = new ModInt[max + 1];
				_inverseFactorial = new ModInt[max + 1];
				_inverse = new ModInt[max + 1];

				_factorial[0] = _factorial[1] = 1;
				_inverseFactorial[0] = _inverseFactorial[1] = 1;
				_inverse[1] = 1;
				for (int i = 2; i <= max; i++) {
					_factorial[i] = _factorial[i - 1] * i;
					_inverse[i] = _p - _inverse[_p % i] * (_p / i);
					_inverseFactorial[i] = _inverseFactorial[i - 1] * _inverse[i];
				}
			} else {
				_factorial = new ModInt[max + 1];
				_inverseFactorial = new ModInt[max + 1];

				_factorial[0] = _factorial[1] = 1;
				for (int i = 2; i <= max; i++) {
					_factorial[i] = _factorial[i - 1] * i;
				}

				_inverseFactorial[max] = new ModInt(1) / _factorial[max];
				for (long i = max - 1; i >= 0; i--) {
					_inverseFactorial[i] = _inverseFactorial[i + 1] * (i + 1);
				}
			}
		}

		public static void InitializeMontmort(long max)
		{
			_montmort = new ModInt[Math.Max(3, max + 1)];
			_montmort[0] = 1;
			_montmort[1] = 0;
			for (int i = 2; i < max + 1; i++) {
				_montmort[i] = (i - 1) * (_montmort[i - 1] + _montmort[i - 2]);
			}
		}

		public static ModInt Factorial(long n)
		{
			if (n < 0) {
				return 0;
			}

			return _factorial[n];
		}

		public static ModInt InverseFactorial(long n)
		{
			if (n < 0) {
				return 0;
			}

			return _inverseFactorial[n];
		}

		public static ModInt Inverse(long n)
		{
			if (n < 0) {
				return 0;
			}

			return _inverse[n];
		}

		public static ModInt Montmort(long n)
		{
			if (n < 0) {
				return 0;
			}

			return _montmort[n];
		}

		public static ModInt Permutation(long n, long k)
		{
			if (n < k || (n < 0 || k < 0)) {
				return 0;
			}

			return _factorial[n] * _inverseFactorial[n - k];
		}

		public static ModInt RepeatedPermutation(long n, long k)
		{
			long ret = 1;
			for (k %= _p - 1; k > 0; k >>= 1, n = n * n % _p) {
				if ((k & 1) == 1) {
					ret = ret * n % _p;
				}
			}

			return ret;
		}

		public static ModInt Combination(long n, long k)
		{
			if (n < k || (n < 0 || k < 0)) {
				return 0;
			}

			return _factorial[n] * _inverseFactorial[k] * _inverseFactorial[n - k];
		}

		public static ModInt CombinationK(long n, long k)
		{
			ModInt ret = 1;
			for (int i = 0; i < k; i++) {
				ret *= (n - i) % _p;
				ret *= _inverse[i + 1];
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

		public static ModInt HomogeneousProductK(long n, long k)
		{
			if (n < 0 || k < 0) {
				return 0;
			}

			return CombinationK(n + k - 1, k);
		}

		public static ModInt Catalan(long n)
		{
			if (n < 0) {
				return 0;
			}

			return Combination(2 * n, n) * Inverse(n + 1);
		}
	}

	public static class VModCounting
	{
		private static long _p = 1000000007;

		private static long[] _factorial;
		private static long[] _inverseFactorial;
		private static long[] _inverse;
		private static long[] _montmort;

		public static void InitializeFactorial(long max, long p, bool withInverse = false)
		{
			_p = p;

			if (withInverse) {
				_factorial = new long[max + 1];
				_inverseFactorial = new long[max + 1];
				_inverse = new long[max + 1];

				_factorial[0] = _factorial[1] = 1;
				_inverseFactorial[0] = _inverseFactorial[1] = 1;
				_inverse[1] = 1;
				for (int i = 2; i <= max; i++) {
					_factorial[i] = _factorial[i - 1] * i % _p;
					_inverse[i] = _p - _inverse[_p % i] * (_p / i) % _p;
					_inverseFactorial[i] = _inverseFactorial[i - 1] * _inverse[i] % _p;
				}
			} else {
				_factorial = new long[max + 1];
				_inverseFactorial = new long[max + 1];

				_factorial[0] = _factorial[1] = 1;
				for (int i = 2; i <= max; i++) {
					_factorial[i] = _factorial[i - 1] * i % _p;
				}

				long value = _factorial[max];
				long k = _p - 2;
				long temp = 1;
				for (k %= _p - 1; k > 0; k >>= 1, value = value * value % _p) {
					if ((k & 1) == 1) {
						temp = temp * value % _p;
					}
				}

				_inverseFactorial[max] = temp;
				for (long i = max - 1; i >= 0; i--) {
					_inverseFactorial[i] = _inverseFactorial[i + 1] * (i + 1) % _p;
				}
			}
		}

		public static void InitializeMontmort(long max)
		{
			_montmort = new long[Math.Max(3, max + 1)];
			_montmort[0] = 1;
			_montmort[1] = 0;
			for (int i = 2; i < max + 1; i++) {
				_montmort[i] = (i - 1) * (_montmort[i - 1] + _montmort[i - 2]);
			}
		}

		public static long Factorial(long n)
		{
			if (n < 0) {
				return 0;
			}

			return _factorial[n];
		}

		public static long InverseFactorial(long n)
		{
			if (n < 0) {
				return 0;
			}

			return _inverseFactorial[n];
		}

		public static long Inverse(long n)
		{
			if (n < 0) {
				return 0;
			}

			return _inverse[n];
		}

		public static long Montmort(long n)
		{
			if (n < 0) {
				return 0;
			}

			return _montmort[n];
		}

		public static long Permutation(long n, long k)
		{
			if (n < k || (n < 0 || k < 0)) {
				return 0;
			}

			return _factorial[n] * _inverseFactorial[n - k] % _p;
		}

		public static long RepeatedPermutation(long n, long k)
		{
			long ret = 1;
			for (k %= _p - 1; k > 0; k >>= 1, n = n * n % _p) {
				if ((k & 1) == 1) {
					ret = ret * n % _p;
				}
			}

			return ret;
		}

		public static long Combination(long n, long k)
		{
			if (n < k || (n < 0 || k < 0)) {
				return 0;
			}

			return _factorial[n] * (_inverseFactorial[k] * _inverseFactorial[n - k] % _p) % _p;
		}

		public static long CombinationK(long n, long k)
		{
			long ret = 1;
			for (int i = 0; i < k; i++) {
				ret = (ret * ((n - i) % _p)) % _p;
				ret = (ret * _inverse[i + 1]) % _p;
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

		public static long HomogeneousProductK(long n, long k)
		{
			if (n < 0 || k < 0) {
				return 0;
			}

			return CombinationK(n + k - 1, k);
		}

		public static long Catalan(long n)
		{
			if (n < 0) {
				return 0;
			}

			return Combination(2 * n, n) * Inverse(n + 1) % _p;
		}
	}
}
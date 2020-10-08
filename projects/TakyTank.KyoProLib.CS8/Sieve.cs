using System;
using System.Collections.Generic;
using System.Text;

namespace TakyTank.KyoProLib.CS8
{
	public class Eratosthenes
	{
		public static (bool[] isPrime, List<int> primes) Sift(long n)
		{
			var isPrime = new bool[n + 1];
			isPrime.AsSpan().Fill(true);

			var primes = new List<int>((int)Math.Sqrt(n));
			for (long i = 2; i <= n; i++) {
				if (isPrime[i]) {
					primes.Add((int)i);
					for (long j = i * i; j <= n; j += i) {
						isPrime[j] = false;
					}
				}
			}

			return (isPrime, primes);
		}

		private readonly long[] primes_;
		private readonly long[] minPrimeFactors_;

		public ReadOnlySpan<long> Primes => primes_;

		public Eratosthenes(long n)
		{
			minPrimeFactors_ = new long[n + 1];
			minPrimeFactors_[0] = -1;
			minPrimeFactors_[1] = -1;

			var tempPrimes = new List<long>();
			for (long d = 2; d <= n; d++) {
				if (minPrimeFactors_[d] == 0) {
					tempPrimes.Add(d);
					minPrimeFactors_[d] = d;
					for (long j = d * d; j <= n; j += d) {
						if (minPrimeFactors_[j] == 0) {
							minPrimeFactors_[j] = d;
						}
					}
				}
			}

			primes_ = tempPrimes.ToArray();
		}

		public bool IsPrime(long n)
		{
			return minPrimeFactors_[n] == n;
		}

		public IReadOnlyList<long> CalculatePrimeFactorsOf(long n)
		{
			var factors = new List<long>();
			while (n > 1) {
				factors.Add(minPrimeFactors_[n]);
				n /= minPrimeFactors_[n];
			}

			return factors;
		}

		public Dictionary<long, int> CalculatePrimeFactors(long n)
		{
			var factors = new Dictionary<long, int>();
			var list = CalculatePrimeFactorsOf(n);
			foreach (long value in list) {
				if (factors.ContainsKey(value)) {
					factors[value]++;
				} else {
					factors[value] = 1;
				}
			}

			return factors;
		}

		public Dictionary<long, int> CalculatePrimeFactorsOfLcm(ReadOnlySpan<long> values)
		{
			var factors = new Dictionary<long, int>();
			foreach (long value in values) {
				var temp = CalculatePrimeFactors(value);
				foreach (long key in temp.Keys) {
					if (factors.ContainsKey(key)) {
						factors[key] = Math.Max(factors[key], temp[key]);
					} else {
						factors[key] = temp[key];
					}
				}
			}

			return factors;
		}
	}

	public class HSEratosthenes
	{
		private readonly int x_;
		private readonly bool[] flags_;

		public HSEratosthenes(int x)
		{
			x_ = x;
			flags_ = new bool[(x + 1) / 2];
			if (x <= 1) {
				return;
			}

			flags_.AsSpan().Fill(true);
			flags_[0] = false;

			long sqrt_x = (long)Math.Ceiling(Math.Sqrt(x) + 0.1);
			long sqrt_xi = sqrt_x / 2;
			for (long i = 1; i < sqrt_xi; ++i) {
				if (flags_[i] == false) {
					continue;
				}

				long p = 2 * i + 1;
				for (long mult = 2 * i * (i + 1); mult < flags_.Length; mult += p) {
					flags_[mult] = false;
				}
			}
		}

		public int GetPrimeCount()
		{
			if (flags_ is null) {
				return -1;
			}

			int ret = 1;
			foreach (var f in flags_) {
				if (f) {
					++ret;
				}
			}

			return ret;
		}

		public List<int> GetPrimes()
		{
			if (x_ <= 1) {
				return new List<int>();
			}

			var primes = new List<int>((int)(x_ / Math.Log(x_) * 1.1)) { 2 };
			for (int i = 3; i <= x_; i += 2) {
				if (flags_[i >> 1]) {
					primes.Add(i);
				}
			}

			return primes;
		}
	}
}

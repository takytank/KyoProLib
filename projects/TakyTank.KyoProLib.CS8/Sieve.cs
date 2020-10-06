using System;
using System.Collections.Generic;
using System.Text;

namespace TakyTank.KyoProLib.CS8
{
	public class Sieve
	{
		private readonly List<long> primes_ = new List<long>();
		private readonly long[] minFactors_;

		public IReadOnlyList<long> Primes { get { return primes_; } }

		public Sieve(long n)
		{
			minFactors_ = new long[n + 1];
			Eratosthenes(n);
		}

		public bool IsPrime(long n)
		{
			return minFactors_[n] == n;
		}

		public IReadOnlyList<long> GetPrimeFactorList(long n)
		{
			var factors = new List<long>();
			while (n > 1) {
				factors.Add(minFactors_[n]);
				n /= minFactors_[n];
			}

			return factors;
		}

		public Dictionary<long, int> GetPrimeFactors(long n)
		{
			var factors = new Dictionary<long, int>();
			var list = GetPrimeFactorList(n);
			foreach (long value in list) {
				if (factors.ContainsKey(value)) {
					factors[value]++;
				} else {
					factors[value] = 1;
				}
			}

			return factors;
		}

		public Dictionary<long, int> GetPrimeFactorsOfLcm(IReadOnlyList<long> values)
		{
			var factors = new Dictionary<long, int>();
			foreach (long value in values) {
				var temp = GetPrimeFactors(value);
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

		private void Eratosthenes(long n)
		{
			minFactors_[0] = -1;
			minFactors_[1] = -1;

			for (long i = 2; i <= n; i++) {
				if (minFactors_[i] == 0) {
					primes_.Add(i);
					minFactors_[i] = i;
					for (long j = i * i; j <= n; j += i) {
						if (minFactors_[j] == 0) {
							minFactors_[j] = i;
						}
					}
				}
			}
		}
	}
}

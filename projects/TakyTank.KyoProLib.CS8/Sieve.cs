using System;
using System.Collections.Generic;
using System.Text;

namespace TakyTank.KyoProLib.CS8
{
	public class Eratosthenes
	{
		private readonly long[] primes_;
		private readonly long[] minPrimeFactors_;

		public ReadOnlySpan<long> Primes => primes_;

		public Eratosthenes(long n)
		{
			minPrimeFactors_ = new long[n + 1];
			minPrimeFactors_[0] = -1;
			minPrimeFactors_[1] = -1;

			var tempPrimes = new List<long>();
			for (long i = 2; i <= n; i++) {
				if (minPrimeFactors_[i] == 0) {
					tempPrimes.Add(i);
					minPrimeFactors_[i] = i;
					for (long j = i * i; j <= n; j += i) {
						if (minPrimeFactors_[j] == 0) {
							minPrimeFactors_[j] = i;
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
}

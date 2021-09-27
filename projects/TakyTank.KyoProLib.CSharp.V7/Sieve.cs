using System;
using System.Collections.Generic;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V7
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

		public IReadOnlyList<long> PrimeFactorsOf(long n)
		{
			var factors = new List<long>();
			while (n > 1) {
				factors.Add(minPrimeFactors_[n]);
				n /= minPrimeFactors_[n];
			}

			return factors;
		}

		public Dictionary<long, int> PrimeFactors(long n)
		{
			var factors = new Dictionary<long, int>();
			var list = PrimeFactorsOf(n);
			foreach (long value in list) {
				if (factors.ContainsKey(value)) {
					factors[value]++;
				} else {
					factors[value] = 1;
				}
			}

			return factors;
		}

		public Dictionary<long, int> PrimeFactorsOfLcm(ReadOnlySpan<long> values)
		{
			var factors = new Dictionary<long, int>();
			foreach (long value in values) {
				var temp = PrimeFactors(value);
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

	public class LinearSeive
	{
		private readonly long[] primes_;
		private readonly long[] minPrimeFactors_;

		public ReadOnlySpan<long> Primes => primes_;

		public LinearSeive(long n)
		{
			minPrimeFactors_ = new long[n + 1];
			minPrimeFactors_[0] = -1;
			minPrimeFactors_[1] = -1;

			var tempPrimes = new List<long>();
			for (int d = 2; d <= n; d++) {
				if (minPrimeFactors_[d] == 0) {
					tempPrimes.Add(d);
					minPrimeFactors_[d] = d;
				}

				foreach (var p in tempPrimes) {
					if (p * d > n || p > minPrimeFactors_[d]) {
						break;
					}

					minPrimeFactors_[p * d] = p;
				}
			}

			primes_ = tempPrimes.ToArray();
		}
	}
}

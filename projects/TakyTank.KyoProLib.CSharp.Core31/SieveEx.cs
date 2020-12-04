using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.Core31
{
	public class HSEratosthenes
	{
		private static readonly int[] MOD30 = new int[] { 1, 7, 11, 13, 17, 19, 23, 29 };
		private static readonly int[] C1 = new int[] { 6, 4, 2, 4, 2, 4, 6, 2 };
		private static readonly int[,] C0 = new int[,] {
			{0, 0, 0, 0, 0, 0, 0, 1}, {1, 1, 1, 0, 1, 1, 1, 1},
			{2, 2, 0, 2, 0, 2, 2, 1}, {3, 1, 1, 2, 1, 1, 3, 1},
			{3, 3, 1, 2, 1, 3, 3, 1}, {4, 2, 2, 2, 2, 2, 4, 1},
			{5, 3, 1, 4, 1, 3, 5, 1}, {6, 4, 2, 4, 2, 4, 6, 1},
		};

		private static readonly byte[,] MASK = new byte[,]{
			{0xfe, 0xfd, 0xfb, 0xf7, 0xef, 0xdf, 0xbf, 0x7f},
			{0xfd, 0xdf, 0xef, 0xfe, 0x7f, 0xf7, 0xfb, 0xbf},
			{0xfb, 0xef, 0xfe, 0xbf, 0xfd, 0x7f, 0xf7, 0xdf},
			{0xf7, 0xfe, 0xbf, 0xdf, 0xfb, 0xfd, 0x7f, 0xef},
			{0xef, 0x7f, 0xfd, 0xfb, 0xdf, 0xbf, 0xfe, 0xf7},
			{0xdf, 0xf7, 0x7f, 0xfd, 0xbf, 0xfe, 0xef, 0xfb},
			{0xbf, 0xfb, 0xf7, 0x7f, 0xfe, 0xef, 0xdf, 0xfd},
			{0x7f, 0xbf, 0xdf, 0xef, 0xf7, 0xfb, 0xfd, 0xfe},
		};

		private readonly int x_;
		private readonly byte[] flags_;

		public HSEratosthenes(int x)
		{
			x_ = x;
			int size = (x + 29) / 30;
			flags_ = new byte[size];
			if (x <= 6) {
				return;
			}

			flags_.AsSpan().Fill(0xff);

			int r = x % 30;
			if (r > 0) {
				if (r < 1) {
					flags_[size - 1] = 0x0;
				} else if (r < 7) {
					flags_[size - 1] = 0x1;
				} else if (r < 11) {
					flags_[size - 1] = 0x3;
				} else if (r < 13) {
					flags_[size - 1] = 0x7;
				} else if (r < 17) {
					flags_[size - 1] = 0xf;
				} else if (r < 19) {
					flags_[size - 1] = 0x1f;
				} else if (r < 23) {
					flags_[size - 1] = 0x3f;
				} else if (r < 29) {
					flags_[size - 1] = 0x7f;
				}
			}

			flags_[0] = 0xfe;

			long sqrt = (long)Math.Ceiling(Math.Sqrt(x) + 0.1) / 30 + 1;
			for (long i = 0; i < sqrt; ++i) {
				for (int flag = flags_[i]; flag > 0; flag &= (flag - 1)) {
					int lsb = flag & (-flag);
					int ibit = BitToIndex((byte)lsb);
					int m = MOD30[ibit];
					long pm = 30 * i + 2 * m;
					for (long j = i * pm + (m * m) / 30, k = ibit;
						j < flags_.Length;
						j += i * C1[k] + C0[ibit, k], k = (k + 1) & 7) {
						flags_[j] &= MASK[ibit, k];
					}
				}
			}
		}

		public int GetPrimeCount()
		{
			if (flags_ is null) {
				return -1;
			}

			int ret = 3;
			foreach (var f in flags_) {
				ret += (int)Popcnt.PopCount(f);
			}

			return ret;
		}

		public List<int> GetPrimes()
		{
			if (x_ <= 1) {
				return new List<int>();
			}

			var primes = new List<int>((int)(x_ / Math.Log(x_) * 1.1));
			if (x_ >= 2) {
				primes.Add(2);
			}

			if (x_ >= 3) {
				primes.Add(3);
			}

			if (x_ >= 5) {
				primes.Add(5);
			}

			for (int i = 0; i < flags_.Length; i++) {
				for (int j = 0; j < 8; j++) {
					if ((1 << j & flags_[i]) != 0) {
						primes.Add(i * 30 + MOD30[j]);
					}
				}
			}

			return primes;
		}

		private int BitToIndex(byte b)
		{
			return b switch {
				1 << 0 => 0,
				1 << 1 => 1,
				1 << 2 => 2,
				1 << 3 => 3,
				1 << 4 => 4,
				1 << 5 => 5,
				1 << 6 => 6,
				1 << 7 => 7,
				_ => -1,
			};
		}
	}
}

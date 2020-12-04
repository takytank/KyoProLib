using System;
using System.Collections.Generic;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public struct ModInt
	{
		//public const long P = 1000000007;
		public const long P = 998244353;
		public const long ROOT = 3;

		// (924844033, 5)
		// (998244353, 3)
		// (1012924417, 5)
		// (167772161, 3)
		// (469762049, 3)
		// (1224736769, 3)

		private long value_;

		public ModInt(long value)
			=> value_ = value;
		public ModInt(long value, bool mods)
		{
			if (mods) {
				value %= P;
				if (value < 0) {
					value += P;
				}
			}

			value_ = value;
		}

		public static ModInt operator +(ModInt lhs, ModInt rhs)
		{
			lhs.value_ = (lhs.value_ + rhs.value_) % P;
			return lhs;
		}
		public static ModInt operator +(long lhs, ModInt rhs)
		{
			rhs.value_ = (lhs + rhs.value_) % P;
			return rhs;
		}
		public static ModInt operator +(ModInt lhs, long rhs)
		{
			lhs.value_ = (lhs.value_ + rhs) % P;
			return lhs;
		}

		public static ModInt operator -(ModInt lhs, ModInt rhs)
		{
			lhs.value_ = (P + lhs.value_ - rhs.value_) % P;
			return lhs;
		}
		public static ModInt operator -(long lhs, ModInt rhs)
		{
			rhs.value_ = (P + lhs - rhs.value_) % P;
			return rhs;
		}
		public static ModInt operator -(ModInt lhs, long rhs)
		{
			lhs.value_ = (P + lhs.value_ - rhs) % P;
			return lhs;
		}

		public static ModInt operator *(ModInt lhs, ModInt rhs)
		{
			lhs.value_ = lhs.value_ * rhs.value_ % P;
			return lhs;
		}
		public static ModInt operator *(long lhs, ModInt rhs)
		{
			rhs.value_ = lhs * rhs.value_ % P;
			return rhs;
		}
		public static ModInt operator *(ModInt lhs, long rhs)
		{
			lhs.value_ = lhs.value_ * rhs % P;
			return lhs;
		}

		public static ModInt operator /(ModInt lhs, ModInt rhs)
		{
			long exp = P - 2;
			while (exp > 0) {
				if (exp % 2 > 0) {
					lhs *= rhs;
				}

				rhs *= rhs;
				exp /= 2;
			}

			return lhs;
		}

		public static implicit operator ModInt(long n) => new ModInt(n, true);

		public static ModInt Inverse(ModInt value) => Pow(value, P - 2);
		public static ModInt Pow(ModInt value, long k) => Pow(value.value_, k);
		public static ModInt Pow(long value, long k)
		{
			long ret = 1;
			for (k %= P - 1; k > 0; k >>= 1, value = value * value % P) {
				if ((k & 1) == 1) {
					ret = ret * value % P;
				}
			}
			return new ModInt(ret);
		}


		public static Span<ModInt> NumberTheoreticTransform(
			Span<int> values, bool inverses = false)
		{
			var mods = new ModInt[values.Length];
			for (int i = 0; i < mods.Length; i++) {
				mods[i] = new ModInt(values[i]);
			}

			return NumberTheoreticTransform(mods, inverses);
		}

		public static Span<ModInt> NumberTheoreticTransform(
			Span<long> values, bool inverses = false)
		{
			var mods = new ModInt[values.Length];
			for (int i = 0; i < mods.Length; i++) {
				mods[i] = new ModInt(values[i]);
			}

			return NumberTheoreticTransform(mods, inverses);
		}

		public static Span<ModInt> NumberTheoreticTransform(
			Span<ModInt> a, bool inverses = false)
		{
			int n = a.Length;
			if (n == 1) {
				return a;
			}

			var b = new ModInt[n].AsSpan();
			int r = inverses
				? (int)(P - 1 - (P - 1) / n)
				: (int)((P - 1) / n);
			ModInt s = Pow(ROOT, r);
			var kp = new ModInt[n / 2 + 1];
			kp.AsSpan().Fill(1);

			for (int i = 0; i < n / 2; ++i) {
				kp[i + 1] = kp[i] * s;
			}

			int l = n / 2;
			for (int i = 1; i < n; i <<= 1, l >>= 1) {
				r = 0;
				for (int j = 0; j < l; ++j, r += i) {
					s = kp[i * j];
					for (int k = 0; k < i; ++k) {
						var p = a[k + r];
						var q = a[k + r + n / 2];
						b[k + 2 * r] = p + q;
						b[k + 2 * r + i] = (p - q) * s;
					}
				}

				var temp = a;
				a = b;
				b = temp;
			}

			if (inverses) {
				s = Inverse(n);
				for (int i = 0; i < n; i++) {
					a[i] = a[i] * s;
				}
			}

			return a;
		}

		public static ModInt[,] NumberTheoreticTransform2D(ModInt[,] a, bool inverses = false)
		{
			int h = a.GetLength(0);
			int w = a.GetLength(1);
			if (h == 1 && w == 1) {
				return a;
			}

			var b = new ModInt[h, w];

			{
				int n = w;
				int r = inverses
					? (int)(P - 1 - (P - 1) / n)
					: (int)((P - 1) / n);
				ModInt s = Pow(ROOT, r);
				var kp = new ModInt[n / 2 + 1];
				kp.AsSpan().Fill(1);

				for (int i = 0; i < n / 2; ++i) {
					kp[i + 1] = kp[i] * s;
				}

				for (int y = 0; y < h; y++) {
					int l = n / 2;
					for (int i = 1; i < n; i <<= 1, l >>= 1) {
						r = 0;
						for (int j = 0; j < l; ++j, r += i) {
							s = kp[i * j];
							for (int k = 0; k < i; ++k) {
								var p = a[y, k + r];
								var q = a[y, k + r + n / 2];
								b[y, k + 2 * r] = p + q;
								b[y, k + 2 * r + i] = (p - q) * s;
							}
						}

						var temp = a;
						a = b;
						b = temp;
					}

					if (inverses) {
						s = Inverse(n);
						for (int i = 0; i < n; i++) {
							a[y, i] = a[y, i] * s;
						}
					}
				}
			}

			for (int i = 0; i < h; i++) {
				for (int j = 0; j < w; j++) {
					b[h, w] = 0;
				}
			}

			{
				int n = h;
				int r = inverses
					? (int)(P - 1 - (P - 1) / n)
					: (int)((P - 1) / n);
				ModInt s = Pow(ROOT, r);
				var kp = new ModInt[n / 2 + 1];
				kp.AsSpan().Fill(1);

				for (int i = 0; i < n / 2; ++i) {
					kp[i + 1] = kp[i] * s;
				}

				for (int x = 0; x < w; x++) {
					int l = n / 2;
					for (int i = 1; i < n; i <<= 1, l >>= 1) {
						r = 0;
						for (int j = 0; j < l; ++j, r += i) {
							s = kp[i * j];
							for (int k = 0; k < i; ++k) {
								var p = a[k + r, x];
								var q = a[k + r + n / 2, x];
								b[k + 2 * r, x] = p + q;
								b[k + 2 * r + i, x] = (p - q) * s;
							}
						}

						var temp = a;
						a = b;
						b = temp;
					}

					if (inverses) {
						s = Inverse(n);
						for (int i = 0; i < n; i++) {
							a[i, x] = a[i, x] * s;
						}
					}
				}
			}

			return a;
		}

		public static Span<ModInt> Convolve(ReadOnlySpan<ModInt> a, ReadOnlySpan<ModInt> b)
		{
			int resultLength = a.Length + b.Length - 1;
			int nttLenght = 1;
			while (nttLenght < resultLength) {
				nttLenght <<= 1;
			}

			var aa = new ModInt[nttLenght];
			a.CopyTo(aa);
			var bb = new ModInt[nttLenght];
			b.CopyTo(bb);

			var fa = NumberTheoreticTransform(aa);
			var fb = NumberTheoreticTransform(bb);
			for (int i = 0; i < nttLenght; i++) {
				fa[i] *= fb[i];
			}

			var convolved = NumberTheoreticTransform(fa, true);
			return convolved.Slice(0, resultLength);
		}

		public long ToLong() => value_;
		public override string ToString() => value_.ToString();
	}

	public struct VModInt
	{
		public static long P { get; set; } = 1000000007;

		public static VModInt Inverse(VModInt value) => Pow(value, P - 2);
		public static VModInt Pow(VModInt value, long k) => Pow(value.value_, k);
		public static VModInt Pow(long value, long k)
		{
			long ret = 1;
			for (k %= P - 1; k > 0; k >>= 1, value = value * value % P) {
				if ((k & 1) == 1) {
					ret = ret * value % P;
				}
			}
			return new VModInt(ret);
		}

		private long value_;

		public VModInt(long value)
			=> value_ = value;
		public VModInt(long value, bool mods)
		{
			if (mods) {
				value %= P;
				if (value < 0) {
					value += P;
				}
			}

			value_ = value;
		}

		public static VModInt operator +(VModInt lhs, VModInt rhs)
		{
			lhs.value_ += rhs.value_;
			if (lhs.value_ >= P) {
				lhs.value_ -= P;
			}
			return lhs;
		}
		public static VModInt operator +(long lhs, VModInt rhs)
		{
			rhs.value_ += lhs;
			if (rhs.value_ >= P) {
				rhs.value_ -= P;
			}
			return rhs;
		}
		public static VModInt operator +(VModInt lhs, long rhs)
		{
			lhs.value_ += rhs;
			if (lhs.value_ >= P) {
				lhs.value_ -= P;
			}
			return lhs;
		}

		public static VModInt operator -(VModInt lhs, VModInt rhs)
		{
			lhs.value_ -= rhs.value_;
			if (lhs.value_ < 0) {
				lhs.value_ += P;
			}
			return lhs;
		}
		public static VModInt operator -(long lhs, VModInt rhs)
		{
			rhs.value_ -= lhs;
			if (rhs.value_ < 0) {
				rhs.value_ += P;
			}
			return rhs;
		}
		public static VModInt operator -(VModInt lhs, long rhs)
		{
			lhs.value_ -= rhs;
			if (lhs.value_ < 0) {
				lhs.value_ += P;
			}
			return lhs;
		}

		public static VModInt operator *(VModInt lhs, VModInt rhs)
			=> new VModInt(lhs.value_ * rhs.value_ % P);
		public static VModInt operator *(long lhs, VModInt rhs)
			=> new VModInt(lhs * rhs.value_ % P);
		public static VModInt operator *(VModInt lhs, long rhs)
			=> new VModInt(lhs.value_ * rhs % P);

		public static VModInt operator /(VModInt lhs, VModInt rhs)
		{
			long exp = P - 2;
			while (exp > 0) {
				if (exp % 2 > 0) {
					lhs *= rhs;
				}

				rhs *= rhs;
				exp /= 2;
			}

			return lhs;
		}

		public static implicit operator VModInt(long n) => new VModInt(n, true);
		public long ToLong() => value_;
		public override string ToString() => value_.ToString();
	}
}

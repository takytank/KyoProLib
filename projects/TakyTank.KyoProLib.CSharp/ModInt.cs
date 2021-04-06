using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public struct ModInt
	{
		//public const long P = 1000000007;
		public const long P = 998244353;
		//public const long P = 2;
		public const long ROOT = 3;

		// (924844033, 5)
		// (998244353, 3)
		// (1012924417, 5)
		// (167772161, 3)
		// (469762049, 3)
		// (1224736769, 3)

		private long value_;

		public static ModInt New(long value, bool mods) => new ModInt(value, mods);
		public ModInt(long value) => value_ = value;
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
			=> lhs * Inverse(rhs);

		public static implicit operator ModInt(long n) => new ModInt(n, true);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ModInt Inverse(ModInt value) => Pow(value, P - 2);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ModInt Pow(ModInt value, long k) => Pow(value.value_, k);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ModInt Pow(long value, long k)
		{
			long ret = 1;
			while (k > 0) {
				if ((k & 1) != 0) {
					ret = ret * value % P;
				}

				value = value * value % P;
				k >>= 1;
			}

			return new ModInt(ret);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<ModInt> NTT(Span<int> values, bool inverses = false, bool extends = false)
			=> NumberTheoreticTransform(values, inverses, extends);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<ModInt> NumberTheoreticTransform(
			Span<int> values, bool inverses = false, bool extends = false)
		{
			int n = extends == false ? values.Length : CeilPow2(values.Length);
			var mods = new ModInt[n];
			for (int i = 0; i < values.Length; ++i) {
				mods[i] = new ModInt(values[i]);
			}

			return NumberTheoreticTransform(mods, inverses, false, true);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<ModInt> NTT(Span<long> values, bool inverses = false, bool extends = false)
			=> NumberTheoreticTransform(values, inverses, extends);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<ModInt> NumberTheoreticTransform(
			Span<long> values, bool inverses = false, bool extends = false)
		{
			int n = extends == false ? values.Length : CeilPow2(values.Length);
			var mods = new ModInt[n];
			for (int i = 0; i < values.Length; ++i) {
				mods[i] = new ModInt(values[i]);
			}

			return NumberTheoreticTransform(mods, inverses, false, true);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<ModInt> NTT(
			Span<ModInt> values, bool inverses = false, bool extends = false, bool inplaces = true)
			=> NumberTheoreticTransform(values, inverses, extends, inplaces);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<ModInt> NumberTheoreticTransform(
			Span<ModInt> a, bool inverses = false, bool extends = false, bool inplaces = true)
		{
			int n = a.Length;
			if (extends) {
				n = CeilPow2(n);
				inplaces = false;
			}

			var ret = a;
			if (inplaces == false) {
				ret = new ModInt[n];
				a.CopyTo(ret);
			}

			if (n == 1) {
				return ret;
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
						var p = ret[k + r];
						var q = ret[k + r + n / 2];
						b[k + 2 * r] = p + q;
						b[k + 2 * r + i] = (p - q) * s;
					}
				}

				var temp = ret;
				ret = b;
				b = temp;
			}

			if (inverses) {
				s = Inverse(n);
				for (int i = 0; i < n; ++i) {
					ret[i] = ret[i] * s;
				}
			}

			return ret;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ModInt[,] Ntt2D(
			ModInt[,] a, bool inverses = false, bool extends = false, bool inplaces = true)
			=> NumberTheoreticTransform2D(a, inverses, extends, inplaces);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ModInt[,] NumberTheoreticTransform2D(
			ModInt[,] a, bool inverses = false, bool extends = false, bool inplaces = true)
		{
			int h = a.GetLength(0);
			int w = a.GetLength(1);
			if (extends) {
				h = CeilPow2(h);
				w = CeilPow2(w);
				inplaces = false;
			}

			var ret = a;
			if (inplaces == false) {
				ret = new ModInt[h, w];
				int hh = a.GetLength(0);
				int ww = a.GetLength(1);
				for (int i = 0; i < hh; i++) {
					for (int j = 0; j < ww; j++) {
						ret[i, j] = a[i, j];
					}
				}
			}

			if (h == 1 && w == 1) {
				return ret;
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

				for (int y = 0; y < h; ++y) {
					int l = n / 2;
					for (int i = 1; i < n; i <<= 1, l >>= 1) {
						r = 0;
						for (int j = 0; j < l; ++j, r += i) {
							s = kp[i * j];
							for (int k = 0; k < i; ++k) {
								var p = ret[y, k + r];
								var q = ret[y, k + r + n / 2];
								b[y, k + 2 * r] = p + q;
								b[y, k + 2 * r + i] = (p - q) * s;
							}
						}

						var temp = ret;
						ret = b;
						b = temp;
					}

					if (inverses) {
						s = Inverse(n);
						for (int i = 0; i < n; ++i) {
							ret[y, i] = ret[y, i] * s;
						}
					}
				}
			}

			for (int i = 0; i < h; ++i) {
				for (int j = 0; j < w; ++j) {
					b[i, j] = 0;
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

				for (int x = 0; x < w; ++x) {
					int l = n / 2;
					for (int i = 1; i < n; i <<= 1, l >>= 1) {
						r = 0;
						for (int j = 0; j < l; ++j, r += i) {
							s = kp[i * j];
							for (int k = 0; k < i; ++k) {
								var p = ret[k + r, x];
								var q = ret[k + r + n / 2, x];
								b[k + 2 * r, x] = p + q;
								b[k + 2 * r + i, x] = (p - q) * s;
							}
						}

						var temp = ret;
						ret = b;
						b = temp;
					}

					if (inverses) {
						s = Inverse(n);
						for (int i = 0; i < n; ++i) {
							ret[i, x] = ret[i, x] * s;
						}
					}
				}
			}

			return ret;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<ModInt> Convolve(ReadOnlySpan<ModInt> a, ReadOnlySpan<ModInt> b)
		{
			int resultLength = a.Length + b.Length - 1;
			int nttLength = CeilPow2(resultLength);

			var aa = new ModInt[nttLength];
			a.CopyTo(aa);
			var bb = new ModInt[nttLength];
			b.CopyTo(bb);

			var fa = NumberTheoreticTransform(aa);
			var fb = NumberTheoreticTransform(bb);
			for (int i = 0; i < nttLength; ++i) {
				fa[i] *= fb[i];
			}

			var convolved = NumberTheoreticTransform(fa, true);
			return convolved.Slice(0, resultLength);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int CeilPow2(int n)
		{
			int pow2 = 1;
			while (pow2 < n) {
				pow2 <<= 1;
			}

			return pow2;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long ToLong() => value_;
		public override string ToString() => value_.ToString();
	}

	public struct DModInt
	{
		public static long P { get; set; } = 1000000007;

		private long value_;

		public static DModInt New(long value, bool mods) => new DModInt(value, mods);
		public DModInt(long value) => value_ = value;
		public DModInt(long value, bool mods)
		{
			if (mods) {
				value %= P;
				if (value < 0) {
					value += P;
				}
			}

			value_ = value;
		}

		public static DModInt operator +(DModInt lhs, DModInt rhs)
		{
			lhs.value_ += rhs.value_;
			if (lhs.value_ >= P) {
				lhs.value_ -= P;
			}
			return lhs;
		}
		public static DModInt operator +(long lhs, DModInt rhs)
		{
			rhs.value_ += lhs;
			if (rhs.value_ >= P) {
				rhs.value_ -= P;
			}
			return rhs;
		}
		public static DModInt operator +(DModInt lhs, long rhs)
		{
			lhs.value_ += rhs;
			if (lhs.value_ >= P) {
				lhs.value_ -= P;
			}
			return lhs;
		}

		public static DModInt operator -(DModInt lhs, DModInt rhs)
		{
			lhs.value_ -= rhs.value_;
			if (lhs.value_ < 0) {
				lhs.value_ += P;
			}
			return lhs;
		}
		public static DModInt operator -(long lhs, DModInt rhs)
		{
			rhs.value_ -= lhs;
			if (rhs.value_ < 0) {
				rhs.value_ += P;
			}
			return rhs;
		}
		public static DModInt operator -(DModInt lhs, long rhs)
		{
			lhs.value_ -= rhs;
			if (lhs.value_ < 0) {
				lhs.value_ += P;
			}
			return lhs;
		}

		public static DModInt operator *(DModInt lhs, DModInt rhs)
			=> new DModInt(lhs.value_ * rhs.value_ % P);
		public static DModInt operator *(long lhs, DModInt rhs)
			=> new DModInt(lhs * rhs.value_ % P);
		public static DModInt operator *(DModInt lhs, long rhs)
			=> new DModInt(lhs.value_ * rhs % P);

		public static DModInt operator /(DModInt lhs, DModInt rhs)
			=> lhs * Inverse(rhs);

		public static implicit operator DModInt(long n) => new DModInt(n, true);

		public static DModInt Inverse(DModInt value)
		{
			if (Gcd(value.value_, P) != 1) {
				throw new Exception($"GCD of {value.value_} and {P} is not 1");
			}

			var (_, x, _) = ExtendedEuclidean(value.value_, P);
			x %= P;
			if (x < 0) {
				x += P;
			}

			return x;

			static long Gcd(long a, long b)
			{
				if (b == 0) {
					return a;
				}

				return Gcd(b, a % b);
			}

			static (long gcd, long x, long y) ExtendedEuclidean(long a, long b)
			{
				if (b == 0) {
					return (a, 1, 0);
				}

				var (gcd, y, x) = ExtendedEuclidean(b, a % b);
				y -= a / b * x;
				return (gcd, x, y);
			}
		}

		public static DModInt Pow(DModInt value, long k) => Pow(value.value_, k);
		public static DModInt Pow(long value, long k)
		{
			long ret = 1;
			while (k > 0) {
				if ((k & 1) != 0) {
					ret = ret * value % P;
				}

				value = value * value % P;
				k >>= 1;
			}

			return new DModInt(ret);
		}

		public long ToLong() => value_;
		public override string ToString() => value_.ToString();
	}
}

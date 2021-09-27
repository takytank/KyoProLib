using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V8
{
	public static class Fourier
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<Complex> FFT(Span<double> values, bool inverses = false)
			=> CooleyTukey(values, inverses);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<Complex> CooleyTukey(
			Span<double> values, bool inverses = false)
		{
			var complex = new Complex[values.Length];
			for (int i = 0; i < complex.Length; i++) {
				complex[i] = new Complex(values[i], 0);
			}

			return CooleyTukey(complex, inverses);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<Complex> FFT(Span<Complex> values, bool inverses = false)
			=> CooleyTukey(values, inverses);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<Complex> CooleyTukey(
			Span<Complex> values, bool inverses = false)
		{
			int n = values.Length;
			int h = 0;
			for (int i = 0; 1 << i < n; i++) {
				h++;
			}

			for (int i = 0; i < n; i++) {
				int j = 0;
				for (int k = 0; k < h; k++) {
					j |= (i >> k & 1) << (h - 1 - k);
				}

				if (i < j) {
					var temp = values[i];
					values[i] = values[j];
					values[j] = temp;
				}
			}

			for (int b = 1; b < n; b *= 2) {
				for (int j = 0; j < b; j++) {
					var w = Complex.FromPolarCoordinates(
						1.0,
						(2 * Math.PI) / (2 * b) * j * (inverses ? 1 : -1));
					for (int k = 0; k < n; k += b * 2) {
						var s = values[j + k];
						var t = values[j + k + b] * w;
						values[j + k] = s + t;
						values[j + k + b] = s - t;
					}
				}
			}

			if (inverses) {
				for (int i = 0; i < n; i++) values[i] /= n;
			}

			return values;
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
				? (int)(ModInt.P - 1 - (ModInt.P - 1) / n)
				: (int)((ModInt.P - 1) / n);
			ModInt s = ModInt.Pow(ModInt.ROOT, r);
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
				s = ModInt.Inverse(n);
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
					? (int)(ModInt.P - 1 - (ModInt.P - 1) / n)
					: (int)((ModInt.P - 1) / n);
				ModInt s = ModInt.Pow(ModInt.ROOT, r);
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
						s = ModInt.Inverse(n);
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
					? (int)(ModInt.P - 1 - (ModInt.P - 1) / n)
					: (int)((ModInt.P - 1) / n);
				ModInt s = ModInt.Pow(ModInt.ROOT, r);
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
						s = ModInt.Inverse(n);
						for (int i = 0; i < n; ++i) {
							ret[i, x] = ret[i, x] * s;
						}
					}
				}
			}

			return ret;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<double> Convolve(ReadOnlySpan<double> a, ReadOnlySpan<double> b)
		{
			int resultLength = a.Length + b.Length - 1;
			int fftLength = CeilPow2(resultLength);

			var aa = new double[fftLength];
			a.CopyTo(aa);
			var bb = new double[fftLength];
			b.CopyTo(bb);

			var fa = FFT(aa);
			var fb = FFT(bb);
			for (int i = 0; i < fftLength; i++) {
				fa[i] *= fb[i];
			}

			var convolved = FFT(fa, true);
			var result = new double[resultLength];
			for (int i = 0; i < resultLength; i++) {
				result[i] = convolved[i].Real;
			}

			return result;
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
	}
}

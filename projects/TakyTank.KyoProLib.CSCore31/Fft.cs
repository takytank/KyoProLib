using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace TakyTank.KyoProLib.CSCore31
{
	public static class Fft
	{
		public static Span<Complex> FourierTransformByCooleyTukey(
			Span<double> values, bool inverses = false)
		{
			var complex = new Complex[values.Length];
			for (int i = 0; i < complex.Length; i++) {
				complex[i] = new Complex(values[i], 0);
			}

			return FourierTransformByCooleyTukey(complex, inverses);
		}

		public static Span<Complex> FourierTransformByCooleyTukey(
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

		public static Span<double> Convolve(ReadOnlySpan<double> a, ReadOnlySpan<double> b)
		{
			int resultLength = a.Length + b.Length - 1;
			int fftLength = 1;
			while (fftLength < resultLength) {
				fftLength <<= 1;
			}

			var aa = new double[fftLength];
			a.CopyTo(aa);
			var bb = new double[fftLength];
			b.CopyTo(bb);

			var fa = FourierTransformByCooleyTukey(aa);
			var fb = FourierTransformByCooleyTukey(bb);
			for (int i = 0; i < fftLength; i++) {
				fa[i] *= fb[i];
			}

			var convolved = FourierTransformByCooleyTukey(fa, true);
			var result = new double[resultLength];
			for (int i = 0; i < resultLength; i++) {
				result[i] = convolved[i].Real;
			}

			return result;
		}
	}
}

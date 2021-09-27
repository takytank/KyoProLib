using System;
using System.Collections.Generic;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V8
{
	public static class WalshHadamard
	{
		public static Span<long> Fwt(BitOp op, Span<long> values, bool inverses)
		{
			return op switch {
				BitOp.Or => FwtOr(values, inverses),
				BitOp.And => FwtAnd(values, inverses),
				BitOp.Xor => FwtXor(values, inverses),
				_ => null,
			};
		}

		public static Span<long> FwtAnd(Span<long> values, bool inverses)
		{
			int n = values.Length;
			for (int i = 1; i < n; i <<= 1) {
				for (int j = 0; j < n; j++) {
					if ((j & i) == 0) {
						if (inverses) {
							values[j] -= values[j | i];
						} else {
							values[j] += values[j | i];
						}
					}
				}
			}

			return values;
		}

		public static Span<long> FwtOr(Span<long> values, bool inverses)
		{
			int n = values.Length;
			for (int i = 1; i < n; i <<= 1) {
				for (int j = 0; j < n; j++) {
					if ((j & i) == 0) {
						if (inverses) {
							values[j | i] -= values[j];
						} else {
							values[j | i] += values[j];
						}
					}
				}
			}

			return values;
		}

		public static Span<long> FwtXor(Span<long> values, bool inverses)
		{
			int n = values.Length;
			int div = inverses ? 2 : 1;
			for (int i = 1; i < n; i <<= 1) {
				for (int j = 0; j < n; j++) {
					if ((j & i) == 0) {
						long x = values[j];
						long y = values[j | i];
						values[j] = (x + y) / div;
						values[j | i] = (x - y) / div;
					}
				}
			}

			return values;
		}

		public static Span<long> Convolve(BitOp op, ReadOnlySpan<long> a, ReadOnlySpan<long> b)
		{
			int n = a.Length;
			var aa = new long[n];
			a.CopyTo(aa);
			var bb = new long[n];
			b.CopyTo(bb);

			Fwt(op, aa, false);
			Fwt(op, bb, false);
			for (int i = 0; i < n; i++) {
				aa[i] *= bb[i];
			}

			var convolved = Fwt(op, aa, true);
			return convolved;
		}

		public enum BitOp
		{
			Or,
			And,
			Xor,
		}
	}
}

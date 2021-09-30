using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V8
{
	public static class ZetaTransform
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<long> FastZetaTransform(SetOp op, Span<long> values) => Fzt(op, values);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<long> Fzt(SetOp op, Span<long> values)
		{
			return op switch {
				SetOp.Super => ZetaSuperSet(values),
				SetOp.Sub => ZetaSubSet(values),
				SetOp.Divisor => ZetaDivisor(values),
				SetOp.Multiple => ZetaMultiple(values),
				_ => null,
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<long> FastMoebiusTransform(SetOp op, Span<long> values) => Fmt(op, values);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<long> Fmt(SetOp op, Span<long> values)
		{
			return op switch {
				SetOp.Super => MoebiusSuperSet(values),
				SetOp.Sub => MoebiusSubSet(values),
				SetOp.Divisor => MoebiusDivisor(values),
				SetOp.Multiple => MoebiusMultiple(values),
				_ => null,
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<long> ZetaSuperSet(Span<long> values)
		{
			int n = values.Length;
			for (int i = 1; i < n; i <<= 1) {
				for (int j = 0; j < n; ++j) {
					if ((j & i) == 0) {
						values[j] += values[j | i];
					}
				}
			}

			return values;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<long> MoebiusSuperSet(Span<long> values)
		{
			int n = values.Length;
			for (int i = 1; i < n; i <<= 1) {
				for (int j = 0; j < n; ++j) {
					if ((j & i) == 0) {
						values[j] -= values[j | i];
					}
				}
			}

			return values;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<long> ZetaSubSet(Span<long> values)
		{
			int n = values.Length;
			for (int i = 1; i < n; i <<= 1) {
				for (int j = 0; j < n; ++j) {
					if ((j & i) == 0) {
						values[j | i] += values[j];
					}
				}
			}

			return values;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<long> MoebiusSubSet(Span<long> values)
		{
			int n = values.Length;
			for (int i = 1; i < n; i <<= 1) {
				for (int j = 0; j < n; ++j) {
					if ((j & i) == 0) {
						values[j | i] -= values[j];
					}
				}
			}

			return values;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<long> ZetaDivisor(Span<long> values)
		{
			int n = values.Length;
			var sieve = new bool[n];
			for (int p = 2; p < n; ++p) {
				if (sieve[p] == false) {
					for (int i = 1; i * p < n; ++i) {
						sieve[i * p] = true;
						values[i * p] += values[i];
					}
				}
			}

			return values;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<long> MoebiusDivisor(Span<long> values)
		{
			int n = values.Length;
			var sieve = new bool[n];
			int m = n - 1;
			for (int p = 2; p < n; ++p) {
				if (sieve[p] == false) {
					for (int i = m / p; i > 0; --i) {
						sieve[i * p] = true;
						values[i * p] -= values[i];
					}
				}
			}

			return values;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<long> ZetaMultiple(Span<long> values)
		{
			int n = values.Length;
			var sieve = new bool[n];
			int m = n - 1;
			for (int p = 2; p < n; ++p) {
				if (sieve[p] == false) {
					for (int i = m / p; i > 0; --i) {
						sieve[i * p] = true;
						values[i] += values[i * p];
					}
				}
			}

			return values;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<long> MoebiusMultiple(Span<long> values)
		{
			int n = values.Length;
			var sieve = new bool[n];
			for (int p = 2; p < n; ++p) {
				if (sieve[p] == false) {
					for (int i = 1; i * p < n; ++i) {
						sieve[i * p] = true;
						values[i] -= values[i * p];
					}
				}
			}

			return values;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<long> ConvolveAnd(ReadOnlySpan<long> a, ReadOnlySpan<long> b)
			=> Convolve(SetOp.Super, a, b);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<long> ConvolveOr(ReadOnlySpan<long> a, ReadOnlySpan<long> b)
			=> Convolve(SetOp.Sub, a, b);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<long> ConvolveGcd(ReadOnlySpan<long> a, ReadOnlySpan<long> b)
			=> Convolve(SetOp.Multiple, a, b);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<long> ConvolveLcm(ReadOnlySpan<long> a, ReadOnlySpan<long> b)
			=> Convolve(SetOp.Divisor, a, b);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<long> Convolve(SetOp op, ReadOnlySpan<long> a, ReadOnlySpan<long> b)
		{
			int n = a.Length;
			var aa = new long[n];
			a.CopyTo(aa);
			var bb = new long[n];
			b.CopyTo(bb);

			Fzt(op, aa);
			Fzt(op, bb);
			for (int i = 0; i < n; i++) {
				aa[i] *= bb[i];
			}

			var convolved = Fmt(op, aa);
			return convolved;
		}

		public enum SetOp
		{
			Super,
			Sub,
			Divisor,
			Multiple,
		}
	}
}

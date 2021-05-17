using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public static class ZetaTransform
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<T> FastZetaTransform<T>(
			Span<T> values, Func<T, T, T> translate, bool superSet = true)
			=> Fzt(values, translate, superSet);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<T> Fzt<T>(
			Span<T> values,
			Func<T, T, T> translate,
			bool superSet = true)
		{
			int n = values.Length;
			for (int i = 1; i < n; i <<= 1) {
				for (int j = 0; j < n; ++j) {
					if ((j & i) == 0) {
						if (superSet) {
							values[j] = translate(values[j], values[j | i]);
						} else {
							values[j | i] = translate(values[j | i], values[j]);
						}
					}
				}
			}

			return values;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<long> FastZetaTransformSuperSet(Span<long> values)
			=> FztSuperSet(values);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<long> FztSuperSet(Span<long> values)
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
		public static Span<long> FastZetaTransformSubSet(Span<long> values)
			=> FztSubSet(values);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<long> FztSubSet(Span<long> values)
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
		public static Span<long> FastMoebiusTransformSuperSet(Span<long> values)
			=> FmtSuperSet(values);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<long> FmtSuperSet(Span<long> values)
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
		public static Span<long> FastMoebiusTransformSubSet(Span<long> values)
			=> FmtSubSet(values);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<long> FmtSubSet(Span<long> values)
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
		public static Span<long> FastZetaTransformDivisor(Span<long> values)
			=> FztDivisor(values);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<long> FztDivisor(Span<long> values)
		{
			int n = values.Length;
			var sieve = new bool[n];
			for (int i = 0; i < n; i++) {
				sieve[i] = true;
			}

			for (int p = 2; p < n; ++p) {
				if (sieve[p]) {
					for (int k = 1; k * p < n; ++k) {
						sieve[k * p] = false;
						values[k * p] += values[k];
					}
				}
			}

			return values;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<long> FztDivisor2(Span<long> values)
		{
			int n = values.Length;
			for (int i = n - 1; i > 0; --i) {
				for (int j = i; (j += i) < n;) {
					values[j] += values[i];
				}
			}

			return values;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<long> FastMoebiusTransformDivisor(Span<long> values)
			=> FmtDivisor(values);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<long> FmtDivisor(Span<long> values)
		{
			int n = values.Length;
			for (int i = 1; i < n; i++) {
				for (int j = i; (j += i) < n;) {
					values[j] -= values[i];
				}
			}

			return values;
		}
	}
}

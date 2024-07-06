using System;
using System.Runtime.CompilerServices;

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
		public static Span<long> FastMobiusTransform(SetOp op, Span<long> values) => Fmt(op, values);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<long> Fmt(SetOp op, Span<long> values)
		{
			return op switch {
				SetOp.Super => MobiusSuperSet(values),
				SetOp.Sub => MobiusSubSet(values),
				SetOp.Divisor => MobiusDivisor(values),
				SetOp.Multiple => MobiusMultiple(values),
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
		public static Span<long> MobiusSuperSet(Span<long> values)
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
		public static Span<long> MobiusSubSet(Span<long> values)
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
		public static Span<long> MobiusDivisor(Span<long> values)
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
		public static Span<long> MobiusMultiple(Span<long> values)
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

	public class ZetaTransform<T>
	{
		private readonly Func<T, T, T> _add;
		private readonly Func<T, T, T> _sub;

		public ZetaTransform(
			Func<T, T, T> add,
			Func<T, T, T> sub)
		{
			_add = add;
			_sub = sub;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Span<T> ZetaSuperSet(Span<T> values)
		{
			int n = values.Length;
			for (int i = 1; i < n; i <<= 1) {
				for (int j = 0; j < n; ++j) {
					if ((j & i) == 0) {
						values[j] = _add(values[j], values[j | i]);
					}
				}
			}

			return values;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Span<T> MobiusSuperSet(Span<T> values)
		{
			int n = values.Length;
			for (int i = 1; i < n; i <<= 1) {
				for (int j = 0; j < n; ++j) {
					if ((j & i) == 0) {
						values[j] = _sub(values[j], values[j | i]);
					}
				}
			}

			return values;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Span<T> ZetaSubSet(Span<T> values)
		{
			int n = values.Length;
			for (int i = 1; i < n; i <<= 1) {
				for (int j = 0; j < n; ++j) {
					if ((j & i) == 0) {
						values[j | i] = _add(values[j | i], values[j]);
					}
				}
			}

			return values;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Span<T> MobiusSubSet(Span<T> values)
		{
			int n = values.Length;
			for (int i = 1; i < n; i <<= 1) {
				for (int j = 0; j < n; ++j) {
					if ((j & i) == 0) {
						values[j | i] = _sub(values[j | i], values[j]);
					}
				}
			}

			return values;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Span<T> ZetaDivisor(Span<T> values)
		{
			int n = values.Length;
			var sieve = new bool[n];
			for (int p = 2; p < n; ++p) {
				if (sieve[p] == false) {
					for (int i = 1; i * p < n; ++i) {
						sieve[i * p] = true;
						values[i * p] = _add(values[i * p], values[i]);
					}
				}
			}

			return values;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Span<T> MobiusDivisor(Span<T> values)
		{
			int n = values.Length;
			var sieve = new bool[n];
			int m = n - 1;
			for (int p = 2; p < n; ++p) {
				if (sieve[p] == false) {
					for (int i = m / p; i > 0; --i) {
						sieve[i * p] = true;
						values[i * p] = _sub(values[i * p], values[i]);
					}
				}
			}

			return values;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Span<T> ZetaMultiple(Span<T> values)
		{
			int n = values.Length;
			var sieve = new bool[n];
			int m = n - 1;
			for (int p = 2; p < n; ++p) {
				if (sieve[p] == false) {
					for (int i = m / p; i > 0; --i) {
						sieve[i * p] = true;
						values[i] = _add(values[i], values[i * p]);
					}
				}
			}

			return values;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Span<T> MobiusMultiple(Span<T> values)
		{
			int n = values.Length;
			var sieve = new bool[n];
			for (int p = 2; p < n; ++p) {
				if (sieve[p] == false) {
					for (int i = 1; i * p < n; ++i) {
						sieve[i * p] = true;
						values[i] = _sub(values[i], values[i * p]);
					}
				}
			}

			return values;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V8
{
	public static class Convolution
	{
		public static long[] Convolve(ReadOnlySpan<long> a, ReadOnlySpan<long> b)
		{
			unchecked {
				var n = a.Length;
				var m = b.Length;

				if (n == 0 || m == 0) {
					return Array.Empty<long>();
				}

				const ulong Mod1 = FftMod1.MOD;
				const ulong Mod2 = FftMod2.MOD;
				const ulong Mod3 = FftMod3.MOD;
				const ulong M2M3 = Mod2 * Mod3;
				const ulong M1M3 = Mod1 * Mod3;
				const ulong M1M2 = Mod1 * Mod2;
				const ulong M1M2M3 = Mod1 * Mod2 * Mod3;

				ulong i1 = (ulong)InverseGCD((long)M2M3, (long)Mod1).x;
				ulong i2 = (ulong)InverseGCD((long)M1M3, (long)Mod2).x;
				ulong i3 = (ulong)InverseGCD((long)M1M2, (long)Mod3).x;

				var c1 = Convolve<FftMod1>(a, b);
				var c2 = Convolve<FftMod2>(a, b);
				var c3 = Convolve<FftMod3>(a, b);

				var c = new long[n + m - 1];

				Span<ulong> offset = stackalloc ulong[] { 0, 0, M1M2M3, 2 * M1M2M3, 3 * M1M2M3 };

				for (int i = 0; i < c.Length; i++) {
					ulong x = 0;
					x += c1[i] * i1 % Mod1 * M2M3;
					x += c2[i] * i2 % Mod2 * M1M3;
					x += c3[i] * i3 % Mod3 * M1M2;

					long diff = (long)c1[i] - SafeMod((long)x, (long)Mod1);
					if (diff < 0) {
						diff += (long)Mod1;
					}

					x -= offset[(int)(diff % offset.Length)];
					c[i] = (long)x;
				}

				return c;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static ulong[] Convolve<TMod>(ReadOnlySpan<long> a, ReadOnlySpan<long> b)
				where TMod : struct, IFftMod
		{
			int z = 1 << CeilPow2(a.Length + b.Length - 1);

			var aTemp = new FftModInt<TMod>[z];
			for (int i = 0; i < a.Length; i++) {
				aTemp[i] = new FftModInt<TMod>(a[i]);
			}

			var bTemp = new FftModInt<TMod>[z];
			for (int i = 0; i < b.Length; i++) {
				bTemp[i] = new FftModInt<TMod>(b[i]);
			}

			var c = Convolve<TMod>(aTemp, bTemp, a.Length, b.Length, z);
			var result = new ulong[c.Length];
			for (int i = 0; i < result.Length; i++) {
				result[i] = (ulong)c[i].Value;
			}

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static Span<FftModInt<TMod>> Convolve<TMod>(
			Span<FftModInt<TMod>> a, Span<FftModInt<TMod>> b, int n, int m, int z)
			where TMod : struct, IFftMod
		{
			FftModInt<TMod>.Butterfly(a);
			FftModInt<TMod>.Butterfly(b);

			for (int i = 0; i < a.Length; i++) {
				a[i] *= b[i];
			}

			FftModInt<TMod>.ButterflyInv(a);
			var result = a[0..(n + m - 1)];
			var iz = new FftModInt<TMod>(z).Inv();
			foreach (ref var r in result) {
				r *= iz;
			}

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static long SafeMod(long x, long m)
		{
			x %= m;
			if (x < 0) {
				x += m;
			}

			return x;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static (long g, long x) InverseGCD(long a, long b)
		{
			a = SafeMod(a, b);
			if (a == 0) {
				return (b, 0);
			}

			long s = b;
			long t = a;
			long m0 = 0;
			long m1 = 1;

			long u;
			while (true) {
				if (t == 0) {
					if (m0 < 0) {
						m0 += b / s;
					}

					return (s, m0);
				}

				u = s / t;
				s -= t * u;
				m0 -= m1 * u;

				if (s == 0) {
					if (m1 < 0) {
						m1 += b / t;
					}

					return (t, m1);
				}

				u = t / s;
				t -= s * u;
				m1 -= m0 * u;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int CeilPow2(int n)
		{
			var un = (uint)n;
			if (un <= 1) {
				return 0;
			}

			return BitOperations.Log2(un - 1) + 1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int BitScanForward(uint n)
		{
			return BitOperations.TrailingZeroCount(n);
		}

		private interface IFftMod
		{
			uint Mod { get; }
			int PrimitiveRoot { get; }
			bool IsPrime { get; }
		}

		private readonly struct FftMod1 : IFftMod
		{
			public const uint MOD = 754974721;
			public uint Mod => MOD;
			public int PrimitiveRoot => 11;
			public bool IsPrime => true;
		}

		private readonly struct FftMod2 : IFftMod
		{
			public const uint MOD = 167772161;
			public uint Mod => MOD;
			public int PrimitiveRoot => 3;
			public bool IsPrime => true;
		}

		private readonly struct FftMod3 : IFftMod
		{
			public const uint MOD = 469762049;
			public uint Mod => MOD;
			public int PrimitiveRoot => 3;
			public bool IsPrime => true;
		}

		private readonly struct FftModInt<T> where T : struct, IFftMod
		{
			private readonly uint v_;
			private static readonly uint mod_ = default(T).Mod;
			private static readonly FftModInt<T>[] sumE_ = CalcurateSumE();
			private static readonly FftModInt<T>[] sumIE_ = CalcurateSumIE();

			public int Value => (int)v_;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static FftModInt<T> Raw(int v)
			{
				var u = unchecked((uint)v);
				return new FftModInt<T>(u);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static void Butterfly(Span<FftModInt<T>> a)
			{
				var n = a.Length;
				var h = CeilPow2(n);
				var registerLength = Vector<uint>.Count;
				var modVector = new Vector<uint>(mod_);
				for (int ph = 1; ph <= h; ph++) {
					int w = 1 << ph - 1;
					int p = 1 << h - ph;
					var now = Raw(1);
					int shift = h - ph + 1;
					for (int s = 0; s < w; s++) {
						int offset = s << shift;
						if (p < registerLength) {
							for (int i = 0; i < p; i++) {
								var l = a[i + offset];
								var r = a[i + offset + p] * now;
								a[i + offset] = l + r;
								a[i + offset + p] = l - r;
							}
						} else {
							var spanL = a.Slice(offset, p);
							var spanR = a.Slice(offset + p, p);
							foreach (ref var r in spanR) {
								r *= now;
							}

							var spanL_u = MemoryMarshal.Cast<FftModInt<T>, uint>(spanL);
							var spanR_u = MemoryMarshal.Cast<FftModInt<T>, uint>(spanR);
							for (int i = 0; i < spanL_u.Length; i += registerLength) {
								var targetL = spanL_u[i..];
								var targetR = spanR_u[i..];
								var l = new Vector<uint>(targetL);
								var r = new Vector<uint>(targetR);
								var add = l + r;
								var sub = l - r;
								var condition = Vector.GreaterThanOrEqual(add, modVector);
								add = Vector.ConditionalSelect(condition, add - modVector, add);
								condition = Vector.GreaterThanOrEqual(sub, modVector);
								sub = Vector.ConditionalSelect(condition, sub + modVector, sub);
								add.CopyTo(targetL);
								sub.CopyTo(targetR);
							}
						}

						now *= sumE_[BitScanForward(~(uint)s)];
					}
				}
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static void ButterflyInv(Span<FftModInt<T>> a)
			{
				var n = a.Length;
				var h = CeilPow2(n);
				var registerLength = Vector<uint>.Count;
				var modVector = new Vector<uint>(mod_);
				for (int ph = h; ph >= 1; ph--) {
					int w = 1 << ph - 1;
					int p = 1 << h - ph;
					var inverseNow = Raw(1);
					int shift = h - ph + 1;
					for (int s = 0; s < w; s++) {
						int offset = s << shift;
						if (p < registerLength) {
							for (int i = 0; i < p; i++) {
								var l = a[i + offset];
								var r = a[i + offset + p];
								a[i + offset] = l + r;
								a[i + offset + p] = Raw(
									unchecked((int)((ulong)(mod_ + l.Value - r.Value)
										* (ulong)inverseNow.Value % default(T).Mod)));
							}
						} else {
							var spanL = a.Slice(offset, p);
							var spanR = a.Slice(offset + p, p);
							var spanL_u = MemoryMarshal.Cast<FftModInt<T>, uint>(spanL);
							var spanR_u = MemoryMarshal.Cast<FftModInt<T>, uint>(spanR);
							for (int i = 0; i < spanL_u.Length; i += registerLength) {
								var targetL = spanL_u[i..];
								var targetR = spanR_u[i..];
								var l = new Vector<uint>(targetL);
								var r = new Vector<uint>(targetR);
								var add = l + r;
								var sub = l - r;
								var condition = Vector.GreaterThanOrEqual(add, modVector);
								add = Vector.ConditionalSelect(condition, add - modVector, add);
								condition = Vector.GreaterThanOrEqual(sub, modVector);
								sub = Vector.ConditionalSelect(condition, sub + modVector, sub);
								add.CopyTo(targetL);
								sub.CopyTo(targetR);
							}

							foreach (ref var r in spanR) {
								r *= inverseNow;
							}
						}

						inverseNow *= sumIE_[BitScanForward(~(uint)s)];
					}
				}
			}

			public FftModInt(long v) : this(Round(v)) { }
			private FftModInt(uint v) => v_ = v;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static uint Round(long v)
			{
				var x = v % default(T).Mod;
				if (x < 0) {
					x += mod_;
				}
				return (uint)x;
			}

			public static FftModInt<T> operator ++(FftModInt<T> value)
			{
				var v = value.v_ + 1;
				if (v == mod_) {
					v = 0;
				}
				return new FftModInt<T>(v);
			}

			public static FftModInt<T> operator --(FftModInt<T> value)
			{
				var v = value.v_;
				if (v == 0) {
					v = mod_;
				}
				return new FftModInt<T>(v - 1);
			}

			public static FftModInt<T> operator +(FftModInt<T> lhs, FftModInt<T> rhs)
			{
				var v = lhs.v_ + rhs.v_;
				if (v >= mod_) {
					v -= mod_;
				}
				return new FftModInt<T>(v);
			}

			public static FftModInt<T> operator -(FftModInt<T> lhs, FftModInt<T> rhs)
			{
				unchecked {
					var v = lhs.v_ - rhs.v_;
					if (v >= mod_) {
						v += mod_;
					}
					return new FftModInt<T>(v);
				}
			}

			public static FftModInt<T> operator *(FftModInt<T> lhs, FftModInt<T> rhs)
			{
				return new FftModInt<T>((uint)((ulong)lhs.v_ * rhs.v_ % default(T).Mod));
			}

			public static FftModInt<T> operator /(FftModInt<T> lhs, FftModInt<T> rhs)
				=> lhs * rhs.Inv();

			public static FftModInt<T> operator +(FftModInt<T> value) => value;
			public static FftModInt<T> operator -(FftModInt<T> value) => new FftModInt<T>() - value;
			public static bool operator ==(FftModInt<T> lhs, FftModInt<T> rhs) => lhs.v_ == rhs.v_;
			public static bool operator !=(FftModInt<T> lhs, FftModInt<T> rhs) => lhs.v_ != rhs.v_;
			public static implicit operator FftModInt<T>(int value) => new FftModInt<T>(value);
			public static implicit operator FftModInt<T>(long value) => new FftModInt<T>(value);

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public FftModInt<T> Pow(long n)
			{
				var x = this;
				var r = new FftModInt<T>(1u);

				while (n > 0) {
					if ((n & 1) > 0) {
						r *= x;
					}
					x *= x;
					n >>= 1;
				}

				return r;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public FftModInt<T> Inv()
			{
				if (default(T).IsPrime) {
					return Pow(default(T).Mod - 2);
				} else {
					var (_, x) = InverseGCD(v_, mod_);
					return new FftModInt<T>(x);
				}
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public override string ToString() => v_.ToString();
			public override bool Equals(object obj) => obj is FftModInt<T> value && this == value;
			public override int GetHashCode() => v_.GetHashCode();

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static FftModInt<T>[] CalcurateSumE()
			{
				int g = default(T).PrimitiveRoot;
				int cnt2 = BitScanForward(default(T).Mod - 1);
				var e = new FftModInt<T>(g).Pow(default(T).Mod - 1 >> cnt2);
				var ie = e.Inv();

				var sumE = new FftModInt<T>[cnt2 - 2];

				Span<FftModInt<T>> es = stackalloc FftModInt<T>[cnt2 - 1];
				Span<FftModInt<T>> ies = stackalloc FftModInt<T>[cnt2 - 1];

				for (int i = es.Length - 1; i >= 0; i--) {
					es[i] = e;
					ies[i] = ie;
					e *= e;
					ie *= ie;
				}

				var now = Raw(1);
				for (int i = 0; i < sumE.Length; i++) {
					sumE[i] = es[i] * now;
					now *= ies[i];
				}

				return sumE;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static FftModInt<T>[] CalcurateSumIE()
			{
				int g = default(T).PrimitiveRoot;
				int cnt2 = BitScanForward(default(T).Mod - 1);
				var e = new FftModInt<T>(g).Pow(default(T).Mod - 1 >> cnt2);
				var ie = e.Inv();

				var sumIE = new FftModInt<T>[cnt2 - 2];

				Span<FftModInt<T>> es = stackalloc FftModInt<T>[cnt2 - 1];
				Span<FftModInt<T>> ies = stackalloc FftModInt<T>[cnt2 - 1];

				for (int i = es.Length - 1; i >= 0; i--) {
					es[i] = e;
					ies[i] = ie;
					e *= e;
					ie *= ie;
				}

				var now = Raw(1);
				for (int i = 0; i < sumIE.Length; i++) {
					sumIE[i] = ies[i] * now;
					now *= es[i];
				}

				return sumIE;
			}
		}
	}
}

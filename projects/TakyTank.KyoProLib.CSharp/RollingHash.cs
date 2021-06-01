using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class RollingHash
	{
		private const ulong MASK30 = (1UL << 30) - 1;
		private const ulong MASK31 = (1UL << 31) - 1;
		private const ulong MOD = (1UL << 61) - 1;
		private const ulong POSITIVIZER = MOD * 4;

		private static readonly ulong base_;
		private static readonly List<ulong> basePows_ = new List<ulong>(1000) { 1 };
		static RollingHash()
		{
			var random = new Random();
			base_ = (uint)random.Next(1 << 10, int.MaxValue);
		}

		private readonly ulong[] hash_;
		public string Str { get; }
		public RollingHash(string s)
		{
			Str = s;
			int n = s.Length;
			for (int i = basePows_.Count; i <= n; i++) {
				basePows_.Add(CalculateMod(Multiply(basePows_[i - 1], base_)));
			}

			hash_ = new ulong[n + 1];
			for (int i = 0; i < n; i++) {
				hash_[i + 1] = CalculateMod(Multiply(hash_[i], base_) + s[i]);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ulong GetHash(Range range) => GetHash(range.Start.Value, range.End.Value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ulong GetHash(int a, int b)
		{
			return CalculateMod(hash_[b] + (POSITIVIZER - Multiply(hash_[a], basePows_[b - a])));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int LongestCommonPrefix(Range range1, RollingHash target, Range range2)
			=> LCP(range1.Start.Value, range1.End.Value, target, range2.Start.Value, range2.End.Value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int LCP(Range range1, RollingHash target, Range range2)
			=> LCP(range1.Start.Value, range1.End.Value, target, range2.Start.Value, range2.End.Value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int LongestCommonPrefix(int l1, int r1, RollingHash target, int l2, int r2)
			=> LCP(l1, r1, target, l2, r2);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int LCP(int l1, int r1, RollingHash target, int l2, int r2)
		{
			int len = Math.Min(r1 - l1, r2 - l2);
			int ok = -1;
			int ng = len + 1;
			while (ng - ok > 1) {
				int mid = (ok + ng) / 2;
				if (GetHash(l1, l1 + mid) == target.GetHash(l2, l2 + mid)) {
					ok = mid;
				} else {
					ng = mid;
				}
			}

			return (ok);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ulong Multiply(ulong a, ulong b)
		{
			ulong msbA = a >> 31;
			ulong lsbA = a & MASK31;
			ulong msbB = b >> 31;
			ulong lsbB = b & MASK31;
			ulong mid = lsbA * msbB + msbA * lsbB;
			return msbA * msbB * 2 + (mid >> 30) + ((mid & MASK30) << 31) + lsbA * lsbB;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ulong CalculateMod(ulong x)
		{
			ulong res = (x >> 61) + (x & MOD);
			if (res >= MOD) {
				res -= MOD;
			}

			return res;
		}
	}
}

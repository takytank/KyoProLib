﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TakyTank.KyoProLib.CSharp.V8
{
	public static class Helper
	{
		public static long INF => 1L << 50;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Clamp<T>(this T value, T min, T max) where T : struct, IComparable<T>
		{
			if (value.CompareTo(min) <= 0) {
				return min;
			}

			if (value.CompareTo(max) >= 0) {
				return max;
			}

			return value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void UpdateMin<T>(this ref T target, T value) where T : struct, IComparable<T>
			=> target = target.CompareTo(value) > 0 ? value : target;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void UpdateMin<T>(this ref T target, T value, Action<T> onUpdated)
			where T : struct, IComparable<T>
		{
			if (target.CompareTo(value) > 0) {
				target = value;
				onUpdated(value);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void UpdateMax<T>(this ref T target, T value) where T : struct, IComparable<T>
			=> target = target.CompareTo(value) < 0 ? value : target;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void UpdateMax<T>(this ref T target, T value, Action<T> onUpdated)
			where T : struct, IComparable<T>
		{
			if (target.CompareTo(value) < 0) {
				target = value;
				onUpdated(value);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long BinarySearchOKNG(long ok, long ng, Func<long, bool> satisfies)
		{
			while (ng - ok > 1) {
				long mid = (ok + ng) / 2;
				if (satisfies(mid)) {
					ok = mid;
				} else {
					ng = mid;
				}
			}

			return ok;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long BinarySearchNGOK(long ng, long ok, Func<long, bool> satisfies)
		{
			while (ok - ng > 1) {
				long mid = (ok + ng) / 2;
				if (satisfies(mid)) {
					ok = mid;
				} else {
					ng = mid;
				}
			}

			return ok;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int LowerBound<T>(Span<T> array, T value) where T : IComparable<T>
		=> LowerBound(array, -1, array.Length, value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int LowerBound<T>(Span<T> array, int ng, int ok, T value)
			where T : IComparable<T>
		{
			while (ok - ng > 1) {
				int mid = (ok + ng) / 2;
				if (array[mid].CompareTo(value) >= 0) {
					ok = mid;
				} else {
					ng = mid;
				}
			}

			return ok;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int UpperBound<T>(Span<T> array, T value) where T : IComparable<T>
			=> UpperBound(array, -1, array.Length, value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int UpperBound<T>(Span<T> array, int ng, int ok, T value)
			where T : IComparable<T>
		{
			while (ok - ng > 1) {
				int mid = (ok + ng) / 2;
				if (array[mid].CompareTo(value) > 0) {
					ok = mid;
				} else {
					ng = mid;
				}
			}

			return ok;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T[] Array1<T>(int n, T initialValue) where T : struct
			=> new T[n].Fill(initialValue);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T[] Array1<T>(int n, Func<int, T> initializer)
			=> Enumerable.Range(0, n).Select(x => initializer(x)).ToArray();
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T[] Fill<T>(this T[] array, T value)
			where T : struct
		{
			array.AsSpan().Fill(value);
			return array;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T[,] Array2<T>(int n, int m, T initialValule) where T : struct
			=> new T[n, m].Fill(initialValule);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T[,] Array2<T>(int n, int m, Func<int, int, T> initializer)
		{
			var array = new T[n, m];
			for (int i = 0; i < n; ++i) {
				for (int j = 0; j < m; ++j) {
					array[i, j] = initializer(i, j);
				}
			}

			return array;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T[,] Fill<T>(this T[,] array, T initialValue)
			where T : struct
		{
			MemoryMarshal.CreateSpan<T>(ref array[0, 0], array.Length).Fill(initialValue);
			return array;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<T> AsSpan<T>(this T[,] array, int i)
			=> MemoryMarshal.CreateSpan<T>(ref array[i, 0], array.GetLength(1));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T[,,] Array3<T>(int n1, int n2, int n3, T initialValue)
			where T : struct
			=> new T[n1, n2, n3].Fill(initialValue);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T[,,] Fill<T>(this T[,,] array, T initialValue)
			where T : struct
		{
			MemoryMarshal.CreateSpan<T>(ref array[0, 0, 0], array.Length).Fill(initialValue);
			return array;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<T> AsSpan<T>(this T[,,] array, int i, int j)
			=> MemoryMarshal.CreateSpan<T>(ref array[i, j, 0], array.GetLength(2));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T[,,,] Array4<T>(int n1, int n2, int n3, int n4, T initialValue)
			where T : struct
			=> new T[n1, n2, n3, n4].Fill(initialValue);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T[,,,] Fill<T>(this T[,,,] array, T initialValue)
			where T : struct
		{
			MemoryMarshal.CreateSpan<T>(ref array[0, 0, 0, 0], array.Length).Fill(initialValue);
			return array;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<T> AsSpan<T>(this T[,,,] array, int i, int j, int k)
			=> MemoryMarshal.CreateSpan<T>(ref array[i, j, k, 0], array.GetLength(3));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T[] Merge<T>(ReadOnlySpan<T> first, ReadOnlySpan<T> second) where T : IComparable<T>
		{
			var ret = new T[first.Length + second.Length];
			int p = 0;
			int q = 0;
			while (p < first.Length || q < second.Length) {
				if (p == first.Length) {
					ret[p + q] = second[q];
					q++;
					continue;
				}

				if (q == second.Length) {
					ret[p + q] = first[p];
					p++;
					continue;
				}

				if (first[p].CompareTo(second[q]) < 0) {
					ret[p + q] = first[p];
					p++;
				} else {
					ret[p + q] = second[q];
					q++;
				}
			}

			return ret;
		}

		private static readonly int[] delta4_ = { 1, 0, -1, 0, 1 };
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ReadOnlySpan<(int i, int j)> Adjacence4(int i, int j, int imax, int jmax)
		{
			int p = 0;
			Span<(int i, int j)> adjacences = new (int i, int j)[4];
			for (int dn = 0; dn < 4; ++dn) {
				int d4i = i + delta4_[dn];
				int d4j = j + delta4_[dn + 1];
				if ((uint)d4i < (uint)imax && (uint)d4j < (uint)jmax) {
					adjacences[p] = (d4i, d4j);
					++p;
				}
			}

			return adjacences[..p];
		}

		private static readonly int[] delta8_ = { 1, 0, -1, 0, 1, 1, -1, -1, 1 };
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ReadOnlySpan<(int i, int j)> Adjacence8(int i, int j, int imax, int jmax)
		{
			int p = 0;
			Span<(int i, int j)> adjacences = new (int i, int j)[8];
			for (int dn = 0; dn < 8; ++dn) {
				int d8i = i + delta8_[dn];
				int d8j = j + delta8_[dn + 1];
				if ((uint)d8i < (uint)imax && (uint)d8j < (uint)jmax) {
					adjacences[p] = (d8i, d8j);
					++p;
				}
			}

			return adjacences[..p];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<int> SubBitsOf(int bit)
		{
			for (int sub = bit; sub > 0; sub = --sub & bit) {
				yield return sub;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string Reverse(string src)
		{
			var chars = src.ToCharArray();
			for (int i = 0, j = chars.Length - 1; i < j; ++i, --j) {
				(chars[j], chars[i]) = (chars[i], chars[j]);
			}

			return new string(chars);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string Exchange(string src, char a, char b)
		{
			var chars = src.ToCharArray();
			for (int i = 0; i < chars.Length; i++) {
				if (chars[i] == a) {
					chars[i] = b;
				} else if (chars[i] == b) {
					chars[i] = a;
				}
			}

			return new string(chars);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Swap(this string str, int i, int j)
		{
			var span = str.AsWriteableSpan();
			(span[i], span[j]) = (span[j], span[i]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static char Replace(this string str, int index, char c)
		{
			var span = str.AsWriteableSpan();
			char old = span[index];
			span[index] = c;
			return old;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<char> ReverseInPlace(this string str)
			=> str.ReverseInPlace(0, str.Length);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<char> ReverseInPlace(this string str, int l, int r)
		{
			var span = str.AsWriteableSpan(l, r);
			for (int i = 0, j = span.Length - 1; i < j; ++i, --j) {
				(span[j], span[i]) = (span[i], span[j]);
			}

			return MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(span), span.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<char> AsWriteableSpan(this string str)
			=> str.AsWriteableSpan(0, str.Length);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<char> AsWriteableSpan(this string str, int l, int r)
		{
			var span = str.AsSpan(l, r - l);
			return MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(span), span.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string Join<T>(this IEnumerable<T> values, string separator = "")
			=> string.Join(separator, values);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string JoinNL<T>(this IEnumerable<T> values)
			=> string.Join(Environment.NewLine, values);
	}
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CS8
{
	public static class Helper
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void UpdateMin<T>(ref T target, T value) where T : IComparable<T>
			=> target = target.CompareTo(value) > 0 ? value : target;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void UpdateMin<T>(ref T target, T value, Action<T> onUpdated) where T : IComparable<T>
		{
			if (target.CompareTo(value) > 0) {
				target = value;
				onUpdated(value);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void UpdateMax<T>(ref T target, T value) where T : IComparable<T>
			=> target = target.CompareTo(value) < 0 ? value : target;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void UpdateMax<T>(ref T target, T value, Action<T> onUpdated) where T : IComparable<T>
		{
			if (target.CompareTo(value) < 0) {
				target = value;
				onUpdated(value);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T[] Array<T>(int n, Func<int, T> init)
			=> Enumerable.Range(0, n).Select(x => init(x)).ToArray();
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static List<T> List<T>(int n, Func<int, T> init)
			=> Enumerable.Range(0, n).Select(x => init(x)).ToList();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T[,] Array2<T>(int n, int m, T init)
			where T : struct
		{
			var array = new T[n, m];
			for (int i = 0; i < n; i++) {
				for (int j = 0; j < m; j++) {
					array[i, j] = init;
				}
			}

			return array;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T[,] Array2<T>(int n, int m, Func<int, int, T> initializer)
		{
			var array = new T[n, m];
			for (int i = 0; i < n; i++) {
				for (int j = 0; j < m; j++) {
					array[i, j] = initializer(i, j);
				}
			}

			return array;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T[,,] Array3<T>(int n1, int n2, int n3, T init)
			where T : struct
		{
			var array = new T[n1, n2, n3];
			for (int i1 = 0; i1 < n1; i1++) {
				for (int i2 = 0; i2 < n2; i2++) {
					for (int i3 = 0; i3 < n3; i3++) {
						array[i1, i2, i3] = init;
					}
				}
			}

			return array;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T[,,,] Array4<T>(int n1, int n2, int n3, int n4, T init)
			where T : struct
		{
			var array = new T[n1, n2, n3, n4];
			for (int i1 = 0; i1 < n1; i1++) {
				for (int i2 = 0; i2 < n2; i2++) {
					for (int i3 = 0; i3 < n3; i3++) {
						for (int i4 = 0; i4 < n4; i4++) {
							array[i1, i2, i3, i4] = init;
						}
					}
				}
			}

			return array;
		}

		private static readonly int[] delta4_ = { 1, 0, -1, 0, 1 };
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void DoAt4(int i, int j, int imax, int jmax, Action<int, int> action)
		{
			for (int dn = 0; dn < 4; dn++) {
				int d4i = i + delta4_[dn];
				int d4j = j + delta4_[dn + 1];
				if ((uint)d4i < (uint)imax && (uint)d4j < (uint)jmax) {
					action(d4i, d4j);
				}
			}
		}

		private static readonly int[] delta8_ = { 1, 0, -1, 0, 1, 1, -1, -1, 1 };
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void DoAt8(int i, int j, int imax, int jmax, Action<int, int> action)
		{
			for (int dn = 0; dn < 8; dn++) {
				int d8i = i + delta8_[dn];
				int d8j = j + delta8_[dn + 1];
				if ((uint)d8i < (uint)imax && (uint)d8j < (uint)jmax) {
					action(d8i, d8j);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ForEachSubBit(int bit, Action<int> action)
		{
			for (int sub = bit; sub >= 0; sub--) {
				sub &= bit;
				action(sub);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string Reverse(string src)
		{
			var chars = src.ToCharArray();
			for (int i = 0, j = chars.Length - 1; i < j; i++, j--) {
				var tmp = chars[i];
				chars[i] = chars[j];
				chars[j] = tmp;
			}
			return new string(chars);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string Join<T>(this IEnumerable<T> values, string separator = "")
			=> string.Join(separator, values);
	}
}

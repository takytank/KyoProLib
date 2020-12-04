using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public static class Helper
	{
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

		private static readonly int[] delta4_ = { 1, 0, -1, 0, 1 };
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void DoIn4(int i, int j, int imax, int jmax, Action<int, int> action)
		{
			for (int dn = 0; dn < 4; ++dn) {
				int d4i = i + delta4_[dn];
				int d4j = j + delta4_[dn + 1];
				if ((uint)d4i < (uint)imax && (uint)d4j < (uint)jmax) {
					action(d4i, d4j);
				}
			}
		}

		private static readonly int[] delta8_ = { 1, 0, -1, 0, 1, 1, -1, -1, 1 };
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void DoIn8(int i, int j, int imax, int jmax, Action<int, int> action)
		{
			for (int dn = 0; dn < 8; ++dn) {
				int d8i = i + delta8_[dn];
				int d8j = j + delta8_[dn + 1];
				if ((uint)d8i < (uint)imax && (uint)d8j < (uint)jmax) {
					action(d8i, d8j);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ForEachSubBits(int bit, Action<int> action)
		{
			for (int sub = bit; sub >= 0; --sub) {
				sub &= bit;
				action(sub);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string Reverse(string src)
		{
			var chars = src.ToCharArray();
			for (int i = 0, j = chars.Length - 1; i < j; ++i, --j) {
				var tmp = chars[i];
				chars[i] = chars[j];
				chars[j] = tmp;
			}

			return new string(chars);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string Join<T>(this IEnumerable<T> values, string separator = "")
			=> string.Join(separator, values);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string JoinNL<T>(this IEnumerable<T> values)
			=> string.Join(Environment.NewLine, values);
	}
}

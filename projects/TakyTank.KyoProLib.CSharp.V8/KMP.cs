using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V8
{
	public static class KMP
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static List<int> Search(string text, string pattern)
			=> Search(text, pattern, LongestStrictBorder(pattern));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static List<int> Search(string text, string pattern, int[] longestBorder)
		{
			int m = pattern.Length;
			var matchIndex = new List<int>();
			int i = 0;
			int j = 0;
			while (i + j < text.Length) {
				if (pattern[j] == text[i + j]) {
					if (++j != m) {
						continue;
					}

					matchIndex.Add(i);
				}

				i += j - longestBorder[j];
				j = Math.Max(longestBorder[j], 0);
			}

			return matchIndex;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int[] MinimumCycle(string s)
			=> MinimumCycle(s, LongestBorder(s));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int[] MinimumCycle(string s, int[] longestBorder)
		{
			var cycleLength = new int[s.Length];
			for (int i = 0; i < s.Length; ++i) {
				cycleLength[i] = i + 1 - longestBorder[i + 1];
			}

			return cycleLength;
		}

		// MP
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int[] LongestBorder(string s)
		{
			int l = s.Length;
			var a = new int[l + 1];
			a[0] = -1;
			int j = -1;
			for (int i = 0; i < l; ++i) {
				while (j >= 0 && s[i] != s[j]) {
					j = a[j];
				}

				++j;
				a[i + 1] = j;
			}

			return a;
		}

		// KMP
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int[] LongestStrictBorder(string s)
		{
			int l = s.Length;
			var a = new int[l + 1];
			a[0] = -1;
			int j = -1;
			for (int i = 0; i < l; ++i) {
				while (j >= 0 && s[i] != s[j]) {
					j = a[j];
				}

				++j;
				if (i < l && s[i + 1] == s[j]) {
					a[i + 1] = a[j];
				} else {
					a[i + 1] = j;
				}
			}

			return a;
		}
	}

	public static class KMP<T>
		where T : IComparable<T>
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static List<int> Search(ReadOnlySpan<T> text, ReadOnlySpan<T> pattern)
			=> Search(text, pattern, LongestStrictBorder(pattern));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static List<int> Search(
			ReadOnlySpan<T> text, ReadOnlySpan<T> pattern, int[] longestBorder)
		{
			int m = pattern.Length;
			var matchIndex = new List<int>();
			int i = 0;
			int j = 0;
			while (i + j < text.Length) {
				if (pattern[j].CompareTo(text[i + j]) == 0) {
					++j;
					if (j != m) {
						continue;
					}

					matchIndex.Add(i);
				}

				i += j - longestBorder[j];
				j = Math.Max(longestBorder[j], 0);
			}

			return matchIndex;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int[] MinimumCycle(ReadOnlySpan<T> s)
			=> MinimumCycle(s, LongestBorder(s));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int[] MinimumCycle(ReadOnlySpan<T> s, int[] longestBorder)
		{
			var cycleLength = new int[s.Length];
			for (int i = 0; i < s.Length; ++i) {
				cycleLength[i] = i + 1 - longestBorder[i + 1];
			}

			return cycleLength;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int[] LongestBorder(ReadOnlySpan<T> s)
		{
			int l = s.Length;
			var a = new int[l + 1];
			a[0] = -1;
			int j = -1;
			for (int i = 0; i < l; ++i) {
				while (j >= 0 && s[i].CompareTo(s[j]) != 0) {
					j = a[j];
				}

				++j;
				a[i + 1] = j;
			}

			return a;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int[] LongestStrictBorder(ReadOnlySpan<T> s)
		{
			int l = s.Length;
			var a = new int[l + 1];
			a[0] = -1;
			int j = -1;
			for (int i = 0; i < l; ++i) {
				while (j >= 0 && s[i].CompareTo(s[j]) != 0) {
					j = a[j];
				}

				++j;
				if (i < l - 1 && s[i + 1].CompareTo(s[j]) == 0) {
					a[i + 1] = a[j];
				} else {
					a[i + 1] = j;
				}
			}

			return a;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V11
{
	public static class Permutation
	{
		public static IEnumerable<int[]> All(int n, int offset = 0)
		{
			var src = Enumerable.Range(1 + offset, n).ToArray();
			do {
				yield return src;
			} while (Next(src, 0, n));
		}

		public static bool Next(int[] src) => Next(src, 0, src.Length);
		public static bool Next(int[] src, int index, int length)
		{
			if (length <= 1) {
				return false;
			}

			int last = index + length - 1;
			int i = last;
			while (true) {
				int ii = i;
				--i;
				if (src[i] < src[ii]) {
					int j = last;
					while (src[i] >= src[j]) {
						--j;
					}

					(src[j], src[i]) = (src[i], src[j]);
					Array.Reverse(src, ii, last - ii + 1);
					return true;
				}

				if (i == index) {
					Array.Reverse(src, index, length);
					return false;
				}
			}
		}

		public static bool Prev(int[] src) => Prev(src, 0, src.Length);
		public static bool Prev(int[] src, int index, int length)
		{
			if (length <= 1) {
				return false;
			}

			int last = index + length - 1;
			int i = last;
			while (true) {
				int ii = i;
				--i;
				if (src[ii] < src[i]) {
					int j = last;
					while (src[j] >= src[i]) {
						--j;
					}

					(src[j], src[i]) = (src[i], src[j]);
					Array.Reverse(src, ii, last - ii + 1);
					return true;
				}

				if (i == index) {
					Array.Reverse(src, index, length);
					return false;
				}
			}
		}

		public static IEnumerable<T[]> All<T>(T[] src)
			where T : IComparable<T>
		{
			int n = src.Length;
			do {
				yield return src;
			} while (Next<T>(src, 0, n));
		}

		public static bool Next<T>(T[] src) where T : IComparable<T>
			=> Next(src, 0, src.Length);
		public static bool Next<T>(T[] src, int index, int length)
			where T : IComparable<T>
		{
			if (length <= 1) {
				return false;
			}

			int last = index + length - 1;
			int i = last;
			while (true) {
				int ii = i;
				--i;
				if (src[i].CompareTo(src[ii]) < 0) {
					int j = last;
					while (src[i].CompareTo(src[j]) >= 0) {
						--j;
					}

					(src[j], src[i]) = (src[i], src[j]);
					Array.Reverse(src, ii, last - ii + 1);
					return true;
				}

				if (i == index) {
					Array.Reverse(src, index, length);
					return false;
				}
			}
		}

		public static bool Prev<T>(T[] src) where T : IComparable<T>
			=> Prev(src, 0, src.Length);
		public static bool Prev<T>(T[] src, int index, int length)
			where T : IComparable<T>
		{
			if (length <= 1) {
				return false;
			}

			int last = index + length - 1;
			int i = last;
			while (true) {
				int ii = i;
				--i;
				if (src[ii].CompareTo(src[i]) < 0) {
					int j = last;
					while (src[j].CompareTo(src[i]) >= 0) {
						--j;
					}

					(src[j], src[i]) = (src[i], src[j]);
					Array.Reverse(src, ii, last - ii + 1);
					return true;
				}

				if (i == index) {
					Array.Reverse(src, index, length);
					return false;
				}
			}
		}

		public static IReadOnlyList<T[]> AllOfNotComparable<T>(IEnumerable<T> src)
		{
			var perms = new List<T[]>();
			Search(perms, new Stack<T>(), src.ToArray());
			return perms;
		}

		private static void Search<T>(List<T[]> perms, Stack<T> stack, T[] a)
		{
			int n = a.Length;
			if (n == 0) {
				perms.Add(stack.Reverse().ToArray());
			} else {
				var b = new T[n - 1];
				Array.Copy(a, 1, b, 0, n - 1);
				for (int i = 0; i < a.Length; ++i) {
					stack.Push(a[i]);
					Search(perms, stack, b);
					if (i < b.Length) {
						b[i] = a[i];
					}

					stack.Pop();
				}
			}
		}
	}
}

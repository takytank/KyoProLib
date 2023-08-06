using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V11
{
	public static class SuffixArray
	{
		//original: ttps://github.com/key-moon/ac-library-cs
		public static int[] SuffixArrayOf(string s)
			=> SAIS(s.Select(c => (int)c).ToArray(), char.MaxValue);
		public static int[] SAIS(ReadOnlyMemory<int> memory, int upper) => SAIS(memory, upper, 10, 40);
		public static int[] SAIS(ReadOnlyMemory<int> memory, int upper, int thresholdNaive, int thresholdDouling)
		{
			var s = memory.Span;
			var n = s.Length;
			if (n == 0) {
				return Array.Empty<int>();
			} else if (n == 1) {
				return new int[] { 0 };
			} else if (n == 2) {
				if (s[0] < s[1]) {
					return new int[] { 0, 1 };
				} else {
					return new int[] { 1, 0 };
				}
			} else if (n < thresholdNaive) {
				return SANaive(memory);
			} else if (n < thresholdDouling) {
				return SADoubling(memory);
			}

			var sa = new int[n];
			var isTypeS = new bool[n];
			for (int i = sa.Length - 2; i >= 0; --i) {
				isTypeS[i] = (s[i] == s[i + 1]) ? isTypeS[i + 1] : (s[i] < s[i + 1]);
			}

			var sumL = new int[upper + 1];
			var sumS = new int[upper + 1];

			for (int i = 0; i < s.Length; ++i) {
				if (!isTypeS[i]) {
					++sumS[s[i]];
				} else {
					++sumL[s[i] + 1];
				}
			}

			for (int i = 0; i < sumL.Length; ++i) {
				sumS[i] += sumL[i];
				if (i < upper) {
					sumL[i + 1] += sumS[i];
				}
			}

			var lmsMap = new int[n + 1];
			lmsMap.AsSpan().Fill(-1);
			int m = 0;
			for (int i = 1; i < isTypeS.Length; ++i) {
				if (!isTypeS[i - 1] && isTypeS[i]) {
					lmsMap[i] = m;
					++m;
				}
			}

			var lms = new List<int>(m);
			for (int i = 1; i < isTypeS.Length; ++i) {
				if (!isTypeS[i - 1] && isTypeS[i]) {
					lms.Add(i);
				}
			}

			void Induce(List<int> lms)
			{
				var s = memory.Span;
				sa.AsSpan().Fill(-1);
				var buf = new int[sumS.Length];

				sumS.AsSpan().CopyTo(buf);
				foreach (var d in lms) {
					if (d == n) {
						continue;
					}

					sa[buf[s[d]]] = d;
					++buf[s[d]];
				}

				sumL.AsSpan().CopyTo(buf);
				sa[buf[s[n - 1]]] = n - 1;
				++buf[s[n - 1]];
				for (int i = 0; i < sa.Length; ++i) {
					int v = sa[i];
					if (v >= 1 && !isTypeS[v - 1]) {
						sa[buf[s[v - 1]]] = v - 1;
						++buf[s[v - 1]];
					}
				}

				sumL.AsSpan().CopyTo(buf);
				for (int i = sa.Length - 1; i >= 0; --i) {
					int v = sa[i];
					if (v >= 1 && isTypeS[v - 1]) {
						--buf[s[v - 1] + 1];
						sa[buf[s[v - 1] + 1]] = v - 1;
					}
				}
			}

			Induce(lms);

			if (m > 0) {
				var sortedLms = new List<int>(m);
				foreach (var v in sa) {
					if (lmsMap[v] != -1) {
						sortedLms.Add(v);
					}
				}

				var recS = new int[m];
				var recUpper = 0;
				recS[lmsMap[sortedLms[0]]] = 0;
				for (int i = 1; i < sortedLms.Count; i++) {
					var l = sortedLms[i - 1];
					var r = sortedLms[i];
					var endL = (lmsMap[l] + 1 < m) ? lms[lmsMap[l] + 1] : n;
					var endR = (lmsMap[r] + 1 < m) ? lms[lmsMap[r] + 1] : n;
					var same = true;

					if (endL - l != endR - r) {
						same = false;
					} else {
						while (l < endL) {
							if (s[l] != s[r]) {
								break;
							}

							++l;
							++r;
						}

						if (l == n || s[l] != s[r]) {
							same = false;
						}
					}

					if (!same) {
						++recUpper;
					}

					recS[lmsMap[sortedLms[i]]] = recUpper;
				}

				var recSA = SAIS(recS, recUpper, thresholdNaive, thresholdDouling);
				for (int i = 0; i < sortedLms.Count; ++i) {
					sortedLms[i] = lms[recSA[i]];
				}

				Induce(sortedLms);
			}

			return sa;
		}

		private static int[] SANaive(ReadOnlyMemory<int> memory)
		{
			var n = memory.Length;
			var sa = Enumerable.Range(0, n).ToArray();

			int Compare(int l, int r)
			{
				var s = memory.Span;
				while (l < s.Length && r < s.Length) {
					if (s[l] != s[r]) {
						return s[l] - s[r];
					}

					++l;
					++r;
				}

				return r - l;
			}

			Array.Sort(sa, Compare);
			return sa;
		}

		private static int[] SADoubling(ReadOnlyMemory<int> memory)
		{
			var s = memory.Span;
			var n = s.Length;
			var sa = Enumerable.Range(0, n).ToArray();
			var rnk = new int[n];
			var tmp = new int[n];
			s.CopyTo(rnk);

			for (int k = 1; k < n; k <<= 1) {
				int Compare(int x, int y)
				{
					if (rnk[x] != rnk[y]) {
						return rnk[x] - rnk[y];
					}

					int rx = x + k < n ? rnk[x + k] : -1;
					int ry = y + k < n ? rnk[y + k] : -1;

					return rx - ry;
				}

				Array.Sort(sa, Compare);
				tmp[sa[0]] = 0;
				for (int i = 1; i < sa.Length; ++i) {
					tmp[sa[i]] = tmp[sa[i - 1]] + (Compare(sa[i - 1], sa[i]) < 0 ? 1 : 0);
				}

				(tmp, rnk) = (rnk, tmp);
			}

			return sa;
		}

		//original: ttps://github.com/key-moon/ac-library-cs
		public static int[] LcpArrayOf(string s, int[] saffixArray) => LcpArrayOf(s.AsSpan(), saffixArray);
		public static int[] LcpArrayOf<T>(T[] s, int[] saffixArray) => LcpArrayOf((ReadOnlySpan<T>)s, saffixArray);
		public static int[] LcpArrayOf<T>(ReadOnlySpan<T> s, int[] saffixArray)
		{
			int[] rnk = new int[s.Length];
			for (int i = 0; i < s.Length; ++i) {
				rnk[saffixArray[i]] = i;
			}

			int[] lcp = new int[s.Length - 1];
			int h = 0;
			for (int i = 0; i < s.Length; ++i) {
				if (h > 0) {
					--h;
				}

				if (rnk[i] == 0) {
					continue;
				}

				int j = saffixArray[rnk[i] - 1];
				for (; j + h < s.Length && i + h < s.Length; ++h) {
					if (!EqualityComparer<T>.Default.Equals(s[j + h], s[i + h])) {
						break;
					}
				}

				lcp[rnk[i] - 1] = h;
			}

			return lcp;
		}
	}
}

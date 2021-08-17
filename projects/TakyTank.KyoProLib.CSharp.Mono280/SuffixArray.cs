using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.Mono280
{
	public static class SuffixArray
	{
		public static int[] Create(string s)
		{
			return SAIS(s.Select(c => (int)c).ToArray(), char.MaxValue);
		}
		public static int[] SAIS(int[] memory, int upper)
		{
			return SAIS(memory, upper, 10, 40);
		}
		public static int[] SAIS(int[] memory, int upper, int thresholdNaive, int thresholdDouling)
		{
			var s = memory;
			var n = s.Length;
			if (n == 0) {
				return new int[0];
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
			for (int i = 0; i < n + 1; i++) {
				lmsMap[i] = -1;
			}

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

			Action<List<int>> Induce = lms2 => {
				for (int i = 0; i < sa.Length; i++) {
					sa[i] = -1;
				}

				var buf = new int[sumS.Length];

				Array.Copy(sumS, buf, sumS.Length);
				foreach (var d in lms2) {
					if (d == n) {
						continue;
					}

					sa[buf[s[d]]] = d;
					++buf[s[d]];
				}

				Array.Copy(sumL, buf, sumL.Length);
				sa[buf[s[n - 1]]] = n - 1;
				++buf[s[n - 1]];
				for (int i = 0; i < sa.Length; ++i) {
					int v = sa[i];
					if (v >= 1 && !isTypeS[v - 1]) {
						sa[buf[s[v - 1]]] = v - 1;
						++buf[s[v - 1]];
					}
				}

				Array.Copy(sumL, buf, sumL.Length);
				for (int i = sa.Length - 1; i >= 0; --i) {
					int v = sa[i];
					if (v >= 1 && isTypeS[v - 1]) {
						--buf[s[v - 1] + 1];
						sa[buf[s[v - 1] + 1]] = v - 1;
					}
				}
			};

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

		private static int[] SANaive(int[] memory)
		{
			var n = memory.Length;
			var sa = Enumerable.Range(0, n).ToArray();

			var s = memory;
			Comparison<int> compare = (l, r) => {
				while (l < s.Length && r < s.Length) {
					if (s[l] != s[r]) {
						return s[l] - s[r];
					}

					++l;
					++r;
				}

				return r - l;
			};

			Array.Sort(sa, compare);
			return sa;
		}

		private static int[] SADoubling(int[] memory)
		{
			var s = memory;
			var n = s.Length;
			var sa = Enumerable.Range(0, n).ToArray();
			var rnk = new int[n];
			var tmp = new int[n];
			Array.Copy(s, rnk, n);

			for (int k = 1; k < n; k <<= 1) {
				Comparison<int> compre = (x, y) => {
					if (rnk[x] != rnk[y]) {
						return rnk[x] - rnk[y];
					}

					int rx = x + k < n ? rnk[x + k] : -1;
					int ry = y + k < n ? rnk[y + k] : -1;

					return rx - ry;
				};

				Array.Sort(sa, compre);
				tmp[sa[0]] = 0;
				for (int i = 1; i < sa.Length; ++i) {
					tmp[sa[i]] = tmp[sa[i - 1]] + (compre(sa[i - 1], sa[i]) < 0 ? 1 : 0);
				}

				var tt = tmp;
				tmp = rnk;
				rnk = tt;
			}

			return sa;
		}
	}
}

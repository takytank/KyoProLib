using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V11
{
	public static class DnaAlignment
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static (T[] alignedS, T[] alignedt, int score) Global<T>(
			ReadOnlySpan<T> s,
			ReadOnlySpan<T> t,
			Func<T, T, int> getScore,
			T gapCharacter,
			int gapScore)
			=> NeedlemanWunsch(s, t, getScore, gapCharacter, gapScore);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static (T[] alignedS, T[] alignedt, int score) NeedlemanWunsch<T>(
			ReadOnlySpan<T> s,
			ReadOnlySpan<T> t,
			Func<T, T, int> getScore,
			T gapCharacter,
			int gapScore)
		{
			int n = s.Length;
			int m = t.Length;

			var dp = new int[n + 1, m + 1].Fill(int.MinValue);
			dp[0, 0] = 0;
			var prev = new Direction[n + 1, m + 1];

			for (int i = 0; i <= n; i++) {
				for (int j = 0; j <= m; j++) {
					if (dp[i, j] == int.MinValue) {
						continue;
					}

					int nextScore;
					if (i < n && j < m) {
						nextScore = dp[i, j] + getScore(s[i], t[j]);
						if (dp[i + 1, j + 1] < nextScore) {
							dp[i + 1, j + 1] = nextScore;
							prev[i + 1, j + 1] = Direction.BottomRight;
						}
					}

					nextScore = dp[i, j] + gapScore;
					if (i < n && dp[i + 1, j] < nextScore) {
						dp[i + 1, j] = nextScore;
						prev[i + 1, j] = Direction.Bottom;
					}

					if (j < m && dp[i, j + 1] < nextScore) {
						dp[i, j + 1] = nextScore;
						prev[i, j + 1] = Direction.Right;
					}
				}
			}

			var ss = new List<T>();
			var tt = new List<T>();
			int ii = n;
			int jj = m;
			while (ii > 0 || jj > 0) {
				switch (prev[ii, jj]) {
					case Direction.BottomRight:
						ss.Add(s[ii - 1]);
						tt.Add(t[jj - 1]);
						--ii;
						--jj;
						break;
					case Direction.Bottom:
						ss.Add(s[ii - 1]);
						tt.Add(gapCharacter);
						--ii;
						break;
					case Direction.Right:
						ss.Add(gapCharacter);
						tt.Add(t[jj - 1]);
						--jj;
						break;
					default:
						break;
				}
			}

			ss.Reverse();
			tt.Reverse();

			return (ss.ToArray(), tt.ToArray(), dp[n - 1, m - 1]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static (T[] alignedS, T[] alignedt, int score) Local<T>(
			ReadOnlySpan<T> s,
			ReadOnlySpan<T> t,
			Func<T, T, int> getScore,
			T gapCharacter,
			int gapScore)
			=> SmithWaterman(s, t, getScore, gapCharacter, gapScore);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static (T[] alignedS, T[] alignedt, int score) SmithWaterman<T>(
			ReadOnlySpan<T> s,
			ReadOnlySpan<T> t,
			Func<T, T, int> getScore,
			T gapCharacter,
			int gapScore)
		{
			int n = s.Length;
			int m = t.Length;

			var dp = new int[n + 1, m + 1].Fill(int.MinValue);
			dp[0, 0] = 0;
			var prev = new Direction[n + 1, m + 1];

			for (int i = 0; i <= n; i++) {
				for (int j = 0; j <= m; j++) {
					if (dp[i, j] == int.MinValue) {
						continue;
					}

					int nextScore;
					if (i < n && j < m) {
						nextScore = Math.Max(0, dp[i, j] + getScore(s[i], t[j]));
						if (dp[i + 1, j + 1] < nextScore) {
							dp[i + 1, j + 1] = nextScore;
							prev[i + 1, j + 1] = Direction.BottomRight;
						}
					}

					nextScore = Math.Max(0, dp[i, j] + gapScore);
					if (i < n && dp[i + 1, j] < nextScore) {
						dp[i + 1, j] = nextScore;
						prev[i + 1, j] = Direction.Bottom;
					}

					if (j < m && dp[i, j + 1] < nextScore) {
						dp[i, j + 1] = nextScore;
						prev[i, j + 1] = Direction.Right;
					}
				}
			}

			var ss = new List<T>();
			var tt = new List<T>();
			int ii = n;
			int jj = m;
			int max = int.MinValue;
			for (int i = 0; i <= n; i++) {
				for (int j = 0; j <= m; j++) {
					if (dp[i, j] >= max) {
						ii = i;
						jj = j;
						max = dp[i, j];
					}
				}
			}

			while (ii > 0 || jj > 0) {
				if (dp[ii, jj] <= 0) {
					break;
				}

				switch (prev[ii, jj]) {
					case Direction.BottomRight:
						ss.Add(s[ii - 1]);
						tt.Add(t[jj - 1]);
						--ii;
						--jj;
						break;
					case Direction.Bottom:
						ss.Add(s[ii - 1]);
						tt.Add(gapCharacter);
						--ii;
						break;
					case Direction.Right:
						ss.Add(gapCharacter);
						tt.Add(t[jj - 1]);
						--jj;
						break;
					default:
						break;
				}
			}

			ss.Reverse();
			tt.Reverse();

			return (ss.ToArray(), tt.ToArray(), dp[n - 1, m - 1]);
		}

		private enum Direction : byte
		{
			BottomRight = 0,
			Bottom,
			Right,
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TakyTank.KyoProLib.CSharp.V11
{
	public static class LIS
	{
		public static int[] Increase<T>(T[] array, T inifinity)
			where T : IComparable<T>
		{
			int n = array.Length;
			var ret = new int[n];
			var dp = new T[n];
			dp.AsSpan().Fill(inifinity);
			for (int i = 0; i < n; ++i) {
				int ng = -1;
				int ok = n - 1;
				while (ok - ng > 1) {
					int mid = (ng + ok) / 2;
					if (dp[mid].CompareTo(array[i]) >= 0) {
						ok = mid;
					} else {
						ng = mid;
					}
				}

				ret[i] = ok + 1;
				if (dp[ok].CompareTo(array[i]) > 0) {
					dp[ok] = array[i];
				}
			}

			return ret;
		}

		public static int[] IncreaseReverse<T>(T[] array, T infinity)
			where T : IComparable<T>
		{
			int n = array.Length;
			var ret = new int[n];
			var dp = new T[n];
			dp.AsSpan().Fill(infinity);
			for (int i = n - 1; i >= 0; --i) {
				int ng = -1;
				int ok = n - 1;
				while (ok - ng > 1) {
					int mid = (ng + ok) / 2;
					if (dp[mid].CompareTo(array[i]) >= 0) {
						ok = mid;
					} else {
						ng = mid;
					}
				}

				ret[i] = ok + 1;
				if (dp[ok].CompareTo(array[i]) > 0) {
					dp[ok] = array[i];
				}
			}

			return ret;
		}

		public static int[] Decrease<T>(T[] array, T negativeInfinity)
			where T : IComparable<T>
		{
			int n = array.Length;
			var ret = new int[n];
			var dp = new T[n];
			dp.AsSpan().Fill(negativeInfinity);
			for (int i = 0; i < n; ++i) {
				int ng = -1;
				int ok = n - 1;
				while (ok - ng > 1) {
					int mid = (ng + ok) / 2;
					if (dp[mid].CompareTo(array[i]) <= 0) {
						ok = mid;
					} else {
						ng = mid;
					}
				}

				ret[i] = ok + 1;
				if (dp[ok].CompareTo(array[i]) < 0) {
					dp[ok] = array[i];
				}
			}

			return ret;
		}

		public static int[] DecreaseReverse<T>(T[] array, T negativeInfinity)
			where T : IComparable<T>
		{
			int n = array.Length;
			var ret = new int[n];
			var dp = new T[n];
			dp.AsSpan().Fill(negativeInfinity);
			for (int i = n - 1; i >= 0; --i) {
				int ng = -1;
				int ok = n - 1;
				while (ok - ng > 1) {
					int mid = (ng + ok) / 2;
					if (dp[mid].CompareTo(array[i]) <= 0) {
						ok = mid;
					} else {
						ng = mid;
					}
				}

				ret[i] = ok + 1;
				if (dp[ok].CompareTo(array[i]) < 0) {
					dp[ok] = array[i];
				}
			}

			return ret;
		}
	}
}

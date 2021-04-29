using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public static class TernarySearch
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double MinDouble(double left, double right, Func<double, double> f, double delta)
		{
			while (right - left > delta) {
				double cl = (left * 2 + right) / 3;
				double cr = (left + right * 2) / 3;
				if (f(cl) > f(cr)) {
					left = cl;
				} else {
					right = cr;
				}
			}

			return left;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double MaxDouble(double left, double right, Func<double, double> f, double delta)
		{
			while (right - left > delta) {
				double cl = (left * 2 + right) / 3;
				double cr = (left + right * 2) / 3;
				if (f(cl) < f(cr)) {
					left = cl;
				} else {
					right = cr;
				}
			}

			return left;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Min(int left, int right, Func<int, long> f)
		{
			while (right - left > 1) {
				int cl = (left * 2 + right) / 3;
				int cr = (left + right * 2) / 3;
				if (f(cl) > f(cr)) {
					left = cl;
				} else {
					right = cr;
				}

				if (right - left == 2) {
					if (f(left) < f(left + 1)) {
						right = left + 1;
					} else {
						left++;
					}

					break;
				}
			}

			if (f(left) < f(right)) {
				return left;
			} else {
				return right;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Max(int left, int right, Func<int, long> f)
		{
			while (right - left > 1) {
				int cl = (left * 2 + right) / 3;
				int cr = (left + right * 2) / 3;
				if (f(cl) < f(cr)) {
					left = cl;
				} else {
					right = cr;
				}

				if (right - left == 2) {
					if (f(left) > f(left + 1)) {
						right = left + 1;
					} else {
						left++;
					}

					break;
				}
			}

			if (f(left) > f(right)) {
				return left;
			} else {
				return right;
			}
		}
	}
}

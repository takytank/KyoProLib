using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V11
{
	public static class TernarySearch
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static (double i, double min) MinDouble(double left, double right, Func<double, double> f, double delta)
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

			return (left, f(left));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static (double i, double max) MaxDouble(double left, double right, Func<double, double> f, double delta)
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

			return (left, f(left));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static (int i, long min) Min(int left, int right, Func<int, long> f)
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

			long lv = f(left);
			long rv = f(right);
			if (lv < rv) {
				return (left, lv);
			} else {
				return (right, rv);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static (int i, long max) Max(int left, int right, Func<int, long> f)
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

			long lv = f(left);
			long rv = f(right);
			if (lv > rv) {
				return (left, lv);
			} else {
				return (right, rv);
			}
		}
	}
}

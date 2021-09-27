using System;
using System.Collections.Generic;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V8
{
	public static class NoshiBasis
	{
		public static int[] Get32bitXorBasisOf(
			ReadOnlySpan<int> values, bool isRawValues = false)
		{
			int n = values.Length;
			var eliminateds = new List<int>();
			var raws = new List<int>();
			for (int j = n - 1; j >= 0; j--) {
				int v = values[j];
				foreach (var b in eliminateds) {
					v = Math.Min(v, v ^ b);
				}

				if (v > 0) {
					eliminateds.Add(v);
					raws.Add(values[j]);
				}
			}

			return isRawValues
				? raws.ToArray()
				: eliminateds.ToArray();
		}

		public static long[] Get64bitXorBasisOf(
			ReadOnlySpan<long> values, bool isRawValues = false)
		{
			int n = values.Length;
			var eliminateds = new List<long>();
			var raws = new List<long>();
			for (int j = n - 1; j >= 0; j--) {
				long v = values[j];
				foreach (var b in eliminateds) {
					v = Math.Min(v, v ^ b);
				}

				if (v > 0) {
					eliminateds.Add(v);
					raws.Add(values[j]);
				}
			}

			return isRawValues
				? raws.ToArray()
				: eliminateds.ToArray();
		}
	}
}

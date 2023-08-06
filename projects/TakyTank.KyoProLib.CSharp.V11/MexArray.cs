using System;
using System.Collections.Generic;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V11
{
	public static class MexArray
	{
		public static int[] Calculate(int[] ranges, int origin = 0)
		{
			int count = ranges.Length;
			int n = 1;
			while (n < count) {
				n <<= 1;
			}

			int minf = origin - 1;
			var tree = new int[n << 1];
			for (int i = 0; i < tree.Length; i++) {
				tree[i] = minf;
			}

			Update(0, origin);

			var mexs = new int[count];
			for (int i = 1; i < count; i++) {
				int ok = FindLeftest(0, count, 1, 0, n, i);
				mexs[i] = ok;
				Update(ok, i);
			}

			return mexs;

			void Update(int index, int value)
			{
				if (index >= count) {
					return;
				}

				index += n;
				tree[index] = value;
				index >>= 1;
				while (index != 0) {
					tree[index] = Math.Min(tree[index << 1], tree[(index << 1) | 1]);
					index >>= 1;
				}
			}

			int FindLeftest(int left, int right, int v, int l, int r, int i)
			{
				if (tree[v] >= i - ranges[i] || r <= left || right <= l || count <= left) {
					return right;
				} else if (v >= n) {
					return v - n;
				} else {
					int lc = v << 1;
					int rc = (v << 1) | 1;
					int mid = (l + r) >> 1;
					int vl = FindLeftest(left, right, lc, l, mid, i);
					if (vl != right) {
						return vl;
					} else {
						return FindLeftest(left, right, rc, mid, r, i);
					}
				}
			}
		}
	}
}

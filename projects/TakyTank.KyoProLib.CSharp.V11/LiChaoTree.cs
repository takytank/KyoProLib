using System;
using System.Collections.Generic;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V11
{
	public class LiChaoTree
	{
		private readonly int _n;
		private readonly long[] _xs;
		private readonly long[] _ps;
		private readonly long[] _qs;
		private readonly bool[] _used;
		private readonly bool _getsMax;

		public LiChaoTree(long[] points, bool getsMax, long inf = int.MaxValue)
		{
			int count = points.Length;
			_getsMax = getsMax;

			_n = 1;
			while (_n < count) {
				_n <<= 1;
			}

			_xs = new long[_n << 1];
			_ps = new long[_n << 1];
			_qs = new long[_n << 1];
			_used = new bool[_n << 1];

			for (int i = 0; i < count; ++i) {
				_xs[i] = points[i];
			}

			for (int i = count; i < _xs.Length; ++i) {
				_xs[i] = inf;
			}
		}

		public void AddLine(long p, long q) => AddLineCore(p, q, 1, 0, _n);

		public void AddSegment(long p, long q, int l, int r)
		{
			int lv = l + _n;
			int rv = r + _n;
			int size = 1;
			while (lv < rv) {
				if ((rv & 1) != 0) {
					--rv;
					r -= size;
					AddLineCore(p, q, rv, r, r + size);
				}

				if ((lv & 1) != 0) {
					AddLineCore(p, q, lv, l, l + size);
					++lv;
					l += size;
				}

				lv >>= 1;
				rv >>= 1;
				size <<= 1;
			}
		}

		public long Query(int i) => QueryCore(i, _xs[i]);

		private void AddLineCore(long p, long q, int v, int l, int r)
		{
			if (_getsMax) {
				p *= -1;
				q *= -1;
			}

			while (l < r) {
				int m = (l + r) >> 1;
				if (_used[v] == false) {
					_ps[v] = p;
					_qs[v] = q;
					_used[v] = true;
					return;
				}

				var (lx, mx, rx) = (_xs[l], _xs[m], _xs[r - 1]);
				var (vp, vq) = (_ps[v], _qs[v]);
				bool updatesLeft = (p * lx) + q < (vp * lx) + vq;
				bool updatesMid = (p * mx) + q < (vp * mx) + vq;
				bool updatesRight = (p * rx) + q < (vp * rx) + vq;
				if (updatesLeft && updatesRight) {
					_ps[v] = p;
					_qs[v] = q;
					return;
				}

				if (updatesLeft == false && updatesRight == false) {
					return;
				}

				if (updatesMid) {
					(_ps[v], p) = (p, _ps[v]);
					(_qs[v], q) = (q, _qs[v]);
				}

				if (updatesLeft != updatesMid) {
					v <<= 1;
					r = m;
				} else {
					v = (v << 1) | 1;
					l = m;
				}
			}
		}

		private long QueryCore(int v, long x)
		{
			v += _n;
			long ret = _used[v]
				? (_ps[v] * x) + _qs[v]
				: long.MaxValue;
			while (v > 0) {
				v >>= 1;
				if (_used[v]) {
					long temp = (_ps[v] * x) + _qs[v];
					ret = Math.Min(ret, temp);
				}
			}

			if (_getsMax) {
				ret *= -1;
			}

			return ret;
		}
	}
}

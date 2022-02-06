using System;
using System.Collections.Generic;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class Mo
	{
		private readonly int _sqrtN;
		private readonly int _logN;
		private readonly int _maxN;

		private readonly List<int> _tempL;
		private readonly List<int> _tempR;

		private int[] _left;
		private int[] _right;
		private int[] _order;

		public Mo(int n)
		{
			_sqrtN = (int)Math.Sqrt(n);
			//var rnd = new Random();
			//_sqrtN *= rnd.Next(3) + 1;
			_logN = 0;
			while (n > 0) {
				++_logN;
				n >>= 1;
			}
			++_logN;
			_maxN = 1 << _logN;
			_tempL = new List<int>();
			_tempR = new List<int>();
		}

		public void Add(int l, int r)
		{
			_tempL.Add(l);
			_tempR.Add(r);
		}

		public void Build(QuerySortOrder order = QuerySortOrder.Hilbert)
		{
			_left = _tempL.ToArray();
			_right = _tempR.ToArray();
			int size = _left.Length;
			_order = new int[size];
			for (int i = 0; i < size; i++) {
				_order[i] = i;
			}

			switch (order) {
				case QuerySortOrder.Normal:
					Array.Sort(_order, (a, b) => {
						int ac = _left[a] / _sqrtN;
						int bc = _left[b] / _sqrtN;
						if (ac != bc) {
							return _left[a].CompareTo(_left[b]);
						} else if ((ac & 1) != 0) {
							return _right[b].CompareTo(_right[a]);
						} else {
							return _right[a].CompareTo(_right[b]);
						}
					});
					break
						;
				case QuerySortOrder.Hilbert: {
						var dists = new ulong[size];
						for (int i = 0; i < size; i++) {
							dists[i] = HilbertDistance(_left[i], _right[i]);
						}

						Array.Sort(_order, (x, y) => dists[x].CompareTo(dists[y]));
						break;
					}

				case QuerySortOrder.Triangle: {
						var dists = new long[size];
						for (int i = 0; i < size; i++) {
							dists[i] = TriangleDistance(_left[i], _right[i]);
						}

						Array.Sort(_order, (x, y) => dists[x].CompareTo(dists[y]));
						break;
					}

				default:
					break;
			}

		}

		ulong HilbertDistance(int x, int y)
		{
			ulong d = 0;
			for (int s = 1 << (_logN - 1); s > 0; s >>= 1) {
				bool rx = (x & s) > 0;
				bool ry = (y & s) > 0;
				d = (d << 2) | ((rx ? 1u : 0u) * 3) ^ (ry ? 1u : 0u);
				if (ry) {
					continue;
				}

				if (rx) {
					x = _maxN - x;
					y = _maxN - y;
				}

				(x, y) = (y, x);
			}

			return d;
		}

		long TriangleDistance(long l, long r)
		{
			long d = 0;
			for (long s = 1L << (_logN - 1); s > 1; s >>= 1) {
				if (l >= s) {
					d += (36 * s * s) >> 4;
					l -= s;
					r -= s;
				} else if (l + r > s << 1) {
					d += (24 * s * s) >> 4;
					++l;
					r = (s << 1) - r;
					(l, r) = (r, l);
				} else if (r > s) {
					d += (12 * s * s) >> 4;
					l = s - l;
					r = r - s - 1;
					(l, r) = (r, l);
				}
			}

			d += l + r - 1;
			return d;
		}

		public void Run(Action<int> add, Action<int> delete, Action<int> query)
		{
			int l = 0;
			int r = 0;

			foreach (var idx in _order) {
				while (l > _left[idx]) {
					--l;
					add(l);
				}

				while (r < _right[idx]) {
					add(r);
					++r;
				}

				while (l < _left[idx]) {
					delete(l);
					++l;
				}

				while (r > _right[idx]) {
					--r;
					delete(r);
				}

				query(idx);
			}
		}

		public enum QuerySortOrder
		{
			Normal,
			Hilbert,
			Triangle,
		}
	};
}

using System;
using System.Collections.Generic;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V11
{
	public class ConvexHull
	{
		private List<Ipt> _tempPoints = new List<Ipt>();
		private Ipt[] _points;

		public void Add(Ipt point)
		{
			_tempPoints.Add(point);
		}

		public ReadOnlySpan<Ipt> Build()
		{
			var points = _tempPoints.ToArray();
			Array.Sort(points, (x, y) => {
				if (x.X == y.X) {
					return x.Y.CompareTo(y.Y);
				} else {
					return x.X.CompareTo(y.X);
				}
			});

			int n = points.Length;
			int k = 0;
			var ret = new Ipt[n * 2];
			for (int i = 0; i < n; ++i) {
				while (k > 1 && (ret[k - 1] - ret[k - 2]).Det(points[i] - ret[k - 1]) <= 0) {
					--k;
				}

				ret[k] = points[i];
				++k;
			}

			for (int i = n - 2, t = k; i >= 0; --i) {
				while (k > t && (ret[k - 1] - ret[k - 2]).Det(points[i] - ret[k - 1]) <= 0) {
					--k;
				}

				ret[k] = points[i];
				++k;
			}

			_points = ret.AsSpan().Slice(0, k - 1).ToArray();
			return _points;
		}

		public long AreaX2()
		{
			int n = _points.Length;
			long area = 0;
			for (int i = 0; i < n - 1; i++) {
				area += (_points[i].X - _points[i + 1].X) * (_points[i].Y + _points[i + 1].Y);
			}

			area += (_points[n - 1].X - _points[0].X) * (_points[n - 1].Y + _points[0].Y);
			area = Math.Abs(area);
			return area;
		}

		public long GridPointCount()
		{
			long on = GridPointCountOnEdge();
			long within = AreaX2() - GridPointCountOnEdge() + 2;
			within >>= 1;
			return on + within;
		}

		public long GridPointCountOnEdge()
		{
			static long Gcd(long a, long b)
			{
				if (b == 0) {
					return a;
				}

				return Gcd(b, a % b);
			}

			int n = _points.Length;
			long count = 0;
			for (int i = 0; i < n - 1; i++) {
				count += Gcd(
					Math.Abs(_points[i].X - _points[i + 1].X),
					Math.Abs(_points[i].Y - _points[i + 1].Y));
			}

			count += Gcd(
					Math.Abs(_points[n - 1].X - _points[0].X),
					Math.Abs(_points[n - 1].Y - _points[0].Y));

			return count;
		}

		public long GridPointCountWithinEdges()
		{
			long count = AreaX2() - GridPointCountOnEdge() + 2;
			count >>= 1;
			return count;
		}
	}
}

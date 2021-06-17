using System;
using System.Collections.Generic;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class ConvexHull
	{
		private List<Ipt> _tempPoints = new List<Ipt>();

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

			return ret.AsSpan().Slice(0, k - 1);
		}
	}
}

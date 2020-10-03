using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TakyTank.KyoProLib.CS8
{
	public class Compress<T>
		where T : IComparable<T>
	{
		private readonly List<T> values_;
		private readonly Dictionary<T, int> map_;

		public Compress(IReadOnlyList<T> values, int offset = 0)
		{
			values_ = values.Distinct().OrderBy(x => x).ToList();
			map_ = new Dictionary<T, int>();
			int number = offset;
			for (int i = 0; i < values_.Count; ++i, ++number) {
				map_[values_[i]] = number;
			}
		}

		public int Zip(T value)
		{
			return map_[value];
		}

		public T UnZip(int index)
		{
			return values_[index];
		}

		public int LowerBound(T value)
		{
			int ng = -1;
			int ok = values_.Count - 1;
			while (ok - ng > 1) {
				int mid = (ok + ng) / 2;
				if (values_[mid].CompareTo(value) >= 0) {
					ok = mid;
				} else {
					ng = mid;
				}
			}

			return ok;
		}
	}

	public class Compress1D
	{
		private readonly long[] values_;
		private readonly Dictionary<long, int> map_;

		public int Length => values_.Length;

		public Compress1D(ReadOnlySpan<(long start, long end)> raws, int offset = 0)
		{
			int n = raws.Length;
			var temp = new List<long>(n * 4);
			for (int i = 0; i < n; i++) {
				temp.Add(raws[i].start);
				temp.Add(raws[i].start + 1);
				temp.Add(raws[i].end);
				temp.Add(raws[i].end + 1);
			}

			values_ = temp.Distinct().OrderBy(x => x).ToArray();
			map_ = new Dictionary<long, int>(values_.Length);
			int number = offset;
			for (int i = 0; i < values_.Length - 1; ++i, ++number) {
				map_[values_[i]] = number;
			}
		}

		public int Zip(long value)
		{
			return map_[value];
		}

		public long UnZip(int index)
		{
			return values_[index];
		}

		public int LowerBound(long value)
		{
			int ng = -1;
			int ok = values_.Length - 1;
			while (ok - ng > 1) {
				int mid = (ok + ng) / 2;
				if (values_[mid] >= value) {
					ok = mid;
				} else {
					ng = mid;
				}
			}

			return ok;
		}
	}

	public class Compress2D
	{
		private readonly long[] valuesX_;
		private readonly Dictionary<long, int> mapX_;
		private readonly long[] valuesY_;
		private readonly Dictionary<long, int> mapY_;

		public int Height => valuesY_.Length;
		public int Width => valuesX_.Length;

		public Compress2D(
			ReadOnlySpan<(long x1, long y1, long x2, long y2)> raws,
			int offset = 0)
		{
			int n = raws.Length;
			var tempX = new List<long>(n * 4);
			var tempY = new List<long>(n * 4);
			for (int i = 0; i < n; i++) {
				for (int d = 0; d < 2; d++) {
					tempX.Add(raws[i].x1 + d);
					tempX.Add(raws[i].x2 + d);
					tempY.Add(raws[i].y1 + d);
					tempY.Add(raws[i].y2 + d);
				}
			}

			valuesX_ = tempX.Distinct().OrderBy(x => x).ToArray();
			valuesY_ = tempX.Distinct().OrderBy(x => x).ToArray();
			int number = offset;
			for (int i = 0; i < valuesX_.Length - 1; ++i, ++number) {
				mapX_[valuesX_[i]] = number;
			}

			number = offset;
			for (int i = 0; i < valuesY_.Length - 1; ++i, ++number) {
				mapY_[valuesY_[i]] = number;
			}
		}

		public (int xi, int yi) Zip(long x, long y)
		{
			return (mapX_[x], mapY_[y]);
		}

		public (long x, long y) UnZip(int xi, int yi)
		{
			return (valuesX_[xi], valuesY_[yi]);
		}

		public (int xi, int yi) LowerBound(long x, long y)
		{
			int xi;
			{
				int ng = -1;
				int ok = valuesX_.Length - 1;
				while (ok - ng > 1) {
					int mid = (ok + ng) / 2;
					if (valuesX_[mid] >= x) {
						ok = mid;
					} else {
						ng = mid;
					}
				}

				xi = ok;
			}

			int yi;
			{
				int ng = -1;
				int ok = valuesY_.Length - 1;
				while (ok - ng > 1) {
					int mid = (ok + ng) / 2;
					if (valuesY_[mid] >= y) {
						ok = mid;
					} else {
						ng = mid;
					}
				}

				yi = ok;
			}

			return (xi, yi);
		}
	}
}

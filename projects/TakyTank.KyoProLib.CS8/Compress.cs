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

		public Compress1D(ReadOnlySpan<(long start, long end)> raws, int offset = 0)
		{
			int n = raws.Length;
			var temp = new List<long>(n * 2);
			for (int i = 0; i < n; i++) {
				temp.Add(raws[i].start);
				temp.Add(raws[i].start + 1);
				temp.Add(raws[i].end);
				temp.Add(raws[i].end + 1);
			}

			values_ = temp.Distinct().OrderBy(x => x).ToArray();
			int number = offset;
			for (int i = 0; i < values_.Length; ++i, ++number) {
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
}

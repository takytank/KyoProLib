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

		public Compress(IReadOnlyList<T> values)
		{
			values_ = values.Distinct().OrderBy(x => x).ToList();
			map_ = new Dictionary<T, int>();
			for (int i = 0; i < values_.Count; i++) {
				map_[values_[i]] = i;
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
}

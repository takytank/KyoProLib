using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class Compression<T>
		where T : IComparable<T>
	{
		public static int Compress(
			IList<T> raws,
			Func<int, T> cast)
		{
			int n = raws.Count;
			var values = raws.Distinct().OrderBy(x => x).ToArray();

			static int LowerBound(T[] values, T target)
			{
				int ng = -1;
				int ok = values.Length - 1;
				while (ok - ng > 1) {
					int mid = (ok + ng) / 2;
					if (values[mid].CompareTo(target) >= 0) {
						ok = mid;
					} else {
						ng = mid;
					}
				}

				return ok;
			}

			for (int i = 0; i < n; i++) {
				T newValue = cast.Invoke(LowerBound(values, raws[i]));
				raws[i] = newValue;
			}

			return values.Length;
		}

		private readonly T[] values_;
		private readonly Dictionary<T, int> map_;

		public Compression(IReadOnlyList<T> values, int offset = 0)
		{
			values_ = values.Distinct().OrderBy(x => x).ToArray();
			map_ = new Dictionary<T, int>(values_.Length);
			int number = offset;
			for (int i = 0; i < values_.Length; ++i, ++number) {
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
			int ok = values_.Length - 1;
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

	public class Compression1D<T>
		where T : IComparable<T>
	{
		public static int Compress(
			Span<(T start, T end)> raws,
			Func<T, T> next,
			Func<int, T> cast)
		{
			int n = raws.Length;
			var temp = new List<T>(n * 2);
			for (int i = 0; i < n; i++) {
				temp.Add(raws[i].start);
				temp.Add(next.Invoke(raws[i].start));
				temp.Add(raws[i].end);
				temp.Add(next.Invoke(raws[i].end));
			}

			var values = temp.Distinct().OrderBy(x => x).SkipLast(1).ToArray();

			static int LowerBound(T[] values, T target)
			{
				int ng = -1;
				int ok = values.Length - 1;
				while (ok - ng > 1) {
					int mid = (ok + ng) / 2;
					if (values[mid].CompareTo(target) >= 0) {
						ok = mid;
					} else {
						ng = mid;
					}
				}

				return ok;
			}

			for (int i = 0; i < n; i++) {
				T ns = cast.Invoke(LowerBound(values, raws[i].start));
				T ne = cast.Invoke(LowerBound(values, raws[i].end));
				raws[i] = (ns, ne);
			}

			return values.Length;
		}

		private readonly T[] values_;
		private readonly Dictionary<T, int> map_;

		public int Length => values_.Length;

		public Compression1D(
			ReadOnlySpan<(T start, T end)> raws,
			Func<T, T> next,
			int offset = 0)
		{
			int n = raws.Length;
			var temp = new List<T>(n * 2);
			for (int i = 0; i < n; i++) {
				temp.Add(raws[i].start);
				temp.Add(next.Invoke(raws[i].start));
				temp.Add(raws[i].end);
				temp.Add(next.Invoke(raws[i].end));
			}

			values_ = temp.Distinct().OrderBy(x => x).SkipLast(1).ToArray();
			map_ = new Dictionary<T, int>(values_.Length);
			int number = offset;
			for (int i = 0; i < values_.Length; ++i, ++number) {
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
			int ok = values_.Length - 1;
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

	public class Compression2D<T>
		where T : IComparable<T>
	{
		public static (int height, int width) Compress(
			Span<(T x1, T y1, T x2, T y2)> raws,
			Func<T, T> next,
			Func<int, T> cast)
		{
			int n = raws.Length;
			var tempX = new List<T>(n * 4);
			var tempY = new List<T>(n * 4);
			for (int i = 0; i < n; i++) {
				tempX.Add(raws[i].x1);
				tempX.Add(raws[i].x2);
				tempY.Add(raws[i].y1);
				tempY.Add(raws[i].y2);
				tempX.Add(next(raws[i].x1));
				tempX.Add(next(raws[i].x2));
				tempY.Add(next(raws[i].y1));
				tempY.Add(next(raws[i].y2));
			}

			var valuesX = tempX.Distinct().OrderBy(x => x).SkipLast(1).ToArray();
			var valuesY = tempY.Distinct().OrderBy(x => x).SkipLast(1).ToArray();

			static int LowerBound(T[] values, T target)
			{
				int ng = -1;
				int ok = values.Length - 1;
				while (ok - ng > 1) {
					int mid = (ok + ng) / 2;
					if (values[mid].CompareTo(target) >= 0) {
						ok = mid;
					} else {
						ng = mid;
					}
				}

				return ok;
			}

			for (int i = 0; i < n; i++) {
				T nx1 = cast.Invoke(LowerBound(valuesX, raws[i].x1));
				T nx2 = cast.Invoke(LowerBound(valuesX, raws[i].x2));
				T ny1 = cast.Invoke(LowerBound(valuesY, raws[i].y1));
				T ny2 = cast.Invoke(LowerBound(valuesY, raws[i].y2));
				raws[i] = (nx1, ny1, nx2, ny2);
			}

			return (valuesY.Length, valuesX.Length);
		}

		private readonly T[] valuesX_;
		private readonly Dictionary<T, int> mapX_;
		private readonly T[] valuesY_;
		private readonly Dictionary<T, int> mapY_;

		public int Height => valuesY_.Length;
		public int Width => valuesX_.Length;

		public Compression2D(
			ReadOnlySpan<(T x1, T y1, T x2, T y2)> raws,
			Func<T, T> next,
			int offset = 0)
		{
			int n = raws.Length;
			var tempX = new List<T>(n * 4);
			var tempY = new List<T>(n * 4);
			for (int i = 0; i < n; i++) {
				tempX.Add(raws[i].x1);
				tempX.Add(raws[i].x2);
				tempY.Add(raws[i].y1);
				tempY.Add(raws[i].y2);
				tempX.Add(next(raws[i].x1));
				tempX.Add(next(raws[i].x2));
				tempY.Add(next(raws[i].y1));
				tempY.Add(next(raws[i].y2));
			}

			valuesX_ = tempX.Distinct().OrderBy(x => x).SkipLast(1).ToArray();
			mapX_ = new Dictionary<T, int>(valuesX_.Length);
			valuesY_ = tempY.Distinct().OrderBy(x => x).SkipLast(1).ToArray();
			mapY_ = new Dictionary<T, int>(valuesY_.Length);
			int number = offset;
			for (int i = 0; i < valuesX_.Length; ++i, ++number) {
				mapX_[valuesX_[i]] = number;
			}

			number = offset;
			for (int i = 0; i < valuesY_.Length; ++i, ++number) {
				mapY_[valuesY_[i]] = number;
			}
		}

		public (int xi, int yi) Zip(T x, T y)
		{
			return (mapX_[x], mapY_[y]);
		}

		public (T x, T y) UnZip(int xi, int yi)
		{
			return (valuesX_[xi], valuesY_[yi]);
		}

		public (int xi, int yi) LowerBound(T x, T y)
		{
			int xi;
			{
				int ng = -1;
				int ok = valuesX_.Length - 1;
				while (ok - ng > 1) {
					int mid = (ok + ng) / 2;
					if (valuesX_[mid].CompareTo(x) >= 0) {
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
					if (valuesY_[mid].CompareTo(y) >= 0) {
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

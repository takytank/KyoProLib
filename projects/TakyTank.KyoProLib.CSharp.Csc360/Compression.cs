using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class Compression<T>
		where T : IComparable<T>
	{
		public static int Compress(
			Span<T> raws,
			Func<int, T> cast)
		{
			int n = raws.Length;
			var comp = new Compression<T>(raws);
			comp.Compress();
			for (int i = 0; i < n; i++) {
				raws[i] = cast.Invoke(comp.Zip(raws[i]));
			}

			return comp.Count;
		}

		private readonly HashSet<T> raws_ = new HashSet<T>();
		private T[] values_;
		private Dictionary<T, int> map_;
		public int Count => values_.Length;

		public Compression() { }
		public Compression(ReadOnlySpan<T> values)
		{
			Add(values);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(T value) => raws_.Add(value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(ReadOnlySpan<T> values)
		{
			foreach (var value in values) {
				raws_.Add(value);
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Compress(int offset = 0)
		{
			values_ = raws_.ToArray();
			Array.Sort(values_);
			map_ = new Dictionary<T, int>(values_.Length);
			int number = offset;
			for (int i = 0; i < values_.Length; ++i, ++number) {
				map_[values_[i]] = number;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Zip(T value) => map_[value];
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T UnZip(int index) => values_[index];

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
		private readonly HashSet<T> raws_ = new HashSet<T>();
		private T[] values_;
		private Dictionary<T, int> map_;
		public int Length { get; private set; }

		public Compression1D() { }
		public Compression1D(
			ReadOnlySpan<(T start, T end)> values,
			Func<T, T> next)
		{
			foreach (var value in values) {
				Add(value, next);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add((T start, T end) range, Func<T, T> next)
		{
			raws_.Add(range.start);
			raws_.Add(next.Invoke(range.start));
			raws_.Add(range.end);
			raws_.Add(next.Invoke(range.end));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Compress(int offset = 0)
		{
			values_ = raws_.ToArray();
			Array.Sort(values_);
			Length = values_.Length - 1;

			map_ = new Dictionary<T, int>(Length);
			int number = offset;
			for (int i = 0; i < values_.Length - 1; ++i, ++number) {
				map_[values_[i]] = number;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Zip(T value) => map_[value];
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T UnZip(int index) => values_[index];

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
		private readonly HashSet<T> rawsX_ = new HashSet<T>();
		private readonly HashSet<T> rawsY_ = new HashSet<T>();
		private T[] valuesX_;
		private Dictionary<T, int> mapX_;
		private T[] valuesY_;
		private Dictionary<T, int> mapY_;

		public int Height { get; private set; }
		public int Width { get; private set; }

		public Compression2D() { }
		public Compression2D(
			ReadOnlySpan<(T x1, T y1, T x2, T y2)> values,
			Func<T, T> next)
		{
			foreach (var value in values) {
				Add(value, next);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add((T x1, T y1, T x2, T y2) area, Func<T, T> next)
		{
			rawsX_.Add(area.x1);
			rawsX_.Add(area.x2);
			rawsY_.Add(area.y1);
			rawsY_.Add(area.y2);
			rawsX_.Add(next(area.x1));
			rawsX_.Add(next(area.x2));
			rawsY_.Add(next(area.y1));
			rawsY_.Add(next(area.y2));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Compress(int offset = 0)
		{
			valuesX_ = rawsX_.ToArray();
			Array.Sort(valuesX_);
			Width = valuesX_.Length - 1;

			valuesY_ = rawsY_.ToArray();
			Array.Sort(valuesY_);
			Height = valuesY_.Length - 1;

			mapX_ = new Dictionary<T, int>(Width);
			mapY_ = new Dictionary<T, int>(Height);

			int number = offset;
			for (int i = 0; i < valuesX_.Length - 1; ++i, ++number) {
				mapX_[valuesX_[i]] = number;
			}

			number = offset;
			for (int i = 0; i < valuesY_.Length - 1; ++i, ++number) {
				mapY_[valuesY_[i]] = number;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int xi, int yi) Zip(T x, T y) => (mapX_[x], mapY_[y]);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (T x, T y) UnZip(int xi, int yi) => (valuesX_[xi], valuesY_[yi]);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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

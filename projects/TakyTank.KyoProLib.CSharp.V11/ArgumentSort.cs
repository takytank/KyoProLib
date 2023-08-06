using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V11
{
	public class ArgumentSort
	{
		private readonly (double x, double y) origin_;
		private readonly List<(double x, double y)> tempPoints_;
		private (double x, double y)[] points_;
		private (int index, double angle)[] angles_;

		public int Count => points_.Length;
		public (double x, double y)[] Points => points_;
		public (int index, double angle)[] Sorted => angles_;

		public ArgumentSort(double originX, double originY)
		{
			origin_ = (originX, originY);
			tempPoints_ = new List<(double x, double y)>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double Angle(double x, double y)
			=> Math.Atan2(y - origin_.y, x - origin_.x);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double Distance(double x, double y)
		{
			double dx = x - origin_.x;
			double dy = y - origin_.y;
			return Math.Sqrt(dx * dx + dy * dy);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddPoint(double x, double y) => tempPoints_.Add((x, y));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Build()
		{
			points_ = tempPoints_.ToArray();
			int n = points_.Length;
			angles_ = new (int i, double angle)[n];
			for (int i = 0; i < n; i++) {
				angles_[i] = (i, Angle(points_[i].x, points_[i].y));
			}

			Array.Sort(angles_, (x, y) => x.angle.CompareTo(y.angle));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int i, double angle) Nearest(double target)
		{
			int ok = 0;
			int ng = angles_.Length;
			while (ng - ok > 1) {
				int mid = (ok + ng) / 2;
				if (angles_[mid].angle < target) {
					ok = mid;
				} else {
					ng = mid;
				}
			}

			int ok2 = (ok + 1) % angles_.Length;
			if (Math.Abs(angles_[ok].angle - target) > Math.Abs(angles_[ok2].angle - target)) {
				ok = ok2;
			}

			return angles_[ok];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int i, double angle) ReverseNearest(double current)
		{
			double target = current + Math.PI;
			if (target > 2 * Math.PI) {
				target -= 2 * Math.PI;
			}

			return Nearest(target);
		}
	}
}

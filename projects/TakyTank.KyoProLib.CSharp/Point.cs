using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public struct Ipt
	{
		public long X { get; set; }
		public long Y { get; set; }

		public Ipt(long x, long y)
		{
			X = x;
			Y = y;
		}

		public static long Length2(Ipt p, Ipt q)
		{
			long x = p.X - q.X;
			long y = p.Y - q.Y;
			return x * x + y * y;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsOnSegment(Ipt p1, Ipt p2, Ipt q)
			=> (p1 - q).Det(p2 - q) == 0 && (p1 - q).Dot(p2 - q) <= 0;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsOnLine(Ipt a, Ipt b, Ipt c)
			=> a.Y * (b.X - c.X) + b.Y * (c.X - a.X) + c.Y * (a.X - b.X) == 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsCross(Ipt p1, Ipt p2, Ipt q1, Ipt q2)
		{
			var pp = p2 - p1;
			if (pp.Det(q1 - p1) * pp.Det(q2 - p1) > 0) {
				return false;
			}

			var qq = q2 - q1;
			if (qq.Det(p1 - q1) * qq.Det(p2 - q1) > 0) {
				return false;
			}

			return true;
		}

		public static Ipt operator +(Ipt lhs, Ipt rhs)
			=> new Ipt(lhs.X + rhs.X, lhs.Y + rhs.Y);
		public static Ipt operator -(Ipt lhs, Ipt rhs)
			=> new Ipt(lhs.X - rhs.X, lhs.Y - rhs.Y);
		public static Ipt operator *(Ipt src, long value)
			=> new Ipt(src.X * value, src.Y * value);
		public static Ipt operator *(long value, Ipt src)
			=> new Ipt(src.X * value, src.Y * value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long Dot(Ipt target) => X * target.X + Y * target.Y;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long Det(Ipt target) => X * target.Y - Y * target.X;

		public override int GetHashCode() => HashCode.Combine(X, Y);
	}

	public struct Dpt
	{
		private const double EPS = 1e-10;
		public static Dpt Origin => new Dpt(0, 0);

		public double X { get; set; }
		public double Y { get; set; }
		public double D => Math.Sqrt(X * X + Y * Y);

		public Dpt(double x, double y)
		{
			X = x;
			Y = y;
		}

		public static double Length(Dpt p, Dpt q)
		{
			double x = p.X - q.X;
			double y = p.Y - q.Y;
			return Math.Sqrt(x * x + y * y);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsOnSegment(Dpt p1, Dpt p2, Dpt q)
			=> (p1 - q).Det(p2 - q) == 0 && (p1 - q).Dot(p2 - q) <= 0;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsOnLine(Dpt a, Dpt b, Dpt c)
			=> a.Y * (b.X - c.X) + b.Y * (c.X - a.X) + c.Y * (a.X - b.X) == 0;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Dpt Intersection(Dpt p1, Dpt p2, Dpt q1, Dpt q2)
			=> p1 + (p2 - p1) * ((q2 - q1).Det(q1 - p1) / (q2 - q1).Det(p2 - p1));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsCross(Dpt p1, Dpt p2, Dpt q1, Dpt q2)
		{
			var pp = p2 - p1;
			if (pp.Det(q1 - p1) * pp.Det(q2 - p1) > 0) {
				return false;
			}

			var qq = q2 - q1;
			if (qq.Det(p1 - q1) * qq.Det(p2 - q1) > 0) {
				return false;
			}

			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsCross2(Dpt p1, Dpt p2, Dpt q1, Dpt q2)
		{
			if ((p1 - q1).Det(p2 - q2) == 0) {
				return IsOnSegment(p1, q1, p2)
					|| IsOnSegment(p1, q1, q2)
					|| IsOnSegment(p2, q2, p1)
					|| IsOnSegment(p2, q2, q1);
			} else {
				var intersection = Intersection(p1, p2, q1, q2);
				return IsOnSegment(p1, p2, intersection) && IsOnSegment(q1, q2, intersection);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Dpt Circumcenter(Dpt a, Dpt b, Dpt c)
		{
			double px = b.X - a.X;
			double py = b.Y - a.Y;
			double qx = c.X - a.X;
			double qy = c.Y - a.Y;

			double x = a.X + (qy * (px * px + py * py) - py * (qx * qx + qy * qy)) / (px * qy - py * qx) / 2;
			double y = (py != 0)
				? (px * (a.X + b.X - x - x) + py * (a.Y + b.Y)) / py / 2
				: (qx * (a.X + c.X - x - x) + qy * (a.Y + c.Y)) / qy / 2;

			return new Dpt(x, y);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double RadiusOfCircumcircle(double a, double b, double c)
			=> a * b * c / Math.Sqrt((a + b + c) * (-a + b + c) * (a - b + c) * (a + b - c));

		public static Dpt operator +(Dpt lhs, Dpt rhs)
			=> new Dpt(Add(lhs.X, rhs.X), Add(lhs.Y, rhs.Y));
		public static Dpt operator -(Dpt lhs, Dpt rhs)
			=> new Dpt(Add(lhs.X, -rhs.X), Add(lhs.Y, -rhs.Y));
		public static Dpt operator *(Dpt src, double value)
			=> new Dpt(src.X * value, src.Y * value);
		public static Dpt operator *(double value, Dpt src)
			=> new Dpt(src.X * value, src.Y * value);
		public static Dpt operator /(Dpt src, double value)
			=> new Dpt(src.X / value, src.Y / value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double Dot(Dpt target) => Add(X * target.X, Y * target.Y);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double Det(Dpt target) => Add(X * target.Y, -Y * target.X);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Dpt Rotate(double angle_rad)
		{
			double x = X * Math.Cos(angle_rad) - Y * Math.Sin(angle_rad);
			double y = X * Math.Sin(angle_rad) + Y * Math.Cos(angle_rad);
			return new Dpt(x, y);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static double Add(double a, double b)
			=> Math.Abs(a + b) < EPS * (Math.Abs(a) + Math.Abs(b)) ? 0 : a + b;

		public override int GetHashCode() => HashCode.Combine(X, Y);
		public override string ToString() => $"{X} {Y}";
	}
}

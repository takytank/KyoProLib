using System;
using System.Runtime.CompilerServices;

namespace TakyTank.KyoProLib.CSharp.V8
{
	public readonly struct Ipt : IEquatable<Ipt>, IComparable<Ipt>
	{
		public long X { get; }
		public long Y { get; }

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

		public static (long numerator, long denominator, bool squared)
			DistanceOfPointAndLine(Ipt p1, Ipt p2, Ipt q)
		{
			if ((p2 - p1).Dot(q - p1) < 0
				|| (p1 - p2).Dot(q - p2) < 0) {
				long distance2 = Math.Min(
					Ipt.Length2(p1, q),
					Ipt.Length2(p2, q));
				return (distance2, 1, true);
			} else {
				long numerator = (q - p1).Det(p2 - p1);
				long denominator2 = Ipt.Length2(p1, p2);
				return (numerator, denominator2, false);
			}
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
			if (IsParallel(p1, p2, q1, q2)) {
				return IsOnSegment(p1, p2, q1)
					|| IsOnSegment(p1, p2, q2)
					|| IsOnSegment(q1, q2, p1)
					|| IsOnSegment(q1, q2, p2);
			} else {
				var pp = p2 - p1;
				var det1 = pp.Det(q1 - p1);
				var det2 = pp.Det(q2 - p1);
				if (((det1 >> 63) - (-det1 >> 63)) * ((det2 >> 63) - (-det2 >> 63)) > 0) {
					return false;
				}

				var qq = q2 - q1;
				det1 = qq.Det(p1 - q1);
				det2 = qq.Det(p2 - q1);
				if (((det1 >> 63) - (-det1 >> 63)) * ((det2 >> 63) - (-det2 >> 63)) > 0) {
					return false;
				}

				return true;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsParallel(Ipt p1, Ipt p2, Ipt q1, Ipt q2)
			=> (p2 - p1).Det(q2 - q1) == 0;

		public static Ipt operator +(Ipt lhs, Ipt rhs)
			=> new Ipt(lhs.X + rhs.X, lhs.Y + rhs.Y);
		public static Ipt operator -(Ipt lhs, Ipt rhs)
			=> new Ipt(lhs.X - rhs.X, lhs.Y - rhs.Y);
		public static Ipt operator *(Ipt src, long value)
			=> new Ipt(src.X * value, src.Y * value);
		public static Ipt operator *(long value, Ipt src)
			=> new Ipt(src.X * value, src.Y * value);
		public static bool operator ==(Ipt x1, Ipt x2) => x1.Equals(x2);
		public static bool operator !=(Ipt x1, Ipt x2) => !x1.Equals(x2);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long Dot(Ipt target) => X * target.X + Y * target.Y;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long Det(Ipt target) => X * target.Y - Y * target.X;

		public bool Equals(Ipt other) => X == other.X && Y == other.Y;
		public override bool Equals(object obj) => obj is Ipt x && Equals(x);

		public override string ToString() => $"({X}, {Y})";
		public override int GetHashCode() => HashCode.Combine(X, Y);

		// 0~2πの偏角順。Atan2の順と異なるので注意。
		// また、(0, 0)が入ると壊れるので注意。
		// 偏角が同じ場合は0を返すので、偏角が同じ異なる点の順序は不定。
		// 座標がクロス積でオーバーフローするサイズの場合、Int128なりに書き換えて使うこと。
		public int CompareTo(Ipt other)
		{
			// 0度から順番に並べる。
			// 0度を跨いだ時の判定を楽にするため、[0, 180) と [180, 360) の領域に分ける。
			// これは、(y, x) < (0, 0) が false のときに前者で、そうでないとき後者となる。
			// またこの時、(0, 0)と比較すると前者が正で後者が負となる。
			var cmpP = CompareTo00(X, Y);
			var cmpQ = CompareTo00(other.X, other.Y);
			if (cmpP != cmpQ) {
				// 負の方を後にしたいので、逆に比較。
				return cmpQ.CompareTo(cmpP);
			}

			// 同一象限内にある場合、Pに対するQのクロス積が正の場合に、
			// Pを反時計回り方向に動かした方向にQがあるため、ソート順としてはP->Qの順になる。
			// つまり、P.X * Q.Y よりも P.Y * Q.X の方が小さければよい。
			return (Y * other.X).CompareTo(X * other.Y);

			static int CompareTo00(long x, long y)
			{
				return y == 0
					? x >= 0 ? 1 : -1
					: y > 0 ? 1 : -1;
			}
		}
	}

	public readonly struct Dpt : IEquatable<Dpt>
	{
		private const double EPS = 1e-10;
		public static Dpt Origin => new Dpt(0, 0);

		public double X { get; }
		public double Y { get; }
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

		public static double DistanceOfPointAndLine(Dpt p1, Dpt p2, Dpt q)
		{
			if ((p2 - p1).Dot(q - p1) < 0
				|| (p1 - p2).Dot(q - p2) < 0) {
				double distance = Math.Min(
					Dpt.Length(p1, q),
					Dpt.Length(p2, q));
				return distance;
			} else {
				double numerator = (q - p1).Det(p2 - p1);
				double denominator = Dpt.Length(p1, p2);
				return numerator / denominator;
			}
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
		public static bool operator ==(Dpt x1, Dpt x2) => x1.Equals(x2);
		public static bool operator !=(Dpt x1, Dpt x2) => !x1.Equals(x2);

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

		public bool Equals(Dpt other) => X == other.X && Y == other.Y;
		public override bool Equals(object obj) => obj is Dpt x && Equals(x);

		public override int GetHashCode() => HashCode.Combine(X, Y);
		public override string ToString() => $"{X} {Y}";
	}
}

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V11
{
	public struct CircleI
	{
		public Ipt Center { get; set; }
		public long Radius { get; set; }

		public CircleI(long x, long y, long radius)
		{
			Center = new Ipt(x, y);
			Radius = radius;
		}

		public CircleI(Ipt center, long radius)
		{
			Center = center;
			Radius = radius;
		}

		public static bool Intersects(CircleI p, CircleI q)
		{
			long distance2 = Ipt.Length2(p.Center, q.Center);
			long radiusSum2 = (p.Radius + q.Radius) * (p.Radius + q.Radius);
			if (distance2 > radiusSum2) {
				return false;
			} else if (distance2 == radiusSum2) {
				return true;
			}

			long radiusSub2 = (p.Radius - q.Radius) * (p.Radius - q.Radius);
			if (distance2 < radiusSub2) {
				return false;
			}

			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool OnCircumference(long x, long y) => OnCircumference(new Ipt(x, y));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool OnCircumference(Ipt point)
		{
			long distance2 = Ipt.Length2(Center, point);
			return distance2 == Radius * Radius;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Includes(long x, long y, bool excludesBorder = false)
			=> Includes(new Ipt(x, y), excludesBorder);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Includes(Ipt point, bool excludesBorder = false)
		{
			long distance2 = Ipt.Length2(Center, point);
			return excludesBorder
				? distance2 < Radius * Radius
				: distance2 <= Radius * Radius;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Includes(CircleI other, bool excludesBorder = false)
		{
			if (other.Radius > Radius) {
				return false;
			}

			long distance2 = Ipt.Length2(Center, other.Center);
			long radiusSub2 = (Radius - other.Radius) * (Radius - other.Radius);

			return excludesBorder
				? distance2 < radiusSub2
				: distance2 <= radiusSub2;
		}
	}
}

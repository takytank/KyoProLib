using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.Mono280
{
	public static class BitOp
	{
		public static uint PopCount(uint bits)
		{
			bits = (bits & 0x55555555) + (bits >> 1 & 0x55555555);
			bits = (bits & 0x33333333) + (bits >> 2 & 0x33333333);
			bits = (bits & 0x0f0f0f0f) + (bits >> 4 & 0x0f0f0f0f);
			bits = (bits & 0x00ff00ff) + (bits >> 8 & 0x00ff00ff);
			return (bits & 0x0000ffff) + (bits >> 16 & 0x0000ffff);
		}

		public static int PopCount(ulong bits)
		{
			bits = ((bits & 0xaaaaaaaaaaaaaaaa) >> 1) + (bits & 0x5555555555555555);
			bits = ((bits & 0xcccccccccccccccc) >> 2) + (bits & 0x3333333333333333);
			bits = ((bits & 0xf0f0f0f0f0f0f0f0) >> 4) + (bits & 0x0f0f0f0f0f0f0f0f);
			bits = ((bits & 0xff00ff00ff00ff00) >> 8) + (bits & 0x00ff00ff00ff00ff);
			bits = ((bits & 0xffff0000ffff0000) >> 16) + (bits & 0x0000ffff0000ffff);
			bits = ((bits & 0xffffffff00000000) >> 32) + (bits & 0x00000000ffffffff);
			return (int)bits;
		}
	}
}

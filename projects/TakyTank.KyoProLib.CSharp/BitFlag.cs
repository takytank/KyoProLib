using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public struct BitFlag
	{
		public static BitFlag Begin() => 0;
		public static BitFlag End(int bitCount) => 1 << bitCount;
		public static BitFlag FromBit(int bitNumber) => 1 << bitNumber;

		private readonly int flags_;
		public int Flag => flags_;
		public bool this[int bitNumber] => (flags_ & (1 << bitNumber)) != 0;
		public BitFlag(int flags) { flags_ = flags; }

		public bool Has(BitFlag target) => (flags_ & target.flags_) == target.flags_;
		public bool Has(int target) => (flags_ & target) == target;
		public bool HasBit(int bitNumber) => (flags_ & (1 << bitNumber)) != 0;
		public BitFlag OrBit(int bitNumber) => (flags_ | (1 << bitNumber));
		public BitFlag AndBit(int bitNumber) => (flags_ & (1 << bitNumber));
		public BitFlag XorBit(int bitNumber) => (flags_ ^ (1 << bitNumber));

		public static BitFlag operator ++(BitFlag src) => new BitFlag(src.flags_ + 1);
		public static BitFlag operator --(BitFlag src) => new BitFlag(src.flags_ - 1);
		public static BitFlag operator |(BitFlag lhs, BitFlag rhs)
			=> new BitFlag(lhs.flags_ | rhs.flags_);
		public static BitFlag operator |(BitFlag lhs, int rhs)
			=> new BitFlag(lhs.flags_ | rhs);
		public static BitFlag operator |(int lhs, BitFlag rhs)
			=> new BitFlag(lhs | rhs.flags_);
		public static BitFlag operator &(BitFlag lhs, BitFlag rhs)
			=> new BitFlag(lhs.flags_ & rhs.flags_);
		public static BitFlag operator &(BitFlag lhs, int rhs)
			=> new BitFlag(lhs.flags_ & rhs);
		public static BitFlag operator &(int lhs, BitFlag rhs)
			=> new BitFlag(lhs & rhs.flags_);

		public static bool operator <(BitFlag lhs, BitFlag rhs) => lhs.flags_ < rhs.flags_;
		public static bool operator <(BitFlag lhs, int rhs) => lhs.flags_ < rhs;
		public static bool operator <(int lhs, BitFlag rhs) => lhs < rhs.flags_;
		public static bool operator >(BitFlag lhs, BitFlag rhs) => lhs.flags_ > rhs.flags_;
		public static bool operator >(BitFlag lhs, int rhs) => lhs.flags_ > rhs;
		public static bool operator >(int lhs, BitFlag rhs) => lhs > rhs.flags_;
		public static bool operator <=(BitFlag lhs, BitFlag rhs) => lhs.flags_ <= rhs.flags_;
		public static bool operator <=(BitFlag lhs, int rhs) => lhs.flags_ <= rhs;
		public static bool operator <=(int lhs, BitFlag rhs) => lhs <= rhs.flags_;
		public static bool operator >=(BitFlag lhs, BitFlag rhs) => lhs.flags_ >= rhs.flags_;
		public static bool operator >=(BitFlag lhs, int rhs) => lhs.flags_ >= rhs;
		public static bool operator >=(int lhs, BitFlag rhs) => lhs >= rhs.flags_;

		public static implicit operator BitFlag(int t) => new BitFlag(t);
		public static implicit operator int(BitFlag t) => t.flags_;

		public override string ToString() => $"{Convert.ToString(flags_, 2).PadLeft(32, '0')} ({flags_})";

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ForEachSubBits(Action<BitFlag> action)
		{
			for (BitFlag sub = flags_; sub >= 0; --sub) {
				sub &= flags_;
				action(sub);
			}
		}
	}
}

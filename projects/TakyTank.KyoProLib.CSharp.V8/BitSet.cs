using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V8
{
	public class BitSet : IComparable<BitSet>
	{
		private const int SHIFT = 6;
		private const int BITS = 64;

		private readonly int length_;
		private readonly ulong[] bits_;
		private readonly int extraBits_;

		public int BitCount { get; }
		public BitSet(int bitCount, bool fillTrue = false)
		{
			BitCount = bitCount;
			extraBits_ = DivideByBits(BitCount).remainder;
			length_ = (bitCount - 1 >> SHIFT) + 1;
			bits_ = new ulong[length_];
			if (fillTrue) {
				int y = bitCount % BITS;
				if (y == 0) {
					for (int i = 0; i < length_; i++) {
						bits_[i] = ulong.MaxValue;
					}
				} else {
					for (int i = 0; i < length_ - 1; i++) {
						bits_[i] = ulong.MaxValue;
					}

					bits_[length_ - 1] = (1UL << y) - 1;
				}
			}
		}

		public BitSet(int bitCount, long initialValue)
		{
			BitCount = bitCount;
			extraBits_ = DivideByBits(BitCount).remainder;
			length_ = (bitCount - 1 >> SHIFT) + 1;
			bits_ = new ulong[length_];
			bits_[0] = (ulong)initialValue;
		}

		public bool this[int i]
		{
			get => (bits_[i >> SHIFT] >> i & 1) != 0;
			set => bits_[i >> 6]
				= bits_[i >> SHIFT] & (ulong.MaxValue ^ 1ul << i)
				| (ulong)(value ? 1 : 0) << i;
		}

		public static BitSet[] GetBasisOf(
			ReadOnlySpan<BitSet> values, bool isRawValues = false)
		{
			int n = values.Length;
			var eliminateds = new List<BitSet>();
			var raws = new List<BitSet>();
			var zero = new BitSet(1);
			for (int j = n - 1; j >= 0; j--) {
				BitSet v = values[j];
				foreach (var b in eliminateds) {
					var next = v ^ b;
					if (v > next) {
						v = next;
					}
				}

				if (v > zero) {
					eliminateds.Add(v);
					raws.Add(values[j]);
				}
			}

			return isRawValues
				? raws.ToArray()
				: eliminateds.ToArray();
		}

		public static BitSet operator &(BitSet lhs, BitSet rhs)
		{
			int min = Math.Min(lhs.length_, rhs.length_);
			var ret = new BitSet(Math.Max(lhs.BitCount, rhs.BitCount));
			for (int i = 0; i < min; i++) {
				ret.bits_[i] = lhs.bits_[i] & rhs.bits_[i];
			}

			return ret;
		}

		public static BitSet operator |(BitSet lhs, BitSet rhs)
		{
			int max = Math.Max(lhs.length_, rhs.length_);
			int min = Math.Min(lhs.length_, rhs.length_);
			var ret = new BitSet(Math.Max(lhs.BitCount, rhs.BitCount));
			for (int i = 0; i < min; i++) {
				ret.bits_[i] = lhs.bits_[i] | rhs.bits_[i];
			}

			if (lhs.length_ > rhs.length_) {
				for (int i = min; i < max; i++) {
					ret.bits_[i] = lhs.bits_[i];
				}
			} else {
				for (int i = min; i < max; i++) {
					ret.bits_[i] = rhs.bits_[i];
				}
			}

			return ret;
		}

		public static BitSet operator ^(BitSet lhs, BitSet rhs)
		{
			int max = Math.Max(lhs.length_, rhs.length_);
			int min = Math.Min(lhs.length_, rhs.length_);
			var ret = new BitSet(Math.Max(lhs.BitCount, rhs.BitCount));
			for (int i = 0; i < min; i++) {
				ret.bits_[i] = lhs.bits_[i] ^ rhs.bits_[i];
			}

			if (lhs.length_ > rhs.length_) {
				for (int i = min; i < max; i++) {
					ret.bits_[i] = lhs.bits_[i];
				}
			} else {
				for (int i = min; i < max; i++) {
					ret.bits_[i] = rhs.bits_[i];
				}
			}

			return ret;
		}

		public static BitSet operator <<(BitSet target, int shift)
		{
			var ret = new BitSet(target.BitCount);
			if (shift > target.length_ << SHIFT) {
				return ret;
			}

			int minIndex = shift + 63 >> SHIFT;
			if (shift % BITS == 0) {
				for (int i = target.length_ - 1; i >= minIndex; i--) {
					ret.bits_[i] = target.bits_[i - minIndex];
				}
			} else {
				for (int i = target.length_ - 1; i >= minIndex; i--) {
					ret.bits_[i] = target.bits_[i - minIndex + 1] << shift
						| target.bits_[i - minIndex] >> BITS - shift;
				}

				ret.bits_[minIndex - 1] = target.bits_[0] << shift;
			}

			return ret;
		}

		public static BitSet operator >>(BitSet target, int shift)
		{
			var ret = new BitSet(target.BitCount);
			if (shift > target.length_ << SHIFT) {
				return ret;
			}

			int minIndex = shift + 63 >> SHIFT;
			if (shift % BITS == 0) {
				for (int i = 0; i + minIndex < ret.length_; i++) {
					ret.bits_[i] = target.bits_[i + minIndex];
				}
			} else {
				for (int i = 0; i + minIndex < ret.length_; i++) {
					ret.bits_[i] = target.bits_[i + minIndex - 1] >> shift
						| target.bits_[i + minIndex] << BITS - shift;
				}

				ret.bits_[ret.length_ - minIndex] = target.bits_[ret.length_ - 1] >> shift;
			}

			return ret;
		}

		public static bool operator ==(BitSet x1, BitSet x2) => x1.Equals(x2);
		public static bool operator !=(BitSet x1, BitSet x2) => !x1.Equals(x2);
		public static bool operator >(BitSet x1, BitSet x2) => x1.CompareTo(x2) > 0;
		public static bool operator <(BitSet x1, BitSet x2) => x1.CompareTo(x2) < 0;
		public static bool operator >=(BitSet x1, BitSet x2) => x1.CompareTo(x2) >= 0;
		public static bool operator <=(BitSet x1, BitSet x2) => x1.CompareTo(x2) <= 0;

		public override bool Equals(object obj) => obj is BitSet x && Equals(x);
		public bool Equals(BitSet other)
		{
			int max = Math.Max(length_, other.length_);
			int min = Math.Min(length_, other.length_);
			for (int i = 0; i < min; i++) {
				if (bits_[i] != other.bits_[i]) {
					return false;
				}
			}

			if (length_ > other.length_) {
				for (int i = min; i < max; i++) {
					if (bits_[i] != 0) {
						return false;
					}
				}
			} else {
				for (int i = min; i < max; i++) {
					if (other.bits_[i] != 0) {
						return false;
					}
				}
			}

			return true;
		}

		public int CompareTo(BitSet other)
		{
			int max = Math.Max(length_, other.length_);
			int min = Math.Min(length_, other.length_);
			if (length_ > other.length_) {
				for (int i = min; i < max; i++) {
					if (bits_[i] != 0) {
						return 1;
					}
				}
			} else {
				for (int i = min; i < max; i++) {
					if (other.bits_[i] != 0) {
						return -1;
					}
				}
			}

			for (int i = min - 1; i >= 0; --i) {
				if (bits_[i] > other.bits_[i]) {
					return 1;
				} else if (bits_[i] < other.bits_[i]) {
					return -1;
				}
			}

			return 0;
		}

		public void Or(BitSet target)
		{
			for (int i = 0; i < Math.Min(length_, target.length_); i++) {
				bits_[i] |= target.bits_[i];
			}
		}

		public void And(BitSet target)
		{
			for (int i = 0; i < Math.Min(length_, target.length_); i++) {
				bits_[i] &= target.bits_[i];
			}
		}

		public void Xor(BitSet target)
		{
			for (int i = 0; i < Math.Min(length_, target.length_); i++) {
				bits_[i] ^= target.bits_[i];
			}
		}

		public void RightShift(int shiftBits)
		{
			if (shiftBits <= 0) {
				return;
			}

			int toIndex = 0;
			if (shiftBits < BitCount) {
				var (fromIndex, shiftCount) = DivideByBits(shiftBits);
				if (shiftCount == 0) {
					unchecked {
						ulong mask = ulong.MaxValue >> BITS - extraBits_;
						bits_[length_ - 1] &= mask;
					}

					Array.Copy(bits_, fromIndex, bits_, 0, length_ - fromIndex);
					toIndex = length_ - fromIndex;
				} else {
					int lastIndex = length_ - 1;
					unchecked {
						while (fromIndex < lastIndex) {
							ulong right = bits_[fromIndex] >> shiftCount;
							fromIndex++;
							ulong left = bits_[fromIndex] << BITS - shiftCount;
							bits_[toIndex] = left | right;
							toIndex++;
						}

						ulong mask = ulong.MaxValue >> BITS - extraBits_;
						mask &= bits_[fromIndex];
						bits_[toIndex] = mask >> shiftCount;
						toIndex++;
					}
				}
			}

			bits_.AsSpan(toIndex, length_ - toIndex).Clear();
		}

		public void LeftShift(int shiftBits)
		{
			if (shiftBits <= 0) {
				return;
			}

			int shiftBlockCount;
			if (shiftBits < BitCount) {
				int lastIndex = BitCount - 1 >> SHIFT;
				int shiftBitCount;
				(shiftBlockCount, shiftBitCount) = DivideByBits(shiftBits);
				if (shiftBitCount == 0) {
					Array.Copy(bits_, 0, bits_, shiftBlockCount, lastIndex + 1 - shiftBlockCount);
				} else {
					int fromIndex = lastIndex - shiftBlockCount;
					unchecked {
						while (fromIndex > 0) {
							ulong left = bits_[fromIndex] << shiftBitCount;
							--fromIndex;
							ulong right = bits_[fromIndex] >> BITS - shiftBitCount;
							bits_[lastIndex] = left | right;
							lastIndex--;
						}

						bits_[lastIndex] = bits_[fromIndex] << shiftBitCount;
					}
				}
			} else {
				shiftBlockCount = length_;
			}

			bits_.AsSpan(0, shiftBlockCount).Clear();
		}

		public void OrWithRightShift(int shiftBits)
		{
			if (shiftBits <= 0) {
				return;
			}

			int toIndex = 0;
			if (shiftBits < BitCount) {
				var (fromIndex, shiftCount) = DivideByBits(shiftBits);
				if (shiftCount == 0) {
					unchecked {
						ulong mask = ulong.MaxValue >> BITS - extraBits_;
						bits_[length_ - 1] &= mask;
					}

					for (int i = 0; i < length_ - fromIndex; ++i) {
						bits_[i] |= bits_[i + fromIndex];
					}
				} else {
					int lastIndex = length_ - 1;
					unchecked {
						while (fromIndex < lastIndex) {
							ulong right = bits_[fromIndex] >> shiftCount;
							fromIndex++;
							ulong left = bits_[fromIndex] << BITS - shiftCount;
							bits_[toIndex] |= left | right;
							toIndex++;
						}

						ulong mask = ulong.MaxValue >> BITS - extraBits_;
						mask &= bits_[fromIndex];
						bits_[toIndex] |= mask >> shiftCount;
					}
				}
			}
		}

		public void OrWithLeftShift(int shiftBits)
		{
			if (shiftBits <= 0) {
				return;
			}

			int shiftBlockCount;
			if (shiftBits < BitCount) {
				int lastIndex = BitCount - 1 >> SHIFT;
				int shiftBitCount;
				(shiftBlockCount, shiftBitCount) = DivideByBits(shiftBits);
				if (shiftBitCount == 0) {
					for (int i = 0; i < lastIndex + 1 - shiftBlockCount; i++) {
						bits_[lastIndex - i] |= bits_[lastIndex - i - shiftBlockCount];
					}
				} else {
					int fromIndex = lastIndex - shiftBlockCount;
					unchecked {
						while (fromIndex > 0) {
							ulong left = bits_[fromIndex] << shiftBitCount;
							--fromIndex;
							ulong right = bits_[fromIndex] >> BITS - shiftBitCount;
							bits_[lastIndex] |= left | right;
							lastIndex--;
						}

						bits_[lastIndex] |= bits_[fromIndex] << shiftBitCount;
					}
				}

				unchecked {
					lastIndex = BitCount - 1 >> SHIFT;
					ulong mask = ulong.MaxValue >> BITS - extraBits_;
					bits_[lastIndex] &= mask;
				}
			}
		}

		public int Msb()
		{
			for (int i = length_ - 1; i >= 0; i--) {
				if (bits_[i] != 0) {
					return BITS - BitOperations.LeadingZeroCount(bits_[i]) + BITS * i;
				}
			}

			return 0;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(length_, bits_, BitCount);
		}

		public override string ToString()
		{
			string ret = "";
			for (int i = 0; i < length_ - 1; i++) {
				string temp = Convert.ToString((long)bits_[i], 2);
				temp = (BITS > temp.Length ? new string('0', BITS - temp.Length) : "")
					+ temp;
				ret = temp + ret;
			}

			{
				string temp = Convert.ToString((long)bits_[length_ - 1], 2);
				temp = (BitCount % BITS > temp.Length ? new string('0', BitCount % BITS - temp.Length) : "")
					+ temp;
				ret = temp + ret;
			}

			return ret;
		}

		private static (int quotient, int remainder) DivideByBits(int number)
		{
			uint quotient = (uint)number / BITS;
			int remainder = number & BITS - 1;
			return ((int)quotient, remainder);
		}

	}
}

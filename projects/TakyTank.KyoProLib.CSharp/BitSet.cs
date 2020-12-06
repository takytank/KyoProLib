using System;
using System.Collections.Generic;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class BitSet
	{
		private const int SHIFT = 6;
		private const int BITS = 64;

		private readonly int length_;
		private readonly ulong[] bits_;

		public int BitCount { get; }
		public BitSet(int bitCount, bool fillTrue = false)
		{
			BitCount = bitCount;
			length_ = ((bitCount - 1) >> SHIFT) + 1;
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
			length_ = ((bitCount - 1) >> SHIFT) + 1;
			bits_ = new ulong[length_];
			bits_[0] = (ulong)initialValue;
		}

		public bool this[int i]
		{
			get => ((bits_[i >> SHIFT] >> i) & 1) != 0;
			set => bits_[i >> 6]
				= (bits_[i >> SHIFT] & (ulong.MaxValue ^ (1ul << i)))
				| ((ulong)(value ? 1 : 0) << i);
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

			int minIndex = (shift + 63) >> SHIFT;
			if (shift % BITS == 0) {
				for (int i = target.length_ - 1; i >= minIndex; i--) {
					ret.bits_[i] = target.bits_[i - minIndex];
				}
			} else {
				for (int i = target.length_ - 1; i >= minIndex; i--) {
					ret.bits_[i] = (target.bits_[i - minIndex + 1] << shift)
						| (target.bits_[i - minIndex] >> (BITS - shift));
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

			int minIndex = (shift + 63) >> SHIFT;
			if (shift % BITS == 0) {
				for (int i = 0; i + minIndex < ret.length_; i++) {
					ret.bits_[i] = target.bits_[i + minIndex];
				}
			} else {
				for (int i = 0; i + minIndex < ret.length_; i++) {
					ret.bits_[i] = (target.bits_[i + minIndex - 1] >> shift)
						| (target.bits_[i + minIndex] << (BITS - shift));
				}

				ret.bits_[ret.length_ - minIndex] = target.bits_[ret.length_ - 1] >> shift;
			}

			return ret;
		}

		public static bool operator ==(BitSet lhs, BitSet rhs)
		{
			int max = Math.Max(lhs.length_, rhs.length_);
			int min = Math.Min(lhs.length_, rhs.length_);
			for (int i = 0; i < min; i++) {
				if (lhs.bits_[i] != rhs.bits_[i]) {
					return false;
				}
			}

			if (lhs.length_ > rhs.length_) {
				for (int i = min; i < max; i++) {
					if (lhs.bits_[i] != 0) {
						return false;
					}
				}
			} else {
				for (int i = min; i < max; i++) {
					if (rhs.bits_[i] != 0) {
						return false;
					}
				}
			}

			return true;
		}

		public static bool operator !=(BitSet lhs, BitSet rhs)
		{
			return !(lhs == rhs);
		}

		public override bool Equals(object obj)
		{
			return false;
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
				var (_, extraBits) = DivideByBits(BitCount);
				if (shiftCount == 0) {
					unchecked {
						ulong mask = ulong.MaxValue >> (BITS - extraBits);
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
							ulong left = bits_[fromIndex] << (BITS - shiftCount);
							bits_[toIndex] = left | right;
							toIndex++;
						}

						ulong mask = ulong.MaxValue >> (BITS - extraBits);
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

			int lengthToClear;
			if (shiftBits < BitCount) {
				int lastIndex = (BitCount - 1) >> SHIFT;
				int shiftCount;
				(lengthToClear, shiftCount) = DivideByBits(shiftBits);
				if (shiftCount == 0) {
					Array.Copy(bits_, 0, bits_, lengthToClear, lastIndex + 1 - lengthToClear);
				} else {
					int fromIndex = lastIndex - lengthToClear;
					unchecked {
						while (fromIndex > 0) {
							ulong left = bits_[fromIndex] << shiftCount;
							--fromIndex;
							ulong right = bits_[fromIndex] >> (BITS - shiftCount);
							bits_[lastIndex] = left | right;
							lastIndex--;
						}

						bits_[lastIndex] = bits_[fromIndex] << shiftCount;
					}
				}
			} else {
				lengthToClear = length_;
			}

			bits_.AsSpan(0, lengthToClear).Clear();
		}

		public void OrWithRightShift(int shiftBits)
		{
			if (shiftBits <= 0) {
				return;
			}

			int toIndex = 0;
			if (shiftBits < BitCount) {
				var (fromIndex, shiftCount) = DivideByBits(shiftBits);
				var (_, extraBits) = DivideByBits(BitCount);
				if (shiftCount == 0) {
					unchecked {
						ulong mask = ulong.MaxValue >> (BITS - extraBits);
						bits_[length_ - 1] &= mask;
					}

					Array.Copy(bits_, fromIndex, bits_, 0, length_ - fromIndex);
				} else {
					int lastIndex = length_ - 1;
					unchecked {
						while (fromIndex < lastIndex) {
							ulong right = bits_[fromIndex] >> shiftCount;
							fromIndex++;
							ulong left = bits_[fromIndex] << (BITS - shiftCount);
							bits_[toIndex] |= left | right;
							toIndex++;
						}

						ulong mask = ulong.MaxValue >> (BITS - extraBits);
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

			int lengthToClear;
			if (shiftBits < BitCount) {
				int lastIndex = (BitCount - 1) >> SHIFT;
				int shiftCount;
				(lengthToClear, shiftCount) = DivideByBits(shiftBits);
				if (shiftCount == 0) {
					Array.Copy(bits_, 0, bits_, lengthToClear, lastIndex + 1 - lengthToClear);
				} else {
					int fromIndex = lastIndex - lengthToClear;
					unchecked {
						while (fromIndex > 0) {
							ulong left = bits_[fromIndex] << shiftCount;
							--fromIndex;
							ulong right = bits_[fromIndex] >> (BITS - shiftCount);
							bits_[lastIndex] |= left | right;

							lastIndex--;
						}

						bits_[lastIndex] |= bits_[fromIndex] << shiftCount;
					}
				}
			}
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
				temp = ((BitCount % BITS) > temp.Length ? new string('0', (BitCount % BITS) - temp.Length) : "")
					+ temp;
				ret = temp + ret;
			}

			return ret;
		}

		private static (int quotient, int remainder) DivideByBits(int number)
		{
			uint quotient = (uint)number / BITS;
			int remainder = number & (BITS - 1);
			return ((int)quotient, remainder);
		}

	}
}

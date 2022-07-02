using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V8
{
	public class BitSet : IComparable<BitSet>
	{
		private const int SHIFT = 6;
		private const int BITS = 64;

		private readonly int _length;
		private readonly ulong[] _bits;
		private readonly int _extraBits;

		public int BitCount { get; }
		public ulong[] Raw => _bits;
		public BitSet(int bitCount, bool fillTrue = false)
		{
			BitCount = bitCount;
			_extraBits = DivideByBits(BitCount).remainder;
			_length = (bitCount - 1 >> SHIFT) + 1;
			_bits = new ulong[_length];
			if (fillTrue) {
				int y = bitCount % BITS;
				if (y == 0) {
					for (int i = 0; i < _length; i++) {
						_bits[i] = ulong.MaxValue;
					}
				} else {
					for (int i = 0; i < _length - 1; i++) {
						_bits[i] = ulong.MaxValue;
					}

					_bits[_length - 1] = (1UL << y) - 1;
				}
			}
		}

		public BitSet(int bitCount, long initialValue)
		{
			BitCount = bitCount;
			_extraBits = DivideByBits(BitCount).remainder;
			_length = (bitCount - 1 >> SHIFT) + 1;
			_bits = new ulong[_length];
			_bits[0] = (ulong)initialValue;
		}

		public bool this[int i]
		{
			get => (_bits[i >> SHIFT] >> i & 1) != 0;
			set => _bits[i >> SHIFT]
				= _bits[i >> SHIFT] & (ulong.MaxValue ^ 1ul << i)
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
			int min = Math.Min(lhs._length, rhs._length);
			var ret = new BitSet(Math.Max(lhs.BitCount, rhs.BitCount));
			for (int i = 0; i < min; i++) {
				ret._bits[i] = lhs._bits[i] & rhs._bits[i];
			}

			return ret;
		}

		public static BitSet operator |(BitSet lhs, BitSet rhs)
		{
			int max = Math.Max(lhs._length, rhs._length);
			int min = Math.Min(lhs._length, rhs._length);
			var ret = new BitSet(Math.Max(lhs.BitCount, rhs.BitCount));
			for (int i = 0; i < min; i++) {
				ret._bits[i] = lhs._bits[i] | rhs._bits[i];
			}

			if (lhs._length > rhs._length) {
				for (int i = min; i < max; i++) {
					ret._bits[i] = lhs._bits[i];
				}
			} else {
				for (int i = min; i < max; i++) {
					ret._bits[i] = rhs._bits[i];
				}
			}

			return ret;
		}

		public static BitSet operator ^(BitSet lhs, BitSet rhs)
		{
			int max = Math.Max(lhs._length, rhs._length);
			int min = Math.Min(lhs._length, rhs._length);
			var ret = new BitSet(Math.Max(lhs.BitCount, rhs.BitCount));
			for (int i = 0; i < min; i++) {
				ret._bits[i] = lhs._bits[i] ^ rhs._bits[i];
			}

			if (lhs._length > rhs._length) {
				for (int i = min; i < max; i++) {
					ret._bits[i] = lhs._bits[i];
				}
			} else {
				for (int i = min; i < max; i++) {
					ret._bits[i] = rhs._bits[i];
				}
			}

			return ret;
		}

		public static BitSet operator <<(BitSet target, int shift)
		{
			var ret = new BitSet(target.BitCount);
			if (shift > target._length << SHIFT) {
				return ret;
			}

			int minIndex = shift + (BITS - 1) >> SHIFT;
			if (shift % BITS == 0) {
				for (int i = target._length - 1; i >= minIndex; i--) {
					ret._bits[i] = target._bits[i - minIndex];
				}
			} else {
				for (int i = target._length - 1; i >= minIndex; i--) {
					ret._bits[i] = target._bits[i - minIndex + 1] << shift
						| target._bits[i - minIndex] >> BITS - shift;
				}

				ret._bits[minIndex - 1] = target._bits[0] << shift;
			}

			return ret;
		}

		public static BitSet operator >>(BitSet target, int shift)
		{
			var ret = new BitSet(target.BitCount);
			if (shift > target._length << SHIFT) {
				return ret;
			}

			int minIndex = shift + (BITS - 1) >> SHIFT;
			if (shift % BITS == 0) {
				for (int i = 0; i + minIndex < ret._length; i++) {
					ret._bits[i] = target._bits[i + minIndex];
				}
			} else {
				for (int i = 0; i + minIndex < ret._length; i++) {
					ret._bits[i] = target._bits[i + minIndex - 1] >> shift
						| target._bits[i + minIndex] << BITS - shift;
				}

				ret._bits[ret._length - minIndex] = target._bits[ret._length - 1] >> shift;
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
			int max = Math.Max(_length, other._length);
			int min = Math.Min(_length, other._length);
			for (int i = 0; i < min; i++) {
				if (_bits[i] != other._bits[i]) {
					return false;
				}
			}

			if (_length > other._length) {
				for (int i = min; i < max; i++) {
					if (_bits[i] != 0) {
						return false;
					}
				}
			} else {
				for (int i = min; i < max; i++) {
					if (other._bits[i] != 0) {
						return false;
					}
				}
			}

			return true;
		}

		public int CompareTo(BitSet other)
		{
			int max = Math.Max(_length, other._length);
			int min = Math.Min(_length, other._length);
			if (_length > other._length) {
				for (int i = min; i < max; i++) {
					if (_bits[i] != 0) {
						return 1;
					}
				}
			} else {
				for (int i = min; i < max; i++) {
					if (other._bits[i] != 0) {
						return -1;
					}
				}
			}

			for (int i = min - 1; i >= 0; --i) {
				if (_bits[i] > other._bits[i]) {
					return 1;
				} else if (_bits[i] < other._bits[i]) {
					return -1;
				}
			}

			return 0;
		}

		public void Or(BitSet target)
		{
			for (int i = 0; i < Math.Min(_length, target._length); i++) {
				_bits[i] |= target._bits[i];
			}
		}

		public void And(BitSet target)
		{
			for (int i = 0; i < Math.Min(_length, target._length); i++) {
				_bits[i] &= target._bits[i];
			}
		}

		public void Xor(BitSet target)
		{
			for (int i = 0; i < Math.Min(_length, target._length); i++) {
				_bits[i] ^= target._bits[i];
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
						ulong mask = ulong.MaxValue >> BITS - _extraBits;
						_bits[_length - 1] &= mask;
					}

					Array.Copy(_bits, fromIndex, _bits, 0, _length - fromIndex);
					toIndex = _length - fromIndex;
				} else {
					int lastIndex = _length - 1;
					unchecked {
						while (fromIndex < lastIndex) {
							ulong right = _bits[fromIndex] >> shiftCount;
							fromIndex++;
							ulong left = _bits[fromIndex] << BITS - shiftCount;
							_bits[toIndex] = left | right;
							toIndex++;
						}

						ulong mask = ulong.MaxValue >> BITS - _extraBits;
						mask &= _bits[fromIndex];
						_bits[toIndex] = mask >> shiftCount;
						toIndex++;
					}
				}
			}

			_bits.AsSpan(toIndex, _length - toIndex).Clear();
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
					Array.Copy(_bits, 0, _bits, shiftBlockCount, lastIndex + 1 - shiftBlockCount);
				} else {
					int fromIndex = lastIndex - shiftBlockCount;
					unchecked {
						while (fromIndex > 0) {
							ulong left = _bits[fromIndex] << shiftBitCount;
							--fromIndex;
							ulong right = _bits[fromIndex] >> BITS - shiftBitCount;
							_bits[lastIndex] = left | right;
							lastIndex--;
						}

						_bits[lastIndex] = _bits[fromIndex] << shiftBitCount;
					}
				}
			} else {
				shiftBlockCount = _length;
			}

			_bits.AsSpan(0, shiftBlockCount).Clear();
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
						ulong mask = ulong.MaxValue >> BITS - _extraBits;
						_bits[_length - 1] &= mask;
					}

					for (int i = 0; i < _length - fromIndex; ++i) {
						_bits[i] |= _bits[i + fromIndex];
					}
				} else {
					int lastIndex = _length - 1;
					unchecked {
						while (fromIndex < lastIndex) {
							ulong right = _bits[fromIndex] >> shiftCount;
							fromIndex++;
							ulong left = _bits[fromIndex] << BITS - shiftCount;
							_bits[toIndex] |= left | right;
							toIndex++;
						}

						ulong mask = ulong.MaxValue >> BITS - _extraBits;
						mask &= _bits[fromIndex];
						_bits[toIndex] |= mask >> shiftCount;
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
						_bits[lastIndex - i] |= _bits[lastIndex - i - shiftBlockCount];
					}
				} else {
					int fromIndex = lastIndex - shiftBlockCount;
					unchecked {
						while (fromIndex > 0) {
							ulong left = _bits[fromIndex] << shiftBitCount;
							--fromIndex;
							ulong right = _bits[fromIndex] >> BITS - shiftBitCount;
							_bits[lastIndex] |= left | right;
							lastIndex--;
						}

						_bits[lastIndex] |= _bits[fromIndex] << shiftBitCount;
					}
				}

				unchecked {
					lastIndex = BitCount - 1 >> SHIFT;
					ulong mask = ulong.MaxValue >> BITS - _extraBits;
					_bits[lastIndex] &= mask;
				}
			}
		}

		public int Msb()
		{
			for (int i = _length - 1; i >= 0; i--) {
				if (_bits[i] != 0) {
					return BITS - BitOperations.LeadingZeroCount(_bits[i]) + BITS * i;
				}
			}

			return 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		public int PopCount()
		{
			int count = 0;
			for (int i = 0; i < _bits.Length; i++) {
				count += BitOperations.PopCount(_bits[i]);
			}

			return count;
		}

		public int PopCount2()
		{
			int count = 0;
			for (int i = 0; i < _bits.Length; i++) {
				var bits = _bits[i];
				bits -= (bits >> 1) & 0x5555555555555555;
				bits = (bits & 0x3333333333333333) + (bits >> 2 & 0x3333333333333333);
				count += (int)(((bits + (bits >> 4)) & 0xf0f0f0f0f0f0f0f) * 0x101010101010101 >> 56);
			}

			return count;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(_length, _bits, BitCount);
		}

		public override string ToString()
		{
			string ret = "";
			for (int i = 0; i < _length - 1; i++) {
				string temp = Convert.ToString((long)_bits[i], 2);
				temp = (BITS > temp.Length ? new string('0', BITS - temp.Length) : "")
					+ temp;
				ret = temp + ret;
			}

			{
				string temp = Convert.ToString((long)_bits[_length - 1], 2);
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

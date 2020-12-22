using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.Core31
{
	public class RadixHeap32<T>
	{
		private const int MAX_BIT = 32;
		private readonly List<(uint key, T value)>[] buckets_;
		private readonly uint[] mins_;
		private uint lastKey_;

		public int Count { get; private set; }

		public RadixHeap32()
		{
			buckets_ = new List<(uint key, T value)>[MAX_BIT + 1];
			for (int i = 0; i < MAX_BIT + 1; ++i) {
				buckets_[i] = new List<(uint key, T value)>();
			}

			mins_ = new uint[MAX_BIT + 1];
			mins_.AsSpan().Fill(uint.MaxValue);
		}

		public void Push(uint key, T value)
		{
			++Count;
			int bit = GetBit(key ^ lastKey_);
			buckets_[bit].Add((key, value));
			mins_[bit] = Math.Min(mins_[bit], key);
		}

		public (uint key, T value) Pop()
		{
			if (Count == 0) {
				throw new Exception("heap is empty.");
			}

			Pull();

			--Count;
			var ret = buckets_[0][^1];
			buckets_[0].RemoveAt(buckets_[0].Count - 1);
			return ret;
		}

		public (uint key, T value) Peek()
		{
			if (Count == 0) {
				throw new Exception("heap is empty.");
			}

			Pull();
			return buckets_[0][^1];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Pull()
		{
			if (buckets_[0].Count == 0) {
				int index = 1;
				while (buckets_[index].Count == 0) {
					++index;
				}

				lastKey_ = mins_[index];
				foreach (var item in buckets_[index]) {
					int bit = GetBit(item.key ^ lastKey_);
					buckets_[bit].Add(item);
					mins_[bit] = Math.Min(mins_[bit], item.key);
				}

				buckets_[index].Clear();
				mins_[index] = uint.MaxValue;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int GetBit(uint x)
			=> x != 0 ? MAX_BIT - BitOperations.LeadingZeroCount(x) : 0;
	}

	public class RadixHeap64<T>
	{
		private const int MAX_BIT = 64;
		private readonly List<(ulong key, T value)>[] buckets_;
		private readonly ulong[] mins_;
		private ulong lastKey_;

		public int Count { get; private set; }

		public RadixHeap64()
		{
			buckets_ = new List<(ulong key, T value)>[MAX_BIT + 1];
			for (int i = 0; i < MAX_BIT + 1; ++i) {
				buckets_[i] = new List<(ulong key, T value)>();
			}

			mins_ = new ulong[MAX_BIT + 1];
			mins_.AsSpan().Fill(ulong.MaxValue);
		}

		public void Push(ulong key, T value)
		{
			++Count;
			int bit = GetBit(key ^ lastKey_);
			buckets_[bit].Add((key, value));
			mins_[bit] = Math.Min(mins_[bit], key);
		}

		public (ulong key, T value) Pop()
		{
			if (Count == 0) {
				throw new Exception("heap is empty.");
			}

			Pull();

			--Count;
			var ret = buckets_[0][^1];
			buckets_[0].RemoveAt(buckets_[0].Count - 1);
			return ret;
		}

		public (ulong key, T value) Peek()
		{
			if (Count == 0) {
				throw new Exception("heap is empty.");
			}

			Pull();
			return buckets_[0][^1];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Pull()
		{
			if (buckets_[0].Count == 0) {
				int index = 1;
				while (buckets_[index].Count == 0) {
					++index;
				}

				lastKey_ = mins_[index];
				foreach (var item in buckets_[index]) {
					int bit = GetBit(item.key ^ lastKey_);
					buckets_[bit].Add(item);
					mins_[bit] = Math.Min(mins_[bit], item.key);
				}

				buckets_[index].Clear();
				mins_[index] = ulong.MaxValue;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int GetBit(ulong x)
			=> x != 0 ? MAX_BIT - BitOperations.LeadingZeroCount(x) : 0;
	}
}

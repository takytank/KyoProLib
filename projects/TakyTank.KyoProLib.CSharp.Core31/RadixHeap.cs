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
		private readonly List<(uint priority, T value)>[] buckets_
			= new List<(uint priority, T value)>[MAX_BIT + 1];
		private uint lastPriority_;

		public RadixHeap32()
		{
			for (int i = 0; i < MAX_BIT + 1; i++) {
				buckets_[i] = new List<(uint priority, T value)>();
			}
		}

		public int Count { get; private set; }

		public void Push(uint priority, T value)
		{
			++Count;
			buckets_[GetBit(priority ^ lastPriority_)].Add((priority, value));
		}

		public (uint priority, T value) Pop()
		{
			if (Count == 0) {
				return (0, default);
			}

			if (buckets_[0].Count == 0) {
				int index = 1;
				while (buckets_[index].Count == 0) {
					++index;
				}

				lastPriority_ = buckets_[index].Min(x => x.priority);
				foreach (var p in buckets_[index]) {
					buckets_[GetBit(p.priority ^ lastPriority_)].Add(p);
				}

				buckets_[index].Clear();
			}

			--Count;
			var ret = buckets_[0][^1];
			buckets_[0].RemoveAt(buckets_[0].Count - 1);
			return ret;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int GetBit(uint a)
			=> a != 0 ? MAX_BIT - BitOperations.LeadingZeroCount(a) : 0;
	}
}

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace Benchmarker
{
	public class MeasureBitOp
	{
		private readonly uint[] num32_;
		private readonly ulong[] num64_;
		
		public MeasureBitOp()
		{
			int n = 100000000;
			var rnd = new Random();

			num32_ = new uint[n];
			for (int i = 0; i < n; i++) {
				num32_[i] = (uint)rnd.Next();
			}

			num64_ = new ulong[n];
			for (int i = 0; i < n; i++) {
				num64_[i] = ((ulong)rnd.Next() << 32) + (ulong)rnd.Next();
			}
		}

		[Benchmark]
		public long LeadingZeroCount32()
		{
			long count = 0;
			foreach (uint value in num32_) {
				int temp = 32;
				for (int i = 32 - 1; i >= 0; i--) {
					if ((value & (1 << i)) != 0) {
						temp = 31 - i;
						break;
					}
				}

				count += temp;
			}

			return count;
		}

		[Benchmark]
		public long LeadingZeroCount32PopCount()
		{
			long count = 0;
			foreach (uint value in num32_) {
				uint x = value;
				x |= (x >> 1);
				x |= (x >> 2);
				x |= (x >> 4);
				x |= (x >> 8);
				x |= (x >> 16);

				x = (x & 0x55555555) + (x >> 1 & 0x55555555);
				x = (x & 0x33333333) + (x >> 2 & 0x33333333);
				x = (x & 0x0f0f0f0f) + (x >> 4 & 0x0f0f0f0f);
				x = (x & 0x00ff00ff) + (x >> 8 & 0x00ff00ff);
				x = (x & 0x0000ffff) + (x >> 16 & 0x0000ffff);

				count += (int)x;
			}

			return count;
		}

		[Benchmark]
		public long LeadingZeroCount32Union()
		{
			long count = 0;
			foreach (uint value in num32_) {
				var data = default(Union32);
				data.AsFloat = (float)value + 0.5f;
				count += 158 - (data.AsUint >> 23);
			}

			return count;
		}

		[Benchmark]
		public long LeadingZeroCount32BitOperations()
		{
			long count = 0;
			foreach (uint value in num32_) {
				count += value != 0 ? BitOperations.LeadingZeroCount(value) : 0;
			}

			return count;
		}

		[Benchmark]
		public long LeadingZeroCount64()
		{
			long count = 0;
			foreach (uint value in num64_) {
				int temp = 64;
				for (int i = 64 - 1; i >= 0; i--) {
					if ((value & (1 << i)) != 0) {
						temp = 63 - i;
						break;
					}
				}

				count += temp;
			}

			return count;
		}


		[Benchmark]
		public long LeadingZeroCount64PopCount()
		{
			long count = 0;
			foreach (ulong value in num64_) {
				ulong x = value;
				x |= (x >> 1);
				x |= (x >> 2);
				x |= (x >> 4);
				x |= (x >> 8);
				x |= (x >> 16);
				x |= (x >> 32);

				x = ((x & 0xaaaaaaaaaaaaaaaa) >> 1) + (x & 0x5555555555555555);
				x = ((x & 0xcccccccccccccccc) >> 2) + (x & 0x3333333333333333);
				x = ((x & 0xf0f0f0f0f0f0f0f0) >> 4) + (x & 0x0f0f0f0f0f0f0f0f);
				x = ((x & 0xff00ff00ff00ff00) >> 8) + (x & 0x00ff00ff00ff00ff);
				x = ((x & 0xffff0000ffff0000) >> 16) + (x & 0x0000ffff0000ffff);
				x = ((x & 0xffffffff00000000) >> 32) + (x & 0x00000000ffffffff);
				
				count += (int)x;
			}

			return count;
		}

		[Benchmark]
		public long LeadingZeroCount64Union()
		{
			long count = 0;
			foreach (ulong value in num64_) {
				var data = default(Union64);
				data.AsDouble = (double)value + 0.5;
				count += (int)(1054 - (data.AsUlong >> 52));
			}

			return count;
		}

		[Benchmark]
		public long LeadingZeroCount64BitOperations()
		{
			long count = 0;
			foreach (ulong value in num64_) {
				count += value != 0 ? BitOperations.LeadingZeroCount(value) : 0;
			}

			return count;
		}

		[StructLayout(LayoutKind.Explicit)]
		public struct Union64
		{
			[FieldOffset(0)]
			public ulong AsUlong;

			[FieldOffset(0)]
			public double AsDouble;
		}

		[StructLayout(LayoutKind.Explicit)]
		public struct Union32
		{
			[FieldOffset(0)]
			public uint AsUint;

			[FieldOffset(0)]
			public float AsFloat;
		}
	}
}

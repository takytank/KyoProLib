using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V8
{
	public class DijkstraQ
	{
		private int count_ = 0;
		private long[] distanceHeap_;
		private int[] vertexHeap_;

		public int Count => count_;
		public DijkstraQ()
		{
			distanceHeap_ = new long[8];
			vertexHeap_ = new int[8];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Enqueue(long distance, int v)
		{
			if (distanceHeap_.Length == count_) {
				var newDistanceHeap = new long[distanceHeap_.Length << 1];
				var newVertexHeap = new int[vertexHeap_.Length << 1];
				Unsafe.CopyBlock(
					ref Unsafe.As<long, byte>(ref newDistanceHeap[0]),
					ref Unsafe.As<long, byte>(ref distanceHeap_[0]),
					(uint)(8 * count_));
				Unsafe.CopyBlock(
					ref Unsafe.As<int, byte>(ref newVertexHeap[0]),
					ref Unsafe.As<int, byte>(ref vertexHeap_[0]),
					(uint)(4 * count_));
				distanceHeap_ = newDistanceHeap;
				vertexHeap_ = newVertexHeap;
			}

			ref var dRef = ref distanceHeap_[0];
			ref var vRef = ref vertexHeap_[0];
			Unsafe.Add(ref dRef, count_) = distance;
			Unsafe.Add(ref vRef, count_) = v;
			++count_;

			int c = count_ - 1;
			while (c > 0) {
				int p = c - 1 >> 1;
				var tempD = Unsafe.Add(ref dRef, p);
				if (tempD <= distance) {
					break;
				} else {
					Unsafe.Add(ref dRef, c) = tempD;
					Unsafe.Add(ref vRef, c) = Unsafe.Add(ref vRef, p);
					c = p;
				}
			}

			Unsafe.Add(ref dRef, c) = distance;
			Unsafe.Add(ref vRef, c) = v;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long distance, int v) Dequeue()
		{
			ref var dRef = ref distanceHeap_[0];
			ref var vRef = ref vertexHeap_[0];
			(long distance, int v) ret = (dRef, vRef);
			int n = count_ - 1;

			var distance = Unsafe.Add(ref dRef, n);
			var vertex = Unsafe.Add(ref vRef, n);
			int p = 0;
			int c = (p << 1) + 1;
			while (c < n) {
				if (c != n - 1 && Unsafe.Add(ref dRef, c + 1) < Unsafe.Add(ref dRef, c)) {
					++c;
				}

				var tempD = Unsafe.Add(ref dRef, c);
				if (distance > tempD) {
					Unsafe.Add(ref dRef, p) = tempD;
					Unsafe.Add(ref vRef, p) = Unsafe.Add(ref vRef, c);
					p = c;
					c = (p << 1) + 1;
				} else {
					break;
				}
			}

			Unsafe.Add(ref dRef, p) = distance;
			Unsafe.Add(ref vRef, p) = vertex;
			--count_;

			return ret;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class DijkstraQ
	{
		private int count_ = 0;
		private (long distance, int v)[] heap_;

		public int Count => count_;
		public DijkstraQ()
		{
			heap_ = new (long distance, int v)[2];
		}

		public void Enqueue(long distance, int v)
		{
			var pair = (distance, v);
			if (heap_.Length == count_) {
				var newHeap = new (long distance, int v)[heap_.Length * 2];
				heap_.CopyTo(newHeap, 0);
				heap_ = newHeap;
			}

			heap_[count_] = pair;
			++count_;

			int c = count_ - 1;
			while (c > 0) {
				int p = (c - 1) >> 1;
				if (heap_[p].distance <= distance) {
					break;
				} else {
					heap_[c] = heap_[p];
					c = p;
				}
			}

			heap_[c] = pair;
		}

		public (long distance, int v) Dequeue()
		{
			(long distance, int v) ret = heap_[0];
			int n = count_ - 1;

			var item = heap_[n];
			int p = 0;
			int c = (p << 1) + 1;
			while (c < n) {
				if (c != n - 1 && heap_[c + 1].distance < heap_[c].distance) {
					++c;
				}

				if (item.distance > heap_[c].distance) {
					heap_[p] = heap_[c];
					p = c;
					c = (p << 1) + 1;
				} else {
					break;	
				}
			}

			heap_[p] = item;
			--count_;

			return ret;
		}
	}
}

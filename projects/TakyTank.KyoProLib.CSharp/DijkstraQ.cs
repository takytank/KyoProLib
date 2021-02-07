using System;
using System.Collections.Generic;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class DijkstraQ
	{
		private (long distance, int v)[] heap_;

		public int Count { get; private set; } = 0;
		public DijkstraQ()
		{
			heap_ = new (long distance, int v)[2];
		}

		public void Enqueue(long distance, int v)
		{
			var pair = (distance, v);
			if (heap_.Length == Count) {
				var newHeap = new (long distance, int v)[heap_.Length * 2];
				heap_.CopyTo(newHeap, 0);
				heap_ = newHeap;
			}

			heap_[Count] = pair;
			++Count;

			int c = Count - 1;
			while (c > 0) {
				int p = (c - 1) >> 1;
				if (heap_[p].distance > distance) {
					heap_[c] = heap_[p];
					c = p;
				} else {
					break;
				}
			}

			heap_[c] = pair;
		}

		public (long distance, int v) Dequeue()
		{
			(long distance, int v) ret = heap_[0];
			int n = Count - 1;

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
			Count--;

			return ret;
		}
	}
}

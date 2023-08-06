using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V11
{
	public class BellmanFord
	{
		private const long INFINITY = long.MaxValue;

		private readonly int count_;
		private readonly LightList<Edge> edges_;
		private readonly LightList<int>[] to_;
		private readonly LightList<int>[] from_;

		public BellmanFord(int n)
		{
			count_ = n;
			edges_ = new LightList<Edge>(n);
			to_ = Enumerable.Range(0, n).Select(x => new LightList<int>()).ToArray();
			from_ = Enumerable.Range(0, n).Select(x => new LightList<int>()).ToArray();
		}

		public void AddEdge(int from, int to, long cost)
		{
			edges_.Add(new Edge(from, to, cost));
			to_[from].Add(to);
			from_[to].Add(from);
		}

		public (long[] distances, bool existsNegativeCycle) MinDistance(int startIndex)
		{
			long[] distances = new long[count_];
			for (int i = 0; i < count_; i++) {
				if (i != startIndex) {
					distances[i] = INFINITY;
				}
			}

			bool existsNegativeCycle = false;
			for (int i = 0; i < count_; i++) {
				bool changes = false;
				foreach (var edge in edges_.AsSpan()) {
					if (distances[edge.From] != INFINITY) {
						long newDistance = distances[edge.From] + edge.Cost;
						if (newDistance < distances[edge.To]) {
							changes = true;
							distances[edge.To] = newDistance;
						}
					}
				}

				if (i == count_ - 1) {
					existsNegativeCycle = changes;
				}

				if (changes == false) {
					break;
				}
			}

			return (distances, existsNegativeCycle);
		}

		public (long distance, bool existsNegativeCycle) MinDistance(int startIndex, int endIndex)
		{
			long[] distances = new long[count_];
			for (int i = 0; i < count_; i++) {
				if (i != startIndex) {
					distances[i] = INFINITY;
				}
			}

			bool[] reachableFromStart = new bool[count_];
			bool[] reachableFromEnd = new bool[count_];
			DfsForTo(startIndex, reachableFromStart);
			DfsForFrom(endIndex, reachableFromEnd);

			bool[] enableds = new bool[count_];
			for (int i = 0; i < count_; i++) {
				enableds[i] = reachableFromStart[i] & reachableFromEnd[i];
			}

			bool existsNegativeCycle = false;
			for (int i = 0; i < count_; i++) {
				bool changes = false;
				foreach (var edge in edges_.AsSpan()) {
					if (enableds[edge.From] == false || enableds[edge.To] == false) {
						continue;
					}

					if (distances[edge.From] != INFINITY) {
						long newDistance = distances[edge.From] + edge.Cost;
						if (newDistance < distances[edge.To]) {
							changes = true;
							distances[edge.To] = newDistance;
						}
					}
				}

				if (i == count_ - 1) {
					existsNegativeCycle = changes;
				}

				if (changes == false) {
					break;
				}
			}

			return (distances[endIndex], existsNegativeCycle);
		}

		private void DfsForTo(int index, bool[] reachableFromStart)
		{
			if (reachableFromStart[index]) {
				return;
			}

			reachableFromStart[index] = true;
			foreach (int next in to_[index].AsSpan()) {
				DfsForTo(next, reachableFromStart);
			}
		}

		private void DfsForFrom(int index, bool[] reachableFromEnd)
		{
			if (reachableFromEnd[index]) {
				return;
			}

			reachableFromEnd[index] = true;
			foreach (int next in from_[index].AsSpan()) {
				DfsForFrom(next, reachableFromEnd);
			}
		}

		private readonly struct Edge
		{
			public int From { get; }
			public int To { get; }
			public long Cost { get; }
			public Edge(int from, int to, long cost)
			{
				From = from;
				To = to;
				Cost = cost;
			}
		}
	}
}

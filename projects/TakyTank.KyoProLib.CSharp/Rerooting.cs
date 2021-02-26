using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class Rerooting<T> where T : struct
	{
		private readonly int n_;
		private readonly Func<T, T, T> merge_;
		private readonly Func<int, T, T> operateNode_;
		private readonly T unit_;
		private readonly List<int>[] tempEdges_;
		private readonly int[][] edges_;
		private readonly T[][] dp_;
		private int[] reverseIndexes_;

		public Rerooting(
			int n,
			Func<T, T, T> merge,
			Func<int, T, T> operateNode,
			T unit)
		{
			n_ = n;
			merge_ = merge;
			operateNode_ = operateNode;
			unit_ = unit;

			tempEdges_ = new List<int>[n];
			edges_ = new int[n][];
			dp_ = new T[n][];
			for (int i = 0; i < tempEdges_.Length; ++i) {
				tempEdges_[i] = new List<int>();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge((int a, int b) v) => tempEdges_[v.a].Add(v.b);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge(int a, int b) => tempEdges_[a].Add(b);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge2Way((int a, int b) v) => AddEdge2Way(v.a, v.b);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge2Way(int a, int b)
		{
			tempEdges_[a].Add(b);
			tempEdges_[b].Add(a);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Build()
		{
			for (int i = 0; i < tempEdges_.Length; ++i) {
				edges_[i] = tempEdges_[i].ToArray();
				dp_[i] = new T[tempEdges_[i].Count];
			}
		}

		public T[] Search(int root = 0, bool doseOnlyTreeDp = false)
		{
			T[] result = new T[n_];
			result.AsSpan().Fill(unit_);

			TreeDP(root, result);
			if (doseOnlyTreeDp == false) {
				Reroot(root, result);
			}

			return result;
		}

		private void TreeDP(int v, T[] result)
		{
			var parents = new int[n_];
			var indexes = new int[n_];
			reverseIndexes_ = new int[n_];
			var visited = new bool[n_];
			var routeStack = new Stack<int>(n_);
			var tempStack = new Stack<int>(n_);
			routeStack.Push(v);
			tempStack.Push(v);
			visited[v] = true;
			parents[v] = -1;

			while (tempStack.Count > 0) {
				int current = tempStack.Pop();
				for (int i = 0; i < edges_[current].Length; ++i) {
					int next = edges_[current][i];
					if (visited[next]) {
						reverseIndexes_[current] = i;
						continue;
					}

					visited[next] = true;
					parents[next] = current;
					indexes[next] = i;
					routeStack.Push(next);
					tempStack.Push(next);
				}
			}

			while (routeStack.Count > 0) {
				int target = routeStack.Pop();
				T accum = unit_;
				for (int i = 0; i < edges_[target].Length; ++i) {
					int child = edges_[target][i];
					if (child == parents[target]) {
						continue;
					}

					accum = merge_(accum, dp_[target][i]);
				}

				result[target] = operateNode_(target, accum);
				if (parents[target] >= 0) {
					dp_[parents[target]][indexes[target]] = result[target];
				}
			}
		}

		private void Reroot(int root, T[] result)
		{
			int max = 0;
			for (int i = 0; i < n_; ++i) {
				max = Math.Max(max, edges_[i].Length);
			}

			var accumL = new T[max + 1];
			var accumR = new T[max + 1];

			var q = new Queue<BfsInfo>(n_);
			q.Enqueue(new BfsInfo(root, -1));
			while (q.Count > 0) {
				var current = q.Dequeue();
				int count = edges_[current.V].Length;

				accumL[0] = unit_;
				for (int i = 0; i < count; ++i) {
					accumL[i + 1] = merge_(accumL[i], dp_[current.V][i]);
				}

				accumR[count] = unit_;
				for (int i = count - 1; i >= 0; i--) {
					accumR[i] = merge_(accumR[i + 1], dp_[current.V][i]);
				}

				result[current.V] = operateNode_(current.V, accumL[count]);

				for (int i = 0; i < count; ++i) {
					int next = edges_[current.V][i];
					if (next == current.P) {
						continue;
					}

					T reverse = operateNode_(current.V, merge_(accumL[i], accumR[i + 1]));
					dp_[next][reverseIndexes_[next]] = reverse;

					q.Enqueue(new BfsInfo(next, current.V));
				}
			}
		}

		private struct BfsInfo
		{
			public int V { get; set; }
			public int P { get; set; }
			public BfsInfo(int v, int p)
			{
				V = v;
				P = p;
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class Graph
	{
		private readonly int n_;
		private readonly List<(long d, int v)>[] tempEdges_;
		private readonly (long d, int v)[][] edges_;

		public (long d, int v)[][] Edges => edges_;

		public Graph(int n)
		{
			n_ = n;
			tempEdges_ = new List<(long d, int v)>[n];
			for (int i = 0; i < n; i++) {
				tempEdges_[i] = new List<(long d, int v)>();
			}

			edges_ = new (long d, int v)[n][];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge(int u, int v, long d) => tempEdges_[u].Add((d, v));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Build()
		{
			for (int i = 0; i < n_; i++) {
				edges_[i] = tempEdges_[i].ToArray();
			}
		}
	}
}

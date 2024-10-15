using System;
using System.Collections.Generic;

namespace TakyTank.KyoProLib.CSharp
{
	public class LowLink
	{
		private readonly List<int>[] _edges;
		private readonly bool[] _used;
		private readonly int[] _orders;
		/// <summary>頂点vから後退辺を高々1回通って到達可能な頂点のorderのmin</summary>
		private readonly int[] _lows;

		public List<int> Articulations { get; set; } = new List<int>();
		public List<(int u, int v)> Bridges { get; set; } = new List<(int u, int v)>();

		public LowLink(int n)
		{
			_edges = new List<int>[n];
			for (int i = 0; i < n; i++) {
				_edges[i] = new List<int>();
			}

			_used = new bool[n];
			_orders = new int[n];
			_lows = new int[n];
		}

		public void AddEdge2W(int u, int v)
		{
			_edges[u].Add(v);
			_edges[v].Add(u);
		}

		public void Build()
		{
			int k = 0;
			for (int i = 0; i < _edges.Length; i++) {
				if (_used[i] == false) {
					k = Dfs(i, k, -1);
				}
			}

			Articulations.Sort();
			Bridges.Sort();

			int Dfs(int v, int depth, int parent)
			{
				_used[v] = true;
				_orders[v] = depth;
				++depth;
				_lows[v] = _orders[v];

				bool isArticulation = false;
				int childCount = 0;
				foreach (var u in _edges[v]) {
					if (_used[u] == false) {
						++childCount;
						depth = Dfs(u, depth, v);
						_lows[v] = Math.Min(_lows[v], _lows[u]);
						if (parent != -1 && _orders[v] <= _lows[u]) {
							isArticulation = true;
						}

						if (_orders[v] < _lows[u]) {
							Bridges.Add((Math.Min(v, u), Math.Max(v, u)));
						}
					} else if (u != parent) {
						_lows[v] = Math.Min(_lows[v], _orders[u]);
					}
				}

				if (parent == -1 && childCount >= 2) {
					isArticulation = true;
				}

				if (isArticulation) {
					Articulations.Add(v);
				}

				return depth;
			}
		}
	}
}

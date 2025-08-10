using System;
using System.Collections.Generic;

namespace TakyTank.KyoProLib.CSharp
{
	public class LowLink
	{
		private readonly int _n;
		private readonly List<int>[] _edges;
		/// <summary>DFS木における行きがけ順</summary>
		private readonly int[] _orders;
		/// <summary>頂点vから後退辺を高々1回通って到達可能な頂点のorderのmin</summary>
		private readonly int[] _lows;

		public List<int> Articulations { get; set; } = new List<int>();
		public List<(int u, int v)> Bridges { get; set; } = new List<(int u, int v)>();

		public LowLink(int n)
		{
			_n = n;
			_edges = new List<int>[n];
			for (int i = 0; i < n; i++) {
				_edges[i] = new List<int>();
			}

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
			var used = new bool[_n];

			int k = 0;
			for (int i = 0; i < _edges.Length; i++) {
				if (used[i] == false) {
					k = Dfs(i, k, -1);
				}
			}

			Articulations.Sort();
			Bridges.Sort();

			int Dfs(int v, int order, int parent)
			{
				used[v] = true;
				_orders[v] = order;
				++order;
				_lows[v] = _orders[v];

				bool isArticulation = false;
				int childCount = 0;
				foreach (var u in _edges[v]) {
					if (used[u] == false) {
						++childCount;
						order = Dfs(u, order, v);
						_lows[v] = Math.Min(_lows[v], _lows[u]);
						if (IsArticulation(parent, v, u)) {
							isArticulation = true;
						}

						if (IsBridge(v, u)) {
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

				return order;
			}
		}

		/// <summary>
		/// 二重辺連結成分分解を O(V + E) で行う
		/// </summary>
		/// <returns>
		/// componetIndexes -> 元の頂点が何番目の成分に属しているかの配列
		/// graph -> 連結成分を縮約したグラフ
		/// </returns>
		public (int[] componetIndexes, int[][] graph) CreateTowEdgeConnectedComponets()
		{
			var componentIndexes = new int[_n];
			for (int i = 0; i < _n; i++) {
				componentIndexes[i] = -1;
			}

			int index = 0;
			for (int i = 0; i < _n; i++) {
				if (componentIndexes[i] == -1) {
					Dfs(i, -1);
				}
			}

			void Dfs(int v, int p)
			{
				if (p != -1 && IsBridge(p, v) == false) {
					componentIndexes[v] = componentIndexes[p];
				} else {
					// 端を跨いだときに新しいグループを作成する
					componentIndexes[v] = index;
					++index;
				}

				foreach (var u in _edges[v]) {
					if (componentIndexes[u] == -1) {
						Dfs(u, v);
					}
				}
			}

			// ここに来た時点で、二重辺連結成分を縮約したグラフの頂点数がindexに格納されている。
			var tempEdges = new List<int>[index];
			for (int i = 0; i < index; i++) {
				tempEdges[i] = new List<int>();
			}

			foreach (var (u, v) in Bridges) {
				int uu = componentIndexes[u];
				int vv = componentIndexes[v];
				tempEdges[uu].Add(vv);
				tempEdges[vv].Add(uu);
			}

			var graph = new int[index][];
			for (int i = 0; i < index; i++) {
				graph[i] = tempEdges[i].ToArray();
			}

			return (componentIndexes, graph);
		}

		private bool IsBridge(int v, int c)
		{
			// 辺 vc が橋で無いとき、この辺は閉路の一部になる。
			// これは c かその子孫から、v かその祖先へ向かう後退辺があることを意味する。
			// すなわち、low[c]の値がorder[v]以下になる。
			// 端の場合はその逆なので。
			return _orders[v] < _lows[c];
		}

		private bool IsArticulation(int p, int v, int c)
		{
			// 根の場合は別判定(子が2個以上あるかどうか)になる。ここでは根以外のときの判定。
			// 頂点Vが関節点であるとき、頂点Pと頂点Cが分離される。
			// これは、頂点Cやその子孫から後退辺を使っても、Vかその子孫までしか戻って来れないということ。
			return p != -1 && _orders[v] <= _lows[c];
		}
	}
}

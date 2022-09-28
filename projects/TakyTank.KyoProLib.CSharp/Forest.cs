using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class Forest
	{
		private readonly int _n;
		private readonly UnionFindTree _uf;
		private readonly List<(int u, int v)> _tempEdges;

		private List<int>[] _vertexes;
		private List<(int u, int v)>[] _edges;

		public Forest(int n)
		{
			_n = n;
			_uf = new UnionFindTree(n);
			_tempEdges = new List<(int u, int v)>(n);
		}

		public void AddEdge2W(int u, int v)
		{
			_tempEdges.Add((u, v));
			_uf.Unite(u, v);
		}

		public void Build()
		{
			var groups = new Dictionary<int, List<int>>();
			for (int i = 0; i < _n; i++) {
				int p = _uf.Find(i);
				if (groups.ContainsKey(p) == false) {
					groups[p] = new List<int>();
				}

				groups[p].Add(i);
			}

			var parents = groups.Keys.ToArray();
			int k = parents.Length;
			_vertexes = new List<int>[k];
			_edges = new List<(int u, int v)>[k];
			for (int i = 0; i < k; i++) {
				_vertexes[i] = new List<int>();
				_edges[i] = new List<(int u, int v)>();
			}

			var map = new Dictionary<int, int>();
			for (int i = 0; i < k; i++) {
				map[parents[i]] = i;
			}

			for (int i = 0; i < _n; i++) {
				int p = _uf.Find(i);
				_vertexes[map[p]].Add(i);
			}

			foreach (var edge in _tempEdges) {
				int p = _uf.Find(edge.u);
				_edges[map[p]].Add(edge);
			}
		}

		public IEnumerable<(List<int> vertex, List<(int u, int v)> edge)> ForEachTree()
		{
			int k = _vertexes.Length;
			for (int i = 0; i < k; i++) {
				yield return (_vertexes[i], _edges[i]);
			}
		}
	}
}

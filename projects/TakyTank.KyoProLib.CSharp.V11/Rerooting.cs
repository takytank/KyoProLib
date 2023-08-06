using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V11
{
	public class Rerooting<T> where T : struct
	{
		private readonly int _n;
		private readonly Func<T, T, T> _mergeChild;
		private readonly Func<int, T, T> _calculateVertex;
		private readonly T _unit;
		private readonly List<int>[] _tempEdges;
		private readonly int[][] _edges;
		private readonly T[][] _dp;
		private int[] _reverseIndexes;

		public Rerooting(
			int n,
			Func<T, T, T> mergeChild,
			Func<int, T, T> calculateVertex,
			T unit)
		{
			_n = n;
			_mergeChild = mergeChild;
			_calculateVertex = calculateVertex;
			_unit = unit;

			_tempEdges = new List<int>[n];
			_edges = new int[n][];
			_dp = new T[n][];
			for (int i = 0; i < _tempEdges.Length; ++i) {
				_tempEdges[i] = new List<int>();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge((int a, int b) v) => _tempEdges[v.a].Add(v.b);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge(int a, int b) => _tempEdges[a].Add(b);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge2Way((int a, int b) v) => AddEdge2Way(v.a, v.b);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge2Way(int a, int b)
		{
			_tempEdges[a].Add(b);
			_tempEdges[b].Add(a);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Build()
		{
			for (int i = 0; i < _tempEdges.Length; ++i) {
				_edges[i] = _tempEdges[i].ToArray();
				_dp[i] = new T[_tempEdges[i].Count];
			}
		}

		public T[] Search(int root = 0, bool doseOnlyTreeDp = false)
		{
			T[] result = new T[_n];
			result.AsSpan().Fill(_unit);

			TreeDP(root, result);
			if (doseOnlyTreeDp == false) {
				Reroot(root, result);
			}

			return result;
		}

		private void TreeDP(int v, T[] result)
		{
			var parents = new int[_n];
			var indexes = new int[_n];
			_reverseIndexes = new int[_n];
			var visited = new bool[_n];
			var routeStack = new Stack<int>(_n);
			var tempStack = new Stack<int>(_n);
			routeStack.Push(v);
			tempStack.Push(v);
			visited[v] = true;
			parents[v] = -1;

			while (tempStack.Count > 0) {
				int current = tempStack.Pop();
				for (int i = 0; i < _edges[current].Length; ++i) {
					int next = _edges[current][i];
					if (visited[next]) {
						_reverseIndexes[current] = i;
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
				T accum = _unit;
				for (int i = 0; i < _edges[target].Length; ++i) {
					int child = _edges[target][i];
					if (child == parents[target]) {
						continue;
					}

					accum = _mergeChild(accum, _dp[target][i]);
				}

				result[target] = _calculateVertex(target, accum);
				if (parents[target] >= 0) {
					_dp[parents[target]][indexes[target]] = result[target];
				}
			}
		}

		private void Reroot(int root, T[] result)
		{
			int max = 0;
			for (int i = 0; i < _n; ++i) {
				max = Math.Max(max, _edges[i].Length);
			}

			var accumL = new T[max + 1];
			var accumR = new T[max + 1];

			var q = new Queue<BfsInfo>(_n);
			q.Enqueue(new BfsInfo(root, -1));
			while (q.Count > 0) {
				var current = q.Dequeue();
				int count = _edges[current.V].Length;

				accumL[0] = _unit;
				for (int i = 0; i < count; ++i) {
					accumL[i + 1] = _mergeChild(accumL[i], _dp[current.V][i]);
				}

				accumR[count] = _unit;
				for (int i = count - 1; i >= 0; i--) {
					accumR[i] = _mergeChild(accumR[i + 1], _dp[current.V][i]);
				}

				result[current.V] = _calculateVertex(current.V, accumL[count]);

				for (int i = 0; i < count; ++i) {
					int next = _edges[current.V][i];
					if (next == current.P) {
						continue;
					}

					T reverse = _calculateVertex(current.V, _mergeChild(accumL[i], accumR[i + 1]));
					_dp[next][_reverseIndexes[next]] = reverse;

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

	public class Rerooting<T, TEdge>
		where T : struct
		where TEdge : struct
	{
		private readonly int _n;
		private readonly Func<T, T, T> _mergeChild;
		private readonly Func<T, TEdge, T> _foldEdge;
		private readonly Func<int, T, T> _calculateVertex;
		private readonly T _unit;
		private readonly List<(int v, TEdge info)>[] _tempEdges;
		private readonly (int v, TEdge info)[][] _edges;
		private readonly T[][] _dp;
		private int[] _reverseIndexes;

		public Rerooting(
			int n,
			Func<T, T, T> mergeChild,
			Func<T, TEdge, T> foldEdge,
			Func<int, T, T> calculateVertex,
			T unit)
		{
			_n = n;
			_mergeChild = mergeChild;
			_foldEdge = foldEdge;
			_calculateVertex = calculateVertex;
			_unit = unit;

			_tempEdges = new List<(int v, TEdge info)>[n];
			_edges = new (int v, TEdge info)[n][];
			_dp = new T[n][];
			for (int i = 0; i < _tempEdges.Length; ++i) {
				_tempEdges[i] = new List<(int v, TEdge info)>();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge((int a, int b, TEdge info) v) => _tempEdges[v.a].Add((v.b, v.info));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge(int a, int b, TEdge info) => _tempEdges[a].Add((b, info));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge2Way((int a, int b, TEdge info) v) => AddEdge2Way(v.a, v.b, v.info);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge2Way(int a, int b, TEdge info)
		{
			_tempEdges[a].Add((b, info));
			_tempEdges[b].Add((a, info));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Build()
		{
			for (int i = 0; i < _tempEdges.Length; ++i) {
				_edges[i] = _tempEdges[i].ToArray();
				_dp[i] = new T[_tempEdges[i].Count];
			}
		}

		public T[] Search(int root = 0, bool doseOnlyTreeDp = false)
		{
			T[] result = new T[_n];
			result.AsSpan().Fill(_unit);

			TreeDP(root, result);
			if (doseOnlyTreeDp == false) {
				Reroot(root, result);
			}

			return result;
		}

		private void TreeDP(int v, T[] result)
		{
			var parents = new int[_n];
			var indexes = new int[_n];
			_reverseIndexes = new int[_n];
			var visited = new bool[_n];
			var routeStack = new Stack<int>(_n);
			var tempStack = new Stack<int>(_n);
			routeStack.Push(v);
			tempStack.Push(v);
			visited[v] = true;
			parents[v] = -1;

			while (tempStack.Count > 0) {
				int current = tempStack.Pop();
				for (int i = 0; i < _edges[current].Length; ++i) {
					var (next, _) = _edges[current][i];
					if (visited[next]) {
						_reverseIndexes[current] = i;
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
				T accum = _unit;
				for (int i = 0; i < _edges[target].Length; ++i) {
					var (child, childInfo) = _edges[target][i];
					if (child == parents[target]) {
						continue;
					}

					accum = _mergeChild(accum, _foldEdge(_dp[target][i], childInfo));
				}

				result[target] = _calculateVertex(target, accum);
				if (parents[target] >= 0) {
					_dp[parents[target]][indexes[target]] = result[target];
				}
			}
		}

		private void Reroot(int root, T[] result)
		{
			int max = 0;
			for (int i = 0; i < _n; ++i) {
				max = Math.Max(max, _edges[i].Length);
			}

			var accumL = new T[max + 1];
			var accumR = new T[max + 1];

			var q = new Queue<BfsInfo>(_n);
			q.Enqueue(new BfsInfo(root, -1));
			while (q.Count > 0) {
				var current = q.Dequeue();
				int count = _edges[current.V].Length;

				accumL[0] = _unit;
				for (int i = 0; i < count; ++i) {
					accumL[i + 1] = _mergeChild(accumL[i], _foldEdge(_dp[current.V][i], _edges[current.V][i].info));
				}

				accumR[count] = _unit;
				for (int i = count - 1; i >= 0; i--) {
					accumR[i] = _mergeChild(accumR[i + 1], _foldEdge(_dp[current.V][i], _edges[current.V][i].info));
				}

				result[current.V] = _calculateVertex(current.V, accumL[count]);

				for (int i = 0; i < count; ++i) {
					var (next, nextInfo) = _edges[current.V][i];
					if (next == current.P) {
						continue;
					}

					T reverse = _calculateVertex(current.V, _mergeChild(accumL[i], accumR[i + 1]));
					_dp[next][_reverseIndexes[next]] = reverse;

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

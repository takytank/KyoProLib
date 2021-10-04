using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class SegmentTree2D<TStructure, TData, TUpdate>
	{
		private const int ROOT = 1;

		private readonly Func<int, TStructure> _newStructure;
		private readonly Func<TData, TData, TData> _operate;
		private readonly Func<TStructure, int, int, TData> _query;
		private readonly Action<TStructure, int, TUpdate> _update;
		private readonly Action<TStructure, int, TData> _propagate;
		private readonly TData _unity;
		private readonly Dictionary<int, HashSet<int>> _tempPoints;

		private int _n;
		private int[] _pointsX;
		private int[] _pointsY;
		private int[][] _fcL; //FractionalCascading
		private int[][] _fcR;
		private TStructure[] _seg;

		public SegmentTree2D(
			Func<int, TStructure> newStructure,
			Func<TData, TData, TData> operate,
			Func<TStructure, int, int, TData> query,
			Action<TStructure, int, TUpdate> update,
			Action<TStructure, int, TData> propagate,
			TData unity)
		{
			_newStructure = newStructure;
			_operate = operate;
			_query = query;
			_update = update;
			_propagate = propagate;
			_unity = unity;
			_tempPoints = new Dictionary<int, HashSet<int>>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddPoint(int x, int y)
		{
			if (_tempPoints.ContainsKey(x) == false) {
				_tempPoints[x] = new HashSet<int>();
			}

			_tempPoints[x].Add(y);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Build()
		{
			_pointsX = _tempPoints.Keys.ToArray();
			Array.Sort(_pointsX);

			_n = 1;
			while (_n < _pointsX.Length) {
				_n <<= 1;
			}

			int size = _n << 1;
			_fcL = new int[size][];
			_fcR = new int[size][];
			var pointTree = new int[size][];
			for (int k = 0; k < _n; k++) {
				if (k < _pointsX.Length) {
					int x = _pointsX[k];
					var arr = _tempPoints[x].ToArray();
					Array.Sort(arr);
					pointTree[_n + k] = arr;
				} else {
					pointTree[_n + k] = new int[0];
				}
			}

			_tempPoints.Clear();

			for (int k = _n - 1; k > 0; k--) {
				int lc = k << 1;
				int rc = (k << 1) + 1;

				Merge(pointTree[lc], pointTree[rc], ref pointTree[k]);

				_fcL[k] = new int[pointTree[k].Length + 1];
				_fcR[k] = new int[pointTree[k].Length + 1];
				int tail1 = 0;
				int tail2 = 0;
				for (int i = 0; i < pointTree[k].Length; i++) {
					while (tail1 < pointTree[lc].Length && pointTree[lc][tail1] < pointTree[k][i]) {
						++tail1;
					}

					while (tail2 < pointTree[rc].Length && pointTree[rc][tail2] < pointTree[k][i]) {
						++tail2;
					}

					_fcL[k][i] = tail1;
					_fcR[k][i] = tail2;
				}

				_fcL[k][pointTree[k].Length] = pointTree[lc].Length;
				_fcR[k][pointTree[k].Length] = pointTree[rc].Length;
			}

			_pointsY = pointTree[ROOT];

			_seg = new TStructure[size];
			for (int k = 1; k < pointTree.Length; k++) {
				_seg[k] = _newStructure(pointTree[k].Length);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Update(int x, int y, TUpdate value)
		{
			int x1 = LowerBound(_pointsX, x);
			int y1 = LowerBound(_pointsY, y);
			int y2 = LowerBound(_pointsY, y + 1);
			Update(x1, y1, y2, value, ROOT, 0, _n);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Update(int x, int y1, int y2, TUpdate value, int v, int l, int r)
		{
			if (r <= x || x + 1 <= l) {
				return;
			} else if (x <= l && r <= x + 1) {
				_update(_seg[v], y1, value);
				return;
			}

			int lc = v << 1;
			int rc = (v << 1) + 1;
			Update(x, _fcL[v][y1], _fcL[v][y2], value, lc, l, (l + r) >> 1);
			Update(x, _fcR[v][y1], _fcR[v][y2], value, rc, (l + r) >> 1, r);

			var left = _query(_seg[lc], _fcL[v][y1], _fcL[v][y2]);
			var right = _query(_seg[rc], _fcR[v][y1], _fcR[v][y2]);
			_propagate(_seg[v], y1, _operate(left, right));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TData Query(int x1, int x2, int y1, int y2)
		{
			int xx1 = LowerBound(_pointsX, x1);
			int xx2 = LowerBound(_pointsX, x2);
			int yy1 = LowerBound(_pointsY, y1);
			int yy2 = LowerBound(_pointsY, y2);
			return Query(xx1, xx2, yy1, yy2, ROOT, 0, _n);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private TData Query(int x1, int x2, int y1, int y2, int v, int l, int r)
		{
			if (x1 >= r || x2 <= l) {
				return _unity;
			} else if (x1 <= l && r <= x2) {
				return _query(_seg[v], y1, y2);
			}

			return _operate(
				Query(x1, x2, _fcL[v][y1], _fcL[v][y2], v << 1, l, (l + r) >> 1),
				Query(x1, x2, _fcR[v][y1], _fcR[v][y2], (v << 1) + 1, (l + r) >> 1, r));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int LowerBound(int[] array, int value)
		{
			int ng = -1;
			int ok = array.Length;
			while (ok - ng > 1) {
				int mid = (ok + ng) / 2;
				if (array[mid] >= value) {
					ok = mid;
				} else {
					ng = mid;
				}
			}

			return ok;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Merge(int[] first, int[] second, ref int[] target)
		{
			int p = 0;
			int q = 0;
			var temp = new List<int>();
			int last = int.MinValue;
			while (p < first.Length || q < second.Length) {
				if (p == first.Length) {
					if (last != second[q]) {
						temp.Add(second[q]);
						last = second[q];
					}
					q++;
					continue;
				}

				if (q == second.Length) {
					if (last != first[p]) {
						temp.Add(first[p]);
						last = first[p];
					}
					p++;
					continue;
				}

				if (first[p].CompareTo(second[q]) < 0) {
					if (last != first[p]) {
						temp.Add(first[p]);
						last = first[p];
					}
					p++;
				} else {
					if (last != second[q]) {
						temp.Add(second[q]);
						last = second[q];
					}
					q++;
				}
			}

			target = temp.ToArray();
		}
	}
}

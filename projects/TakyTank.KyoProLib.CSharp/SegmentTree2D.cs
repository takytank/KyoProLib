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

		private readonly int _n;
		private readonly List<TStructure> _seg;
		private readonly Func<int, TStructure> _newStructure;
		private readonly Func<TData, TData, TData> _operate;
		private readonly Func<TStructure, int, int, TData> _query;
		private readonly Action<TStructure, int, TUpdate> _update;
		private readonly Action<TStructure, int, TData> _propagate;
		private readonly TData _unity;
		private readonly int[][] _fcL; //FractionalCascading
		private readonly int[][] _fcR;
		private readonly List<int>[] _points;

		public SegmentTree2D(
			int n,
			Func<int, TStructure> newStructure,
			Func<TData, TData, TData> operate,
			Func<TStructure, int, int, TData> query,
			Action<TStructure, int, TUpdate> update,
			Action<TStructure, int, TData> propagate,
			TData unity)
		{
			_seg = new List<TStructure>();
			_newStructure = newStructure;
			_operate = operate;
			_query = query;
			_update = update;
			_propagate = propagate;
			_unity = unity;
			_n = 1;
			while (_n < n) {
				_n <<= 1;
			}

			int size = _n << 1;
			_points = new List<int>[size];
			_fcL = new int[size][];
			_fcR = new int[size][];
			for (int i = 0; i < size; i++) {
				_points[i] = new List<int>();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddPoint(int x, int y)
		{
			_points[x + _n].Add(y);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Build()
		{
			for (int k = _points.Length - 1; k >= _n; k--) {
				_points[k].Sort();
				_points[k] = _points[k].Distinct().ToList();
			}

			for (int k = _n - 1; k > 0; k--) {
				int lc = k << 1;
				int rc = (k << 1) + 1;

				Merge(_points[lc], _points[rc], _points[k]);
				_points[k] = _points[k].Distinct().ToList();

				_fcL[k] = new int[_points[k].Count + 1];
				_fcR[k] = new int[_points[k].Count + 1];
				int tail1 = 0;
				int tail2 = 0;
				for (int i = 0; i < _points[k].Count; i++) {
					while (tail1 < _points[lc].Count && _points[lc][tail1] < _points[k][i]) {
						++tail1;
					}

					while (tail2 < _points[rc].Count && _points[rc][tail2] < _points[k][i]) {
						++tail2;
					}

					_fcL[k][i] = tail1;
					_fcR[k][i] = tail2;
				}

				_fcL[k][_points[k].Count] = _points[lc].Count;
				_fcR[k][_points[k].Count] = _points[rc].Count;
			}

			for (int k = 0; k < _points.Length; k++) {
				_seg.Add(_newStructure(_points[k].Count));
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Update(int x, int y, TUpdate value)
		{
			int y1 = LowerBound(_points[ROOT], y);
			int y2 = LowerBound(_points[ROOT], y + 1);
			Update(x, y1, y2, value, ROOT, 0, _n);
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
			y1 = LowerBound(_points[ROOT], y1);
			y2 = LowerBound(_points[ROOT], y2);
			return Query(x1, x2, y1, y2, ROOT, 0, _n);
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
		private int LowerBound(List<int> array, int value)
		{
			int ng = -1;
			int ok = array.Count;
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
		private void Merge(List<int> first, List<int> second, List<int> target)
		{
			int p = 0;
			int q = 0;
			while (p < first.Count || q < second.Count) {
				if (p == first.Count) {
					target.Add(second[q]);
					q++;
					continue;
				}

				if (q == second.Count) {
					target.Add(first[p]);
					p++;
					continue;
				}

				if (first[p].CompareTo(second[q]) < 0) {
					target.Add(first[p]);
					p++;
				} else {
					target.Add(second[q]);
					q++;
				}
			}
		}
	}
}

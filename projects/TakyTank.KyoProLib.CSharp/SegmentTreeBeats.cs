using System;
using System.Collections.Generic;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class SegmentTreeBeats
	{
		private const long INF = long.MaxValue;

		private readonly int _n;
		private readonly long[] _max, _max2nd, _maxCount;
		private readonly long[] _min, _min2nd, _minCount;
		private readonly long[] _sum;
		private readonly long[] _length, _lazyAdd, _lazyUpdate;

		public int Count { get; }

		public SegmentTreeBeats(int n) : this(n, null) { }
		public SegmentTreeBeats(long[] a) : this(a.Length, a) { }
		private SegmentTreeBeats(int count, long[] a)
		{
			Count = count;
			_n = 1;
			while (_n < Count) {
				_n <<= 1;
			}

			int size = _n << 1;
			_max = new long[size];
			_max2nd = new long[size];
			_maxCount = new long[size];
			_min = new long[size];
			_min2nd = new long[size];
			_minCount = new long[size];
			_sum = new long[size];
			_length = new long[size];
			_lazyAdd = new long[size];
			_lazyUpdate = new long[size];

			for (int i = 0; i < size; ++i) {
				_lazyAdd[i] = 0;
				_lazyUpdate[i] = INF;
			}

			_length[1] = _n;
			for (int i = 1; i < _n; ++i) {
				_length[i << 1] = _length[(i << 1) | 1] = _length[i] >> 1;
			}

			for (int i = 0; i < Count; ++i) {
				_max[_n + i] = _min[_n + i] = _sum[_n + i] = a != null ? a[i] : 0;
				_max2nd[_n + i] = -INF;
				_min2nd[_n + i] = INF;
				_maxCount[_n + i] = _minCount[_n + i] = 1;
			}

			for (int i = Count; i < _n; ++i) {
				_max[_n + i] = _max2nd[_n + i] = -INF;
				_min[_n + i] = _min2nd[_n + i] = INF;
				_maxCount[_n + i] = _minCount[_n + i] = 0;
			}

			for (int i = _n - 1; i >= 0; i--) {
				Update(i);
			}
		}

		public void UpdateMin(int left, int right, long x) => UpdateMinCore(left, right, x, 1, 0, _n);
		private void UpdateMinCore(int left, int right, long x, int v, int vl, int vr)
		{
			if (right <= vl || vr <= left || _max[v] <= x) {
				return;
			}

			if (left <= vl && vr <= right && _max2nd[v] < x) {
				UpdateNodeMax(v, x);
				return;
			}

			Push(v);
			int vc = (vl + vr) >> 1;
			UpdateMinCore(left, right, x, v << 1, vl, vc);
			UpdateMinCore(left, right, x, (v << 1) | 1, vc, vr);
			Update(v);
		}

		public void UpdateMax(int left, int right, long x) => UdpateMaxCore(left, right, x, 1, 0, _n);
		private void UdpateMaxCore(int left, int right, long x, int v, int vl, int vr)
		{
			if (right <= vl || vr <= left || x <= _min[v]) {
				return;
			}
			if (left <= vl && vr <= right && x < _min2nd[v]) {
				UpdateNodeMin(v, x);
				return;
			}

			Push(v);
			int vc = (vl + vr) >> 1;
			UdpateMaxCore(left, right, x, v << 1, vl, vc);
			UdpateMaxCore(left, right, x, (v << 1) | 1, vc, vr);
			Update(v);
		}

		public void AddValue(int left, int right, long x) => AddValueCore(left, right, x, 1, 0, _n);
		private void AddValueCore(int left, int right, long x, int v, int vl, int vr)
		{
			if (right <= vl || vr <= left) {
				return;
			}

			if (left <= vl && vr <= right) {
				AddAll(v, x);
				return;
			}

			Push(v);
			int vc = (vl + vr) >> 1;
			AddValueCore(left, right, x, v << 1, vl, vc);
			AddValueCore(left, right, x, (v << 1) | 1, vc, vr);
			Update(v);
		}

		public void UpdateValue(int left, int right, long x) => UpdateValueCore(left, right, x, 1, 0, _n);
		private void UpdateValueCore(int left, int right, long x, int v, int vl, int vr)
		{
			if (right <= vl || vr <= left) {
				return;
			}

			if (left <= vl && vr <= right) {
				UpdateAll(v, x);
				return;
			}

			Push(v);
			int vc = (vl + vr) >> 1;
			UpdateValueCore(left, right, x, v << 1, vl, vc);
			UpdateValueCore(left, right, x, (v << 1) | 1, vc, vr);
			Update(v);
		}

		public long QueryMax(int left, int right) => QueryMaxCore(left, right, 1, 0, _n);
		private long QueryMaxCore(int left, int right, int v, int vl, int vr)
		{
			if (right <= vl || vr <= left) {
				return -INF;
			}

			if (left <= vl && vr <= right) {
				return _max[v];
			}

			Push(v);
			int vc = (vl + vr) >> 1;
			long lv = QueryMaxCore(left, right, v << 1, vl, vc);
			long rv = QueryMaxCore(left, right, (v << 1) | 1, vc, vr);
			return Math.Max(lv, rv);
		}

		public long QueryMin(int left, int right) => QueryMinCore(left, right, 1, 0, _n);
		private long QueryMinCore(int left, int right, int v, int vl, int vr)
		{
			if (right <= vl || vr <= left) {
				return INF;
			}

			if (left <= vl && vr <= right) {
				return _min[v];
			}

			Push(v);
			int vc = (vl + vr) >> 1;
			long lv = QueryMinCore(left, right, v << 1, vl, vc);
			long rv = QueryMinCore(left, right, (v << 1) | 1, vc, vr);
			return Math.Min(lv, rv);
		}

		public long QuerySum(int left, int right) => QuerySumCore(left, right, 1, 0, _n);
		private long QuerySumCore(int left, int right, int v, int vl, int vr)
		{
			if (right <= vl || vr <= left) {
				return 0;
			}

			if (left <= vl && vr <= right) {
				return _sum[v];
			}

			Push(v);
			int vc = (vl + vr) >> 1;
			long lv = QuerySumCore(left, right, v << 1, vl, vc);
			long rv = QuerySumCore(left, right, (v << 1) | 1, vc, vr);
			return lv + rv;
		}

		private void UpdateNodeMax(int v, long x)
		{
			_sum[v] += (x - _max[v]) * _maxCount[v];

			if (_max[v] == _min[v]) {
				_max[v] = _min[v] = x;
			} else if (_max[v] == _min2nd[v]) {
				_max[v] = _min2nd[v] = x;
			} else {
				_max[v] = x;
			}

			if (_lazyUpdate[v] != INF && x < _lazyUpdate[v]) {
				_lazyUpdate[v] = x;
			}
		}

		private void UpdateNodeMin(int v, long x)
		{
			_sum[v] += (x - _min[v]) * _minCount[v];

			if (_max[v] == _min[v]) {
				_max[v] = _min[v] = x;
			} else if (_max2nd[v] == _min[v]) {
				_min[v] = _max2nd[v] = x;
			} else {
				_min[v] = x;
			}

			if (_lazyUpdate[v] != INF && _lazyUpdate[v] < x) {
				_lazyUpdate[v] = x;
			}
		}

		private void Push(int v)
		{
			if (_n <= v) {
				return;
			}

			int lc = v << 1;
			int rc = (v << 1) | 1;

			if (_lazyUpdate[v] != INF) {
				UpdateAll(lc, _lazyUpdate[v]);
				UpdateAll(rc, _lazyUpdate[v]);
				_lazyUpdate[v] = INF;
				return;
			}

			if (_lazyAdd[v] != 0) {
				AddAll(lc, _lazyAdd[v]);
				AddAll(rc, _lazyAdd[v]);
				_lazyAdd[v] = 0;
			}

			if (_max[v] < _max[lc]) {
				UpdateNodeMax(lc, _max[v]);
			}
			if (_min[lc] < _min[v]) {
				UpdateNodeMin(lc, _min[v]);
			}

			if (_max[v] < _max[rc]) {
				UpdateNodeMax(rc, _max[v]);
			}
			if (_min[rc] < _min[v]) {
				UpdateNodeMin(rc, _min[v]);
			}
		}

		private void Update(int v)
		{
			int lc = v << 1;
			int rc = (v << 1) | 1;

			_sum[v] = _sum[lc] + _sum[rc];

			if (_max[lc] < _max[rc]) {
				_max[v] = _max[rc];
				_maxCount[v] = _maxCount[rc];
				_max2nd[v] = Math.Max(_max[lc], _max2nd[rc]);
			} else if (_max[lc] > _max[rc]) {
				_max[v] = _max[lc];
				_maxCount[v] = _maxCount[lc];
				_max2nd[v] = Math.Max(_max2nd[lc], _max[rc]);
			} else {
				_max[v] = _max[lc];
				_maxCount[v] = _maxCount[lc] + _maxCount[rc];
				_max2nd[v] = Math.Max(_max2nd[lc], _max2nd[rc]);
			}

			if (_min[lc] < _min[rc]) {
				_min[v] = _min[lc];
				_minCount[v] = _minCount[lc];
				_min2nd[v] = Math.Min(_min2nd[lc], _min[rc]);
			} else if (_min[lc] > _min[rc]) {
				_min[v] = _min[rc];
				_minCount[v] = _minCount[rc];
				_min2nd[v] = Math.Min(_min[lc], _min2nd[rc]);
			} else {
				_min[v] = _min[lc];
				_minCount[v] = _minCount[lc] + _minCount[rc];
				_min2nd[v] = Math.Min(_min2nd[lc], _min2nd[rc]);
			}
		}

		private void AddAll(int v, long x)
		{
			_max[v] += x;
			if (_max2nd[v] != -INF) {
				_max2nd[v] += x;
			}

			_min[v] += x;
			if (_min2nd[v] != INF) {
				_min2nd[v] += x;
			}

			_sum[v] += _length[v] * x;
			if (_lazyUpdate[v] != INF) {
				_lazyUpdate[v] += x;
			} else {
				_lazyAdd[v] += x;
			}
		}

		private void UpdateAll(int v, long x)
		{
			_max[v] = x;
			_max2nd[v] = -INF;
			_min[v] = x;
			_min2nd[v] = INF;
			_maxCount[v] = _minCount[v] = _length[v];

			_sum[v] = x * _length[v];
			_lazyUpdate[v] = x;
			_lazyAdd[v] = 0;
		}
	}
}

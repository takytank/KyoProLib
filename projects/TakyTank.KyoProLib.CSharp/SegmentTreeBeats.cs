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

			_length[0] = _n;
			for (int i = 0; i < _n - 1; ++i) {
				_length[2 * i + 1] = _length[2 * i + 2] = _length[i] >> 1;
			}

			for (int i = 0; i < Count; ++i) {
				_max[_n - 1 + i] = _min[_n - 1 + i] = _sum[_n - 1 + i] = a != null ? a[i] : 0;
				_max2nd[_n - 1 + i] = -INF;
				_min2nd[_n - 1 + i] = INF;
				_maxCount[_n - 1 + i] = _minCount[_n - 1 + i] = 1;
			}

			for (int i = Count; i < _n; ++i) {
				_max[_n - 1 + i] = _max2nd[_n - 1 + i] = -INF;
				_min[_n - 1 + i] = _min2nd[_n - 1 + i] = INF;
				_maxCount[_n - 1 + i] = _minCount[_n - 1 + i] = 0;
			}

			for (int i = _n - 2; i >= 0; i--) {
				Update(i);
			}
		}

		public void UpdateMin(int left, int right, long x) => UpdateMinCore(left, right, x, 0, 0, _n);
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
			UpdateMinCore(left, right, x, 2 * v + 1, vl, (vl + vr) / 2);
			UpdateMinCore(left, right, x, 2 * v + 2, (vl + vr) / 2, vr);
			Update(v);
		}

		public void UpdateMax(int left, int right, long x) => UdpateMaxCore(left, right, x, 0, 0, _n);
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
			UdpateMaxCore(left, right, x, 2 * v + 1, vl, (vl + vr) / 2);
			UdpateMaxCore(left, right, x, 2 * v + 2, (vl + vr) / 2, vr);
			Update(v);
		}

		public void AddValue(int left, int right, long x) => AddValueCore(left, right, x, 0, 0, _n);
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
			AddValueCore(left, right, x, 2 * v + 1, vl, (vl + vr) / 2);
			AddValueCore(left, right, x, 2 * v + 2, (vl + vr) / 2, vr);
			Update(v);
		}

		public void UpdateValue(int left, int right, long x) => UpdateValueCore(left, right, x, 0, 0, _n);
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
			UpdateValueCore(left, right, x, 2 * v + 1, vl, (vl + vr) / 2);
			UpdateValueCore(left, right, x, 2 * v + 2, (vl + vr) / 2, vr);
			Update(v);
		}

		public long QureyMax(int left, int right) => QueryMaxCore(left, right, 0, 0, _n);
		private long QueryMaxCore(int left, int right, int v, int vl, int vr)
		{
			if (right <= vl || vr <= left) {
				return -INF;
			}

			if (left <= vl && vr <= right) {
				return _max[v];
			}

			Push(v);

			long lv = QueryMaxCore(left, right, 2 * v + 1, vl, (vl + vr) / 2);
			long rv = QueryMaxCore(left, right, 2 * v + 2, (vl + vr) / 2, vr);
			return Math.Max(lv, rv);
		}

		public long QueryMin(int left, int right) => QueryMinCore(left, right, 0, 0, _n);
		private long QueryMinCore(int left, int right, int v, int vl, int vr)
		{
			if (right <= vl || vr <= left) {
				return INF;
			}

			if (left <= vl && vr <= right) {
				return _min[v];
			}

			Push(v);

			long lv = QueryMinCore(left, right, 2 * v + 1, vl, (vl + vr) / 2);
			long rv = QueryMinCore(left, right, 2 * v + 2, (vl + vr) / 2, vr);
			return Math.Min(lv, rv);
		}

		public long QuerySum(int left, int right) => QuerySumCore(left, right, 0, 0, _n);
		private long QuerySumCore(int left, int right, int v, int vl, int vr)
		{
			if (right <= vl || vr <= left) {
				return 0;
			}

			if (left <= vl && vr <= right) {
				return _sum[v];
			}

			Push(v);

			long lv = QuerySumCore(left, right, 2 * v + 1, vl, (vl + vr) / 2);
			long rv = QuerySumCore(left, right, 2 * v + 2, (vl + vr) / 2, vr);
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
			if (_n - 1 <= v) {
				return;
			}

			if (_lazyUpdate[v] != INF) {
				UpdateAll(2 * v + 1, _lazyUpdate[v]);
				UpdateAll(2 * v + 2, _lazyUpdate[v]);
				_lazyUpdate[v] = INF;
				return;
			}

			if (_lazyAdd[v] != 0) {
				AddAll(2 * v + 1, _lazyAdd[v]);
				AddAll(2 * v + 2, _lazyAdd[v]);
				_lazyAdd[v] = 0;
			}

			if (_max[v] < _max[2 * v + 1]) {
				UpdateNodeMax(2 * v + 1, _max[v]);
			}
			if (_min[2 * v + 1] < _min[v]) {
				UpdateNodeMin(2 * v + 1, _min[v]);
			}

			if (_max[v] < _max[2 * v + 2]) {
				UpdateNodeMax(2 * v + 2, _max[v]);
			}
			if (_min[2 * v + 2] < _min[v]) {
				UpdateNodeMin(2 * v + 2, _min[v]);
			}
		}

		private void Update(int v)
		{
			_sum[v] = _sum[2 * v + 1] + _sum[2 * v + 2];

			if (_max[2 * v + 1] < _max[2 * v + 2]) {
				_max[v] = _max[2 * v + 2];
				_maxCount[v] = _maxCount[2 * v + 2];
				_max2nd[v] = Math.Max(_max[2 * v + 1], _max2nd[2 * v + 2]);
			} else if (_max[2 * v + 1] > _max[2 * v + 2]) {
				_max[v] = _max[2 * v + 1];
				_maxCount[v] = _maxCount[2 * v + 1];
				_max2nd[v] = Math.Max(_max2nd[2 * v + 1], _max[2 * v + 2]);
			} else {
				_max[v] = _max[2 * v + 1];
				_maxCount[v] = _maxCount[2 * v + 1] + _maxCount[2 * v + 2];
				_max2nd[v] = Math.Max(_max2nd[2 * v + 1], _max2nd[2 * v + 2]);
			}

			if (_min[2 * v + 1] < _min[2 * v + 2]) {
				_min[v] = _min[2 * v + 1];
				_minCount[v] = _minCount[2 * v + 1];
				_min2nd[v] = Math.Min(_min2nd[2 * v + 1], _min[2 * v + 2]);
			} else if (_min[2 * v + 1] > _min[2 * v + 2]) {
				_min[v] = _min[2 * v + 2];
				_minCount[v] = _minCount[2 * v + 2];
				_min2nd[v] = Math.Min(_min[2 * v + 1], _min2nd[2 * v + 2]);
			} else {
				_min[v] = _min[2 * v + 1];
				_minCount[v] = _minCount[2 * v + 1] + _minCount[2 * v + 2];
				_min2nd[v] = Math.Min(_min2nd[2 * v + 1], _min2nd[2 * v + 2]);
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

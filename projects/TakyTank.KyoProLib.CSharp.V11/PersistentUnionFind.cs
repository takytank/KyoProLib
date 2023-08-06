using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V11
{
	public class PartialPersistentUnionFind
	{
		private readonly (int time, int p)[] _parents;
		private readonly List<(int time, int size)>[] _sizes;
		private int _last;

		public PartialPersistentUnionFind(int count)
		{
			_last = int.MinValue;
			_parents = new (int time, int p)[count];
			_sizes = new List<(int time, int size)>[count];
			for (int i = 0; i < count; ++i) {
				_sizes[i] = new List<(int time, int size)>();
			}

			for (int i = 0; i < count; ++i) {
				_parents[i] = (int.MaxValue, i);
				_sizes[i].Add((int.MinValue, 1));
			}
		}

		public bool IsUnited(int x, int y, int time) => Find(x, time) == Find(y, time);
		public bool Unite(int x, int y, int time)
		{
			System.Diagnostics.Debug.Assert(_last <= time);
			_last = time;

			x = Find(x, time);
			y = Find(y, time);
			if (x == y) {
				return false;
			}

			int sizeX = _sizes[x][^1].size;
			int sizeY = _sizes[y][^1].size;
			if (sizeX < sizeY) {
				(x, y) = (y, x);
			}

			_sizes[x].Add((time, sizeX + sizeY));
			_parents[y] = (time, x);
			return true;
		}

		public int Find(int k, int time)
		{
			while (_parents[k].time <= time) {
				k = _parents[k].p;
			}

			return k;
		}

		public int GetSizeOf(int k, int time)
		{
			k = Find(k, time);
			int ng = -1;
			int ok = _sizes[k].Count;
			while (ok - ng > 1) {
				int mid = (ok + ng) >> 1;
				if (_sizes[k][mid].time > time) {
					ok = mid;
				} else {
					ng = mid;
				}
			}

			--ok;

			return _sizes[k][ok].size;
		}

		public IEnumerable<int> GetAllRoots(int time)
		{
			for (int i = 0; i < _parents.Length; i++) {
				if (_parents[i].time > time) {
					yield return i;
				}
			}
		}
	}

	public class PersistentUnionFind
	{
		public static PersistentUnionFind Create(int count) => new PersistentUnionFind(count);

		private readonly ImmutableList<int> _data;

		public int Count => _data.Count;
		public int GroupCount { get; private set; }

		private PersistentUnionFind(int count)
		{
			var builder = ImmutableList.CreateBuilder<int>();
			for (int i = 0; i < count; ++i) {
				builder.Add(-1);
			}

			_data = builder.ToImmutable();
			GroupCount = count;
		}

		private PersistentUnionFind(ImmutableList<int> current, int groupCount)
		{
			_data = current;
			GroupCount = groupCount;
		}

		public int GetSizeOf(int k) => -_data[Find(k)];

		public bool IsUnited(int x, int y) => Find(x) == Find(y);

		public (bool isUnited, PersistentUnionFind newGeneration) Unite(int x, int y)
		{
			x = Find(x);
			y = Find(y);
			if (x == y) {
				return (false, new PersistentUnionFind(_data, GroupCount));
			}

			int sizeX = _data[x];
			int sizeY = _data[y];
			if (sizeX > sizeY) {
				(x, y) = (y, x);
			}

			var builder = _data.ToBuilder();
			builder[x] = sizeX + sizeY;
			builder[y] = x;
			var newData = builder.ToImmutable();

			return (true, new PersistentUnionFind(newData, GroupCount - 1));
		}

		public int Find(int k)
		{
			while (_data[k] >= 0) {
				k = _data[k];
			}

			return k;
		}

		public IEnumerable<int> GetAllRoots()
		{
			for (int i = 0; i < _data.Count; i++) {
				if (_data[i] < 0) {
					yield return i;
				}
			}
		}
	}
}

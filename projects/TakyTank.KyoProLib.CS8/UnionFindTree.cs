using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TakyTank.KyoProLib.CS8
{
	public class UnionFindTree
	{
		private readonly int[] data_;

		public int Count => data_.Length;
		public int GroupCount { get; private set; }

		public UnionFindTree(int count)
		{
			data_ = new int[count];
			for (int i = 0; i < count; i++) {
				data_[i] = -1;
			}

			GroupCount = count;
		}

		public int GetSizeOf(int k) => -data_[Find(k)];

		public bool IsUnited(int x, int y) => Find(x) == Find(y);
		public bool Unite(int x, int y)
		{
			x = Find(x);
			y = Find(y);
			if (x == y) {
				return false;
			}

			if (data_[x] > data_[y]) {
				(x, y) = (y, x);
			}

			--GroupCount;
			data_[x] += data_[y];
			data_[y] = x;
			return true;
		}

		public int Find(int k)
		{
			while (data_[k] >= 0) {
				if (data_[data_[k]] >= 0) {
					data_[k] = data_[data_[k]];
				}

				k = data_[k];
			}

			return k;
		}

		public IEnumerable<int> GetAllRoots()
		{
			for (int i = 0; i < data_.Length; i++) {
				if (data_[i] < 0) {
					yield return i;
				}
			}
		}
	}

	public class UnionFindTree<T>
	{
		private readonly int[] data_;
		private readonly T[] values_;
		private readonly Func<T, T, T> merge_;

		public int Count => data_.Length;
		public int GroupCount { get; private set; }

		public UnionFindTree(
			int count,
			Func<int, T> init,
			Func<T, T, T> merge)
		{
			data_ = new int[count];
			values_ = new T[count];
			for (int i = 0; i < count; i++) {
				data_[i] = -1;
				values_[i] = init(i);
			}

			GroupCount = count;
			merge_ = merge;
		}

		public int GetSizeOf(int k) => -data_[Find(k)];
		public T GetValueOf(int k) => values_[Find(k)];

		public bool IsUnited(int x, int y) => Find(x) == Find(y);
		public bool Unite(int x, int y)
		{
			x = Find(x);
			y = Find(y);
			if (x == y) {
				return false;
			}

			if (data_[x] > data_[y]) {
				(x, y) = (y, x);
			}

			--GroupCount;
			data_[x] += data_[y];
			values_[x] = merge_(values_[x], values_[y]);
			data_[y] = x;
			return true;
		}

		public int Find(int k)
		{
			while (data_[k] >= 0) {
				if (data_[data_[k]] >= 0) {
					data_[k] = data_[data_[k]];
				}

				k = data_[k];
			}

			return k;
		}

		public IEnumerable<int> GetAllRoots()
		{
			for (int i = 0; i < data_.Length; i++) {
				if (data_[i] < 0) {
					yield return i;
				}
			}
		}
	}

	public class UndoUnionFindTree
	{
		private readonly int[] data_;
		private readonly Stack<(int index, int data)> undoStack_ = new Stack<(int index, int data)>();

		public int Count => data_.Length;
		public int GroupCount { get; private set; }

		public UndoUnionFindTree(int count)
		{
			data_ = new int[count];
			for (int i = 0; i < count; i++) {
				data_[i] = -1;
			}

			GroupCount = count;
		}

		public int GetSizeOf(int k) => -data_[Find(k)];

		public bool IsUnited(int x, int y) => Find(x) == Find(y);
		public bool Unite(int x, int y)
		{
			x = Find(x);
			y = Find(y);
			undoStack_.Push((x, data_[x]));
			undoStack_.Push((y, data_[y]));
			if (x == y) {
				return false;
			}

			if (data_[x] > data_[y]) {
				(x, y) = (y, x);
			}

			--GroupCount;
			data_[x] += data_[y];
			data_[y] = x;
			return true;
		}

		public int Find(int k)
		{
			if (data_[k] < 0) {
				return k;
			}

			return Find(data_[k]);
		}

		public void Undo()
		{
			if (undoStack_.Count < 2) {
				return;
			}

			var (indexX, dataX) = undoStack_.Pop();
			var (indexY, dataY) = undoStack_.Pop();
			data_[indexX] = dataX;
			data_[indexY] = dataY;
			if (indexX != indexY) {
				++GroupCount;
			}
		}
	}
}

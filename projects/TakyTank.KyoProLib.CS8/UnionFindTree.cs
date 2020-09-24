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
				int temp = x;
				x = y;
				y = temp;
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

		public IEnumerable<int> GetAllRoots => data_.Where(x => x < 0);
	}

	public class UndoUnionFindTree
	{
		private readonly int[] data_;
		private readonly Stack<(int index, int value)> undoStack_ = new Stack<(int index, int value)>();

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
				int temp = x;
				x = y;
				y = temp;
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

			var (indexX, valueX) = undoStack_.Pop();
			var (indexY, valueY) = undoStack_.Pop();
			data_[indexX] = valueX;
			data_[indexY] = valueY;
			if (indexX != indexY) {
				++GroupCount;
			}
		}
	}
}

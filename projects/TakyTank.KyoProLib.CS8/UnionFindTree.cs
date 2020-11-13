using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

	public class PotentialUnionFind<T>
	{
		private readonly int[] data_;
		private readonly T[] deltaPotentials_;
		private readonly Func<T, T, T> merge_;
		private readonly Func<T, T> invert_;

		public int Count => data_.Length;
		public int GroupCount { get; private set; }

		public PotentialUnionFind(
			int count,
			T initialPotential,
			Func<T, T, T> merge,
			Func<T, T> invert)
		{
			data_ = new int[count];
			deltaPotentials_ = new T[count];
			for (int i = 0; i < count; i++) {
				data_[i] = -1;
				deltaPotentials_[i] = initialPotential;
			}

			GroupCount = count;
			merge_ = merge;
			invert_ = invert;
		}

		public int GetSizeOf(int v) => -data_[Find(v)];

		public bool IsUnited(int x, int y) => Find(x) == Find(y);
		public bool Unite(int x, int y, T w)
		{
			w = merge_(w, GetPotentialOf(x));
			w = merge_(w, invert_(GetPotentialOf(y)));
			x = Find(x);
			y = Find(y);
			if (x == y) {
				return false;
			}

			if (data_[x] > data_[y]) {
				(x, y) = (y, x);
				w = invert_(w);
			}

			--GroupCount;
			data_[x] += data_[y];
			data_[y] = x;

			deltaPotentials_[y] = w;
			return true;
		}

		public int Find(int v)
		{
			if (data_[v] < 0) {
				return v;
			} else {
				int p = Find(data_[v]);
				deltaPotentials_[v] = merge_(
					deltaPotentials_[v], deltaPotentials_[data_[v]]);
				data_[v] = p;
				return p;
			}
		}

		public T GetPotentialOf(int v)
		{
			Find(v);
			return deltaPotentials_[v];
		}

		public T GetDeltaPotential(int x, int y)
			=> merge_(GetPotentialOf(y), invert_(GetPotentialOf(x)));
	}

	public class UndoUnionFindTree
	{
		private readonly int[] data_;
		private readonly Stack<(int index, int data)> undoStack_ = new Stack<(int index, int data)>();
		private int restoreCount_ = 0;

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
			++restoreCount_;
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

		public void Memory() => restoreCount_ = 0;

		public void Undo()
		{
			UndoCore();
			--restoreCount_;
		}

		public void Restore()
		{
			for (int i = 0; i < restoreCount_; i++) {
				UndoCore();
			}

			restoreCount_ = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void UndoCore()
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

	public class UndoUnionFindTree<T>
	{
		private readonly int[] data_;
		private readonly T[] values_;
		private readonly Func<T, T, T> merge_;
		private readonly Stack<(int index, int data, T value)> undoStack_
			= new Stack<(int index, int data, T value)>();
		private int restoreCount_ = 0;

		public int Count => data_.Length;
		public int GroupCount { get; private set; }

		public UndoUnionFindTree(
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
			undoStack_.Push((x, data_[x], values_[x]));
			undoStack_.Push((y, data_[y], values_[y]));
			++restoreCount_;
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
			if (data_[k] < 0) {
				return k;
			}

			return Find(data_[k]);
		}

		public void Memory() => restoreCount_ = 0;

		public void Undo()
		{
			UndoCore();
			--restoreCount_;
		}

		public void Restore()
		{
			for (int i = 0; i < restoreCount_; i++) {
				UndoCore();
			}

			restoreCount_ = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void UndoCore()
		{
			if (undoStack_.Count < 2) {
				return;
			}

			var (indexX, dataX, valueX) = undoStack_.Pop();
			var (indexY, dataY, valueY) = undoStack_.Pop();
			data_[indexX] = dataX;
			values_[indexX] = valueX;
			data_[indexY] = dataY;
			values_[indexY] = valueY;
			if (indexX != indexY) {
				++GroupCount;
			}
		}
	}
}

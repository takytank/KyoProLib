using System;
using System.Collections.Generic;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public static class NthElement
	{
		public static T Select<T>(T[] array, int n)
			where T : IComparable<T>
		{
			// 閉区間で渡すことに注意
			int index = SelectCore(array, 0, array.Length - 1, n);
			return array[index];
		}

		public static T Select<T>(T[] array, int left, int right, int n)
			where T : IComparable<T>
		{
			// 閉区間で渡すことに注意
			int index = SelectCore(array, left, right - 1, n);
			return array[index];
		}

		private static int SelectCore<T>(T[] array, int left, int right, int n)
			where T : IComparable<T>
		{
			while (left < right) {
				// 区間分割のための pivot を計算。
				int pivotIndex = CalculatePivot(array, left, right);
				// pivot より小さい・等しい・大きいの 3 つの区間に分割。
				// 元の配列の中身がこの順番に並べ替えられる。但し各区間内での大小順は保証されない。
				// 等しい区間の両端のインデックスが返る。
				var (el, er) = Partition(array, left, right, pivotIndex);
				if (n < el) {
					// 中央値は pivot よりも値が小さい区間にある
					right = el - 1;
				} else if (n <= er) {
					// 中央値は pivot と同じ値の区間にある。つまり現在の配列の状態でN番目でよい。
					return n;
				} else {
					// 中央値は pivot よりも値が大きい区間にある
					left = er + 1;
				}
			}

			// 最終的に left == right まで追い込んだ時のインデックスが中央値に対応する。
			return left;
		}

		private static int CalculatePivot<T>(T[] array, int left, int right)
			where T : IComparable<T>
		{
			if (right - left < 5) {
				return CalculateMedianOf5(array, left, right);
			}

			// 長さ5の領域に分割し、各領域で中央値を求める。
			for (int i = left; i <= right; i += 5) {
				int medianIndex = CalculateMedianOf5(array, i, Math.Min(i + 4, right));
				int tag = left + (i - left) / 5;
				// 中央値を配列の前の方に寄せて、その範囲に対してさらに中央値を求める。
				(array[medianIndex], array[tag]) = (array[tag], array[medianIndex]);
			}

			int mid = left + ((right - left) / 10) + 1;
			// 中央値を集めた区間で再度中央値を求める再帰
			return SelectCore(array, left, left + ((right - left) / 5), mid);
		}

		private static (int equalLeft, int equalRight) Partition<T>(
			T[] array, int left, int right, int pivotIndex)
			where T : IComparable<T>
		{
			T pivot = array[pivotIndex];
			// 他の要素の並べ替えで影響が出ないように、pivot に対応する要素を一旦末尾に移動。後で戻す。
			(array[pivotIndex], array[right]) = (array[right], array[pivotIndex]);

			// pivot より小さい要素を配列の前方に移動してまとめる
			int smallIndex = left;
			for (int i = left; i < right; ++i) {
				if (array[i].CompareTo(pivot) < 0) {
					(array[smallIndex], array[i]) = (array[i], array[smallIndex]);
					++smallIndex;
				}
			}

			// pivot と同じ要素を、小さいものの区間の次に移動してまとめる
			int equalIndex = smallIndex;
			for (int i = smallIndex; i < right; ++i) {
				if (array[i].CompareTo(pivot) == 0) {
					(array[equalIndex], array[i]) = (array[i], array[equalIndex]);
					++equalIndex;
				}
			}

			// pivot を同じ値の区間の末尾に戻す
			// これより後ろの要素は pivot よりも大きい。
			(array[right], array[equalIndex]) = (array[equalIndex], array[right]);

			// smallIndex は small 要素を格納後にインクリメントしているので、
			// small 区間の末尾の次を指している。つまり、equal 区間の先頭。
			// equalIndex も同様にインクリメントしているが、
			// 一旦配列の末尾に追いやった pivot をその位置に戻してきているので、equal 区間の末尾。
			return (smallIndex, equalIndex);
		}

		private static int CalculateMedianOf5<T>(T[] array, int left, int right)
			where T : IComparable<T>
		{
			int i = left + 1;
			while (i <= right) {
				int j = i;
				while (j > left && array[j - 1].CompareTo(array[j]) > 0) {
					(array[j], array[j - 1]) = (array[j - 1], array[j]);
					--j;
				}

				i += 2;
			}

			return (left + right) / 2;
		}

		public static TObject Select<TObject, TValue>(
			TObject[] array, Func<TObject, TValue> selector, int n)
			where TValue : IComparable<TValue>
		{
			int index = SelectCore(array, selector, 0, array.Length - 1, n);
			return array[index];
		}

		public static TObject Select<TObject, TValue>(
			TObject[] array, Func<TObject, TValue> selector, int left, int right, int n)
			where TValue : IComparable<TValue>
		{
			int index = SelectCore(array, selector, left, right - 1, n);
			return array[index];
		}


		private static int SelectCore<TObject, TValue>(
			TObject[] array, Func<TObject, TValue> selector, int left, int right, int n)
			where TValue : IComparable<TValue>
		{
			while (left < right) {
				int pivotIndex = CalculatePivot(array, selector, left, right);
				var (el, er) = Partition(array, selector, left, right, pivotIndex);
				if (n < el) {
					right = el - 1;
				} else if (n <= er) {
					return n;
				} else {
					left = er + 1;
				}
			}

			return left;
		}

		private static int CalculatePivot<TObject, TValue>(
			TObject[] array, Func<TObject, TValue> selector, int left, int right)
			where TValue : IComparable<TValue>
		{
			if (right - left < 5) {
				return CalculateMedianOf5(array, selector, left, right);
			}

			for (int i = left; i <= right; i += 5) {
				int medianIndex = CalculateMedianOf5(array, selector, i, Math.Min(i + 4, right));
				int tag = left + (i - left) / 5;
				(array[medianIndex], array[tag]) = (array[tag], array[medianIndex]);
			}

			int mid = left + ((right - left) / 10) + 1;
			return SelectCore(array, selector, left, left + ((right - left) / 5), mid);
		}

		private static (int equalLeft, int equalRight) Partition<TObject, TValue>(
			TObject[] array, Func<TObject, TValue> selector, int left, int right, int pivotIndex)
			where TValue : IComparable<TValue>
		{
			var pivot = array[pivotIndex];
			(array[pivotIndex], array[right]) = (array[right], array[pivotIndex]);

			int smallIndex = left;
			for (int i = left; i < right; ++i) {
				if (selector(array[i]).CompareTo(selector(pivot)) < 0) {
					(array[smallIndex], array[i]) = (array[i], array[smallIndex]);
					++smallIndex;
				}
			}

			int equalIndex = smallIndex;
			for (int i = smallIndex; i < right; ++i) {
				if (selector(array[i]).CompareTo(selector(pivot)) == 0) {
					(array[equalIndex], array[i]) = (array[i], array[equalIndex]);
					++equalIndex;
				}
			}

			(array[right], array[equalIndex]) = (array[equalIndex], array[right]);

			return (smallIndex, equalIndex);
		}

		private static int CalculateMedianOf5<TObject, TValue>(
			TObject[] array, Func<TObject, TValue> selector, int left, int right)
			where TValue : IComparable<TValue>
		{
			int i = left + 1;
			while (i <= right) {
				int j = i;
				while (j > left && selector(array[j - 1]).CompareTo(selector(array[j])) > 0) {
					(array[j], array[j - 1]) = (array[j - 1], array[j]);
					--j;
				}

				i += 2;
			}

			return (left + right) / 2;
		}

		public static int Select(int[] array, int n)
		{
			int index = SelectCore(array, 0, array.Length - 1, n);
			return array[index];
		}

		public static int Select(int[] array, int left, int right, int n)
		{
			int index = SelectCore(array, left, right - 1, n);
			return array[index];
		}

		private static int SelectCore(int[] array, int left, int right, int n)
		{
			while (left < right) {
				int pivotIndex = CalculatePivot(array, left, right);
				var (el, er) = Partition(array, left, right, pivotIndex);
				if (n < el) {
					right = el - 1;
				} else if (n <= er) {
					return n;
				} else {
					left = er + 1;
				}
			}

			return left;
		}

		private static int CalculatePivot(int[] array, int left, int right)
		{
			if (right - left < 5) {
				return CalculateMedianOf5(array, left, right);
			}

			for (int i = left; i <= right; i += 5) {
				int medianIndex = CalculateMedianOf5(array, i, Math.Min(i + 4, right));
				int tag = left + (i - left) / 5;
				(array[medianIndex], array[tag]) = (array[tag], array[medianIndex]);
			}

			int mid = left + ((right - left) / 10) + 1;
			return SelectCore(array, left, left + ((right - left) / 5), mid);
		}

		private static (int equalLeft, int equalRight) Partition(
			int[] array, int left, int right, int pivotIndex)
		{
			int pivot = array[pivotIndex];
			(array[pivotIndex], array[right]) = (array[right], array[pivotIndex]);

			int smallIndex = left;
			for (int i = left; i < right; ++i) {
				if (array[i] < pivot) {
					(array[smallIndex], array[i]) = (array[i], array[smallIndex]);
					++smallIndex;
				}
			}

			int equalIndex = smallIndex;
			for (int i = smallIndex; i < right; ++i) {
				if (array[i] == pivot) {
					(array[equalIndex], array[i]) = (array[i], array[equalIndex]);
					++equalIndex;
				}
			}

			(array[right], array[equalIndex]) = (array[equalIndex], array[right]);

			return (smallIndex, equalIndex);
		}

		private static int CalculateMedianOf5(int[] array, int left, int right)
		{
			int i = left + 1;
			while (i <= right) {
				int j = i;
				while (j > left && array[j - 1] > array[j]) {
					(array[j], array[j - 1]) = (array[j - 1], array[j]);
					--j;
				}

				i += 2;
			}

			return (left + right) / 2;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V11
{
	public static class CartesianTree<T>
		where T : IComparable<T>
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int[] Build(ReadOnlySpan<T> values, T minf)
		{
			var left = LeftNeighbors(values, minf);
			var right = RightNeighbors(values, minf);

			int n = values.Length;
			var tree = new int[n];
			for (int i = 0; i < n; i++) {
				tree[i] = left[i].value.CompareTo(right[i].value) >= 0
					? left[i].index
					: right[i].index;
				if (tree[i] == -1) {
					tree[i] = i;
				}
			}

			return tree;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static (int index, T value)[] RightNeighbors(ReadOnlySpan<T> values, T minf)
		{
			int n = values.Length;
			var neighbors = new (int index, T value)[n];
			for (int i = 0; i < neighbors.Length; i++) {
				neighbors[i] = (-1, minf);
			}

			var stack = new Stack<(int index, T value)>();
			for (int i = 0; i < values.Length; i++) {
				T value = values[i];
				while (stack.Count > 0 && stack.Peek().value.CompareTo(value) >= 0) {
					var (index, _) = stack.Pop();
					neighbors[index] = ((i, value));
				}

				stack.Push((i, value));
			}

			return neighbors;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static (int index, T value)[] LeftNeighbors(ReadOnlySpan<T> values, T minf)
		{
			int n = values.Length;
			var neighbors = new (int index, T value)[n];
			for (int i = 0; i < neighbors.Length; i++) {
				neighbors[i] = (-1, minf);
			}

			var stack = new Stack<(int index, T value)>();
			for (int i = n - 1; i >= 0; i--) {
				T value = values[i];
				while (stack.Count > 0 && stack.Peek().value.CompareTo(value) >= 0) {
					var (index, _) = stack.Pop();
					neighbors[index] = ((i, value));
				}

				stack.Push((i, value));
			}

			return neighbors;
		}
	}
}

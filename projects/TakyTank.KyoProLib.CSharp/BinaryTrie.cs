using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class BinaryTrie
	{
		private readonly int _bitLength;
		private Node _root;

		public int Count => _root != null ? _root.Count : 0; 
		public ulong Max => FindXorMin(_root, ~0UL, _bitLength - 1);
		public ulong Min => FindXorMin(_root, 0UL, _bitLength - 1);

		public ulong this[int index]
		{
			get
			{
				System.Diagnostics.Debug.Assert(0 <= index && index < Count);
				return Get(_root, index, _bitLength - 1);
			}
		}

		public ulong this[int index, ulong x]
		{
			get
			{
				System.Diagnostics.Debug.Assert(0 <= index && index < Count);
				return GetXor(_root, index, x, _bitLength - 1);
			}
		}

		public BinaryTrie(int bitLength)
		{
			_bitLength = bitLength;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int CountOf(ulong value)
		{
			if (_root == null) {
				return 0;
			}

			Node node = _root;
			for (int i = _bitLength - 1; i >= 0; i--) {
				node = node.Child[(value >> i) & 1];
				if (node == null) {
					return 0;
				}
			}

			return node.Count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(ulong value) => Add(_root, value, _bitLength - 1);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Node Add(Node node, ulong value, int bit)
		{
			if (node == null) {
				node = new Node();
			}

			node.Count += 1;

			if (bit < 0) {
				return node;
			}

			ulong flag = (value >> bit) & 1;
			node.Child[flag] = Add(node.Child[flag], value, bit - 1);
			return node;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Remove(ulong value)
		{
			_root = Remove(_root, value, _bitLength - 1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Node Remove(Node node, ulong value, int bit)
		{
			System.Diagnostics.Debug.Assert(node != null);

			node.Count -= 1;
			if (node.Count == 0) {
				return null;
			}

			if (bit < 0) {
				return node;
			}

			ulong flag = (value >> bit) & 1;
			node.Child[flag] = Remove(node.Child[flag], value, bit - 1);
			return node;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int LowerBoundIndex(ulong value) => CountLower(_root, value, _bitLength - 1);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int UpperBoundIndex(ulong value) => CountLower(_root, value + 1, _bitLength - 1);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ulong MaxWithXor(ulong x) => FindXorMin(_root, ~x, _bitLength - 1);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ulong MinWithXor(ulong x) => FindXorMin(_root, x, _bitLength - 1);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ulong FindXorMin(Node node, ulong x, int bit) 
		{
			System.Diagnostics.Debug.Assert(node != null);
			if (bit < 0) {
				return 0;
			}

			ulong flag = (x >> bit) & 1;
			flag ^= node.Child[flag] == null ? 1UL : 0UL;
			return FindXorMin(node.Child[flag], x, bit - 1) | (flag << bit);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ulong Get(Node node, int index, int bit)
		{
			if (bit < 0) {
				return 0;
			}

			int leftCount = node.Child[0] != null ? node.Child[0].Count : 0;
			return index < leftCount 
				? Get(node.Child[0], index, bit - 1)
				: Get(node.Child[1], index - leftCount, bit - 1) | (1UL << bit);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ulong GetXor(Node node, int index, ulong x, int bit)
		{
			if (bit < 0) {
				return 0;
			}

			ulong f0 = (x >> bit) ^ 0;
			ulong f1 = (x >> bit) ^ 1;
			int leftCount = node.Child[f0] != null ? node.Child[f0].Count : 0;
			return index < leftCount
				? GetXor(node.Child[f0], index, x, bit - 1) | (f0 << bit)
				: GetXor(node.Child[f1], index - leftCount, x, bit - 1) | (f1 << bit);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int CountLower(Node node, ulong value, int bit)
		{
			if (node == null || bit < 0) {
				return 0;
			}

			ulong f = (value >> bit) & 1;
			return (f != 0 && node.Child[0] != null ? node.Child[0].Count : 0)
				+ CountLower(node.Child[f], value, bit - 1);
		}

		private class Node
		{
			public int Count { get; set; } = 0;
			public Node[] Child { get; set; } = new Node[] { null, null };
		}
	}
}

using System.Runtime.CompilerServices;

namespace TakyTank.KyoProLib.CSharp.V11
{
	/// <summary>トライ木</summary>
	public class Trie<TKey, TValue>
	{
		private readonly int _childCount;
		/// <summary>例えば、'a' ~ 'z' を 0 ~ 25 に変換するデリゲート</summary>
		private readonly Func<TKey, int> _toIndex;
		private readonly TValue _initialValue;
		/// <summary>文字列の0文字目に対応するノード</summary>
		private readonly Node _root;

		/// <summary>i番目に追加した文字列のj番目の文字に対応するノード</summary>
		/// <remarks>
		/// Addした時点でindexは確定し、Removeしてもindexはズレない。
		/// j=0はrootになり、実際の文字に対応するのは1から。
		/// </remarks>
		private Node[][] _wordNodePathes;
		private int _nextWordID = 0;

		public int WordCount => _root.Words.Count;
		public int NodeCount { get; private set; } = 1;

		public Trie(
			int childCount,
			Func<TKey, int> toIndex,
			TValue initialValue,
			int capacity = 100000)
		{
			_childCount = childCount;
			_toIndex = toIndex;
			_initialValue = initialValue;
			_root = new Node(default, _initialValue, _childCount);

			_wordNodePathes = new Node[capacity][];
		}

		// 外部からrootの値を取得する必要は無く、文字列上の0-basedのindexでアクセスしたい。
		// そのため、GetNodeには+1したindexを渡す。
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TValue GetValue(int wordID, int index) => GetNode(wordID, index + 1).Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetValue(int wordID, int index, TValue value)
		{
			GetNode(wordID, index + 1).Value = value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void UpdateValue(int wordID, int index, Func<TValue, TValue> update)
		{
			var node = GetNode(wordID, index + 1);
			node.Value = update(node.Value);
		}

		/// <summary>
		/// 指定した文字列をトライ木に追加
		/// </summary>
		/// <remarks>文字列数をN、文字列長をSとして、O(S + ならしN)</remarks>
		public void Add(ReadOnlySpan<TKey> word)
		{
			if (_nextWordID == _wordNodePathes.Length) {
				Extend(_wordNodePathes.Length * 2);
			}

			int wordID = _nextWordID;
			++_nextWordID;
			var paths = new Node[word.Length + 1];
			paths[0] = _root;

			Node node = _root;
			node.Words.Add(wordID);
			// pathの0文字目はrootなので、実際の文字に対応するindexは1から。
			int i = 1;
			foreach (var w in word) {
				int j = _toIndex(w);
				if (node.Children[j] is null) {
					node.Children[j] = new Node(w, _initialValue, _childCount);
					++NodeCount;
				}

				node = node.Children[j];
				paths[i] = node;
				node.Words.Add(wordID);
				++i;
			}

			node.TerminalWords.Add(wordID);
			_wordNodePathes[wordID] = paths;
		}

		/// <summary>
		/// wordID番目にトライ木に追加した文字列を削除 O(S)
		/// </summary>
		/// <remarks>終端文字のIDを削除するため、文字列そのものではなくIDを渡す実装にする必要がある。</remarks>
		public void Remove(int wordID)
		{
			var path = _wordNodePathes[wordID];
			if (path is null) {
				return;
			}

			_root.Words.Remove(wordID);
			for (int i = 1; i < path.Length; ++i) {
				var node = GetNode(wordID, i);
				node.Words.Remove(wordID);
				if (node.Words.Count == 0) {
					var parent = GetNode(wordID, i - 1);
					int j = _toIndex(node.Key);
					parent.Children[j] = null;
					--NodeCount;
				}
			}

			path[^1].TerminalWords.Remove(wordID);
			_wordNodePathes[wordID] = null;
		}

		/// <summary>
		/// 指定した文字列がトライ木上に存在するかを返す O(S)
		/// </summary>
		public (bool exists, bool exitsAsPrefix, TValue value) Find(ReadOnlySpan<TKey> word)
		{
			Node node = _root;
			foreach (var w in word) {
				int j = _toIndex(w);
				if (node.Children[j] is null) {
					return (false, false, _initialValue);
				}

				node = node.Children[j];
			}

			// ここに来た時点で、wordと先頭が一致する文字列はトライ木上に存在する。
			// 現在のnodeが終端位置となる文字列があるならば、完全一致する文字列が存在する。
			return (node.TerminalWords.Count > 0, true, node.Value);
		}

		/// <summary>
		/// 指定した文字列の接頭辞となる文字列のIDを返す O(S)
		/// </summary>
		/// <remarks>
		/// word が "abcde" の場合 "abc" や "abcde" などが該当する。
		/// </remarks>
		public List<int> FindPrefixWord(ReadOnlySpan<TKey> word)
		{
			var prefixWordIDs = new List<int>();
			Node node = _root;
			foreach (var w in word) {
				int j = _toIndex(w);
				if (node.Children[j] is null) {
					return prefixWordIDs;
				}

				node = node.Children[j];
				// 指定された文字列を辿る段階で、その位置が終端になるものを随時追加していく。
				prefixWordIDs.AddRange(node.TerminalWords);
			}

			return prefixWordIDs;
		}

		/// <summary>
		/// 指定した文字列の接頭辞となる文字列が存在するかを返す O(S)
		/// </summary>
		public bool ExistsPrefixWord(ReadOnlySpan<TKey> word)
		{
			// FindPrefixWord(word).Count > 0 で求まるのはそうなのだが、
			// 存在した時点で短絡評価して高速化するバージョン。
			Node node = _root;
			foreach (var w in word) {
				int j = _toIndex(w);
				if (node.Children[j] is null) {
					return false;
				}

				node = node.Children[j];
				if (node.TerminalWords.Count > 0) {
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// 指定した文字列を接頭辞として持つ文字列のIDを返す O(S)
		/// </summary>
		/// <remarks>
		/// word が "abcde" の場合 "abcdefg" や "abcde" などが該当する。
		/// </remarks>
		public List<int> FindWordHasPrefix(ReadOnlySpan<TKey> word)
		{
			var wordIDs = new List<int>();
			Node node = _root;
			foreach (var w in word) {
				int j = _toIndex(w);
				if (node.Children[j] is null) {
					return wordIDs;
				}

				node = node.Children[j];
			}

			// 指定した文字列の終端に対応するnodeを通る文字列を全て追加
			if (node is not null) {
				wordIDs.AddRange(node.Words);
			}

			return wordIDs;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Node GetNode(int wordID, int index) => _wordNodePathes[wordID][index];

		private void Extend(int newSize)
		{
			var newPathes = new Node[newSize][];
			Array.Copy(_wordNodePathes, newPathes, _wordNodePathes.Length);
			_wordNodePathes = newPathes;
		}

		private class Node
		{
			public TKey Key { get; }
			public TValue Value { get; set; }
			public Node[] Children { get; }

			// Remove不要ならListでよくて、どの文字列かの特定すら不要ならintでよく、
			// 大体の用途ではHashSetだとパフォーマンス的に不利な気はするが……

			/// <summary>このノードを通る文字列のID</summary>
			public HashSet<int> Words { get; set; } = new HashSet<int>();
			/// <summary>このノードが文字列の終端となる文字列のID</summary>
			public HashSet<int> TerminalWords { get; } = new HashSet<int>();

			public Node(TKey key, TValue value, int childCount)
			{
				Key = key;
				Value = value;
				Children = new Node[childCount];
			}
		}
	}

	/// <summary>トライ木(機能削減高速版)</summary>
	public class TrieSlim<TKey, TValue>
	{
		private readonly int _childCount;
		/// <summary>例えば、'a' ~ 'z' を 0 ~ 25 に変換するデリゲート</summary>
		private readonly Func<TKey, int> _toIndex;
		private readonly TValue _initialValue;
		/// <summary>文字列の0文字目に対応するノード</summary>
		private readonly Node _root;

		/// <summary>i番目に追加した文字列のj番目の文字に対応するノード</summary>
		/// <remarks>
		/// Addした時点でindexは確定し、Removeしてもindexはズレない。
		/// j=0はrootになり、実際の文字に対応するのは1から。
		/// </remarks>
		private Node[][] _wordNodePathes;
		private int _nextWordID = 0;

		public int WordCount => _root.WordCount;
		public int NodeCount { get; private set; } = 1;

		public TrieSlim(
			int childCount,
			Func<TKey, int> toIndex,
			TValue initialValue,
			int capacity = 100000)
		{
			_childCount = childCount;
			_toIndex = toIndex;
			_initialValue = initialValue;
			_root = new Node(_initialValue, _childCount);

			_wordNodePathes = new Node[capacity][];
		}

		// 外部からrootの値を取得する必要は無く、文字列上の0-basedのindexでアクセスしたい。
		// そのため、GetNodeには+1したindexを渡す。
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TValue GetValue(int wordID, int index) => GetNode(wordID, index + 1).Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetValue(int wordID, int index, TValue value)
		{
			GetNode(wordID, index + 1).Value = value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void UpdateValue(int wordID, int index, Func<TValue, TValue> update)
		{
			var node = GetNode(wordID, index + 1);
			node.Value = update(node.Value);
		}

		/// <summary>
		/// 指定した文字列をトライ木に追加
		/// </summary>
		/// <remarks>文字列数をN、文字列長をSとして、O(S + ならしN)</remarks>
		public void Add(ReadOnlySpan<TKey> word)
		{
			if (_nextWordID == _wordNodePathes.Length) {
				Extend(_wordNodePathes.Length * 2);
			}

			int wordID = _nextWordID;
			++_nextWordID;
			var paths = new Node[word.Length + 1];
			paths[0] = _root;

			Node node = _root;
			++node.WordCount;
			// pathの0文字目はrootなので、実際の文字に対応するindexは1から。
			int i = 1;
			foreach (var w in word) {
				int j = _toIndex(w);
				if (node.Children[j] is null) {
					node.Children[j] = new Node(_initialValue, _childCount);
					++NodeCount;
				}

				node = node.Children[j];
				paths[i] = node;
				++node.WordCount;
				++i;
			}

			++node.TerminalCount;
			_wordNodePathes[wordID] = paths;
		}

		/// <summary>
		/// 指定した文字列がトライ木上に存在するかを返す O(S)
		/// </summary>
		public (bool exists, bool exitsAsPrefix, TValue value) Find(ReadOnlySpan<TKey> word)
		{
			Node node = _root;
			foreach (var w in word) {
				int j = _toIndex(w);
				if (node.Children[j] is null) {
					return (false, false, _initialValue);
				}

				node = node.Children[j];
			}

			// ここに来た時点で、wordと先頭が一致する文字列はトライ木上に存在する。
			// 現在のnodeが終端位置となる文字列があるならば、完全一致する文字列が存在する。
			return (node.TerminalCount > 0, true, node.Value);
		}

		/// <summary>
		/// 指定した文字列の接頭辞となる文字列が存在するかを返す O(S)
		/// </summary>
		public bool ExistsPrefixWord(ReadOnlySpan<TKey> word)
		{
			// FindPrefixWord(word).Count > 0 で求まるのはそうなのだが、
			// 存在した時点で短絡評価して高速化するバージョン。
			Node node = _root;
			foreach (var w in word) {
				int j = _toIndex(w);
				if (node.Children[j] is null) {
					return false;
				}

				node = node.Children[j];
				if (node.TerminalCount > 0) {
					return true;
				}
			}

			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Node GetNode(int wordID, int index) => _wordNodePathes[wordID][index];

		private void Extend(int newSize)
		{
			var newPathes = new Node[newSize][];
			Array.Copy(_wordNodePathes, newPathes, _wordNodePathes.Length);
			_wordNodePathes = newPathes;
		}

		private class Node
		{
			public TValue Value { get; set; }
			public Node[] Children { get; }

			/// <summary>このノードを通る文字列の数</summary>
			public int WordCount { get; set; } = 0;
			/// <summary>このノードが文字列の終端となる文字列の数</summary>
			public int TerminalCount { get; set; } = 0;

			public Node(TValue value, int childCount)
			{
				Value = value;
				Children = new Node[childCount];
			}
		}
	}
}

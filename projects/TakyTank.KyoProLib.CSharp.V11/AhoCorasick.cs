namespace TakyTank.KyoProLib.CSharp.V11
{
	public class AhoCorasick
	{
		private readonly List<string> _words;
		private readonly Node _root;

		public AhoCorasick(int capacity = 100000)
		{
			_words = new List<string>(capacity);
			_root = new Node();
		}

		public void Add(string word)
		{
			_words.Add(word);
		}

		public void Build()
		{
			// 構築に、Addした文字列長の和をSとしたとき、O(S) かかる。
			CreateTrie();
			CreateFailEdge();
		}

		private void CreateTrie()
		{
			for (int i = 0; i < _words.Count; ++i) {
				string word = _words[i];
				var node = _root;
				foreach (char c in word) {
					if (node.Children.TryGetValue(c, out var child) == false) {
						child = new Node(c);
						node.Children[c] = child;
					}

					node = child;
				}

				node.TerminalID = i;
			}
		}

		private void CreateFailEdge()
		{
			// Fail(失敗遷移)とは、Nodeに対応するキーワードの(自身を除く)接尾辞のうち最長のものに対応するnodeへの遷移である。
			// これは必ず自分より浅いnodeへの遷移となる。
			// Trie木上でBFSを行い、根から順番にFailを確定していく。
			var q = new Queue<Node>();
			q.Enqueue(_root);

			while (q.Count > 0) {
				var current = q.Dequeue();
				foreach (var (c, child) in current.Children) {
					// Queueに格納された時点でcurrentのFailは確定している。
					// 子の接尾辞はcで終わる必要があるため、Failとして指すnodeの対応する文字はcである必要がある。
					// 親のFailが子にcを持っていればそれが最長の接尾辞になるが、
					// 無い場合はさらにFailを辿り徐々に短い接尾辞を探していく。存在しない場合はrootになる。
					var node = current.Fail;
					while (node.Children.ContainsKey(c) == false && node != node.Fail) {
						node = node.Fail;
					}

					if (node.Children.TryGetValue(c, out var candidate) && candidate != child) {
						child.Fail = candidate;
					} else {
						child.Fail = _root;
					}

					// 縮約前の初期参照先としてFailを設定しておく
					child.NextTerminal = child.Fail;
					q.Enqueue(child);
				}
			}
		}

		/// <summary>
		/// 辞書に含まれる各文字列が指定した文字列のどこに現れるかを O(T + K) で列挙して返す
		/// </summary>
		/// <remarks>
		/// 指定した文字列の長さをTとして、マッチ数をKとしたときの計算量。
		/// 辞書に "he" "she" "his" "hers" があり、"ushersihe" を指定した時の結果は、
		/// [[2, 7], [1], [], [2]] になる。
		/// </remarks>
		public List<int>[] FindMatches(string text)
		{
			var ret = new List<int>[_words.Count];
			for (int i = 0; i < ret.Length; ++i) {
				ret[i] = new List<int>();
			}

			var node = _root;

			for (int i = 0; i < text.Length; ++i) {
				char c = text[i];
				// 末尾がcになる文字列に対応するnodeを探す。
				// なるべく現在辿っている文字列をそのまま使いたいので、
				// 子にcがある場合はそのまま使い、無い場合は最長接尾辞であるFailを辿る。
				while (node.Children.ContainsKey(c) == false && node != node.Fail) {
					node = node.Fail;
				}

				if (node.Children.TryGetValue(c, out var child)) {
					node = child;
				}

				if (node.TerminalID != Node.NoTerminal) {
					// 現在のnodeに対応した文字列が一致。
					int id = node.TerminalID;
					// 現在位置は文字列の末端なので、先頭indexを計算するために文字列長を引く。
					ret[id].Add(i + 1 - _words[id].Length);
				}

				// 終端がcになる文字列を順番に探す。
				// rootからnodeに対応する文字列は、textの部分文字列で終端がcの文字列である。
				// Failを辿ることで、その接尾辞、すなわちtextの部分文字列で終端がcのものを効率良く辿ることができる。
				// さらに、その中で文字列の末端nodeになるものだけを繋いだ縮約辺を作ることで、
				// 同じnodeが複数回参照されたときの遷移を効率化する。
				var terminal = node;
				while (terminal.FindNext() != _root) {
					terminal = terminal.NextTerminal;
					// FindNextで経路圧縮後のNextOutputは終端nodeかrootなので、ここではTerminalIDは確実に存在する。
					int id = terminal.TerminalID;
					ret[id].Add(i + 1 - _words[id].Length);
				}
			}

			return ret;
		}

		/// summary>
		/// 辞書に含まれる各文字列が指定した文字列中に何回現れるかを O(S + T) で列挙して返す
		/// </summary>
		/// <remarks>
		/// 辞書に含まれる文字列長の和をS、指定した文字列の長さをTとしたときの計算量。
		/// 辞書に "he" "she" "his" "hers" があり、"ushersihe" を指定した時の結果は、
		/// [2, 1, 0, 1] になる。
		/// </remarks>
		public long[] CountMatches(string text)
		{
			// Trie木の辺でトポロジカルソート
			// 最終的にはこのトポ順の逆順でDPをする。
			// Fail辺は浅い方向に向かう辺なので、逆トポ順に内包される。
			// そのため、ソート時は考慮しなくてよい。
			var topo = new List<Node>();
			var q = new Queue<Node>();
			q.Enqueue(_root);

			while (q.Count > 0) {
				var now = q.Dequeue();
				// 前回のCountの初期化
				now.Count = 0;
				topo.Add(now);
				foreach (var nxt in now.Children.Values) {
					q.Enqueue(nxt);
				}
			}

			// まず、指定した文字列の各文字に対応するnodeをカウントアップする。
			// このとき、最長接尾辞であるFailを使ってなるべく深い位置のnodeを参照するようにする。
			var node = _root;
			foreach (char a in text) {
				while (node.Children.ContainsKey(a) == false && node != node.Fail) {
					node = node.Fail;
				}

				if (node.Children.TryGetValue(a, out var nxt)) {
					node = nxt;
				}

				node.Count += 1;
			}

			// 逆トポ順にDPを行う。
			// rootは処理しないのでループは1まででよい。
			var counts = new long[_words.Count];
			for (int i = topo.Count - 1; i >= 1; --i) {
				node = topo[i];
				if (node.TerminalID != Node.NoTerminal) {
					int id = node.TerminalID;
					counts[id] += node.Count;
				}

				// Failの遷移先は接尾辞nodeなので、
				// nodeの文字列の出現回数と同じ分、接尾辞も出現することになる。
				node.Fail.Count += node.Count;
			}

			return counts;
		}

		private class Node
		{
			public static readonly int NoTerminal = -1;
			// TODO こいつ、デバッグ用途以外に要らないんだよな。
			private readonly char? _c;

			/// <summary>失敗遷移先のnode</summary>
			public Node Fail;
			/// <summary>このノードで終端となる文字列のインデックス(なければ-1)</summary>
			public int TerminalID { get; set; } = NoTerminal;
			/// <summary>次の終端node候補</summary>
			public Node NextTerminal { get; set; }
			/// <summary>出現数カウント用</summary>
			public int Count { get; set; } = 0;
			/// <summary>子ノード</summary>
			public Dictionary<char, Node> Children = new();

			public Node(char? c = null)
			{
				_c = c;
				Fail = this; // root 用に自分自身を指す
				NextTerminal = this; // 初期は自分
			}

			public Node FindNext()
			{
				if (this == NextTerminal) {
					return this;
				}

				// 経路圧縮して、次の文字列終端の探索を効率化する。
				if (NextTerminal.TerminalID == NoTerminal) {
					NextTerminal = NextTerminal.FindNext();
				}

				return NextTerminal;
			}
		}
	}
}

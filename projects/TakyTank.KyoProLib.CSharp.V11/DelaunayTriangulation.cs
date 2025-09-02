namespace TakyTank.KyoProLib.CSharp.V11
{
	/// <summary>ドロネー三角形分割クラス(平面)</summary>
	/// <remarks>
	/// 逐次挿入法（incremental insertion）により三角形分割を構築する。
	/// 点の順序は乱択され、エッジの合法性を保つためにフリップ操作を行う。
	/// TODO 3点が一直線上にあるとき、特にMSTが正しく出ない。
	/// あと、整数座標にしないと結構重い。
	/// </remarks>
	public class DelaunayTriangulation
	{
		private readonly List<Point> _points;
		private readonly List<Triangle> _triangles;
		private readonly List<List<int>> _children;
		private readonly Dictionary<Edge, (int k1, int k2)> _edge2Triangle;

		private int _n;
		private (int a, int b)[] _edges;

		private uint _xor128X, _xor128Y, _xor128Z, _xor128W;

		/// <param name="capacity">頂点数</param>
		/// <param name="seed">乱数シード（省略可）</param>
		public DelaunayTriangulation(int capacity = -1, uint seed = 123456789u)
		{
			if (capacity <= 0) {
				capacity = 100000;
			}

			// 最終的な三角形分割に含まれる辺の数は、凸包の上の点の個数をKとして、3N - 3 - K 個となる。
			// また、分割途中も含めて、作成される三角形の期待値は 9N + 1 以下である。
			// 期待値なのでちょっと多めに 10*N の初期サイズを確保しておく。
			_points = new List<Point>(capacity + 3);
			_triangles = new List<Triangle>(capacity * 10);
			_children = new List<List<int>>(capacity * 10);
			_edge2Triangle = new Dictionary<Edge, (int k1, int k2)>(capacity * 10);

			// xor128の初期化
			_xor128X = seed;
			_xor128Y = 362436069u;
			_xor128Z = 521288629u;
			_xor128W = 88675123u;
		}

		public void AddPoint(double x, double y)
		{
			_points.Add(new Point(x, y));
		}

		/// <summary>
		/// ドロネー三角形分割を実行 O(N * logN)
		/// </summary>
		/// <param name="minDelta">エッジ上に点が落ちた場合の最小摂動量</param>
		/// <param name="maxDelta">エッジ上に点が落ちた場合の最大摂動量</param>
		/// <param name="maxMissCount">摂動を試みる最大回数</param>
		public void Build(double minDelta = 1e-6, double maxDelta = 1e-5, int maxMissCount = 30)
		{
			_n = _points.Count;

			// 全点を内部に含む十分大きな外接初期三角形を追加。
			// この点を含む三角形は最終的に除去される。
			double r = double.MinValue;
			for (int i = 0; i < _n; ++i) {
				r = Math.Max(r, Math.Abs(_points[i].X));
				r = Math.Max(r, Math.Abs(_points[i].Y));
			}

			_points.Add(new Point(3.1 * r, 0));
			_points.Add(new Point(0, 3.1 * r));
			_points.Add(new Point(-3.1 * r, -3.1 * r));

			_triangles.Add(CreateCcwTriangle(_n, _n + 1, _n + 2));
			_children.Add(new List<int>());
			RegisterTriangle(0, _triangles[0]);

			// ランダム順序で点を挿入
			var ids = new int[_n];
			for (int i = 0; i < _n; ++i) {
				ids[i] = i;
			}

			for (int i = 0; i < _n; ++i) {
				int jj = (int)(Xor128() % (uint)(_n - i));
				int kk = _n - i - 1;
				(ids[jj], ids[kk]) = (ids[kk], ids[jj]);
			}

			// 各点を順に挿入し、三角形分割を更新
			foreach (int p in ids) {
				int missCount = 0;
				var (px, py) = (_points[p].X, _points[p].Y);
				// 三角形の分割で出来た木構造を根から順に辿り、次に分割する三角形を探す。
				int k = 0;
				while (_children[k].Count > 0) {
					int next = FindChild(k, p);
					if (next == -1) {
						++missCount;
						if (missCount >= maxMissCount) {
							break;
						}

						// 挿入する点が現在見ている三角形を分割する辺上にある場合、どちらの子に進むべきか確定できない。
						// 点を微小に摂動させて、どちらかの三角形に適当に押し込む。
						double dx = minDelta + (maxDelta - minDelta) * (Xor128() / (double)uint.MaxValue);
						double dy = minDelta + (maxDelta - minDelta) * (Xor128() / (double)uint.MaxValue);
						dx *= Xor128() % 2 == 0 ? 1 : -1;
						dy *= Xor128() % 2 == 0 ? 1 : -1;
						_points[p] = new Point(px + dx, py + dy);
					} else {
						k = next;
					}
				}

				if (_children[k].Count == 0) {
					// 追加した点で三角形を分割
					DivideTriangle(k, p);
				}

				// 摂動分を元に戻す
				_points[p] = new Point(px, py);
			}

			// 外側凸包の辺の調整
			ModifyConvexity();

			// 最初に追加した外接三角形のダミー点を含まない辺のみを抽出
			var tempEdges = new List<(int a, int b)>();
			foreach (var kv in _edge2Triangle) {
				var e = kv.Key;
				if (e.A < _n && e.B < _n) {
					tempEdges.Add((e.A, e.B));
				}
			}

			_edges = tempEdges.ToArray();
		}

		/// <summary>
		/// 三角形分割結果の辺集合を取得
		/// </summary>
		/// <returns>辺集合を頂点番号のペアのコレクションとして返す</returns>
		public ReadOnlySpan<(int a, int b)> GetEdges() => _edges ??= Array.Empty<(int a, int b)>();

		/// <summary>
		/// 三角形の辺のみを使って最小全域木を作成
		/// </summary>
		public (double length, (int u, int v, double cost)[] edges) CreateMST()
		{
			var edgesList = new List<(int to, double d)>[_n];
			for (int i = 0; i < _n; i++) {
				edgesList[i] = new List<(int to, double d)>();
			}

			foreach (var e in GetEdges()) {
				var p = _points[e.a];
				var q = _points[e.b];
				var r = p - q;
				double d = Math.Sqrt(r.X * r.X + r.Y * r.Y);
				edgesList[e.a].Add((e.b, d));
				edgesList[e.b].Add((e.a, d));
			}

			double length = 0;
			var edges = new (int u, int v, double cost)[_n - 1];
			int count = 0;
			var done = new bool[_n];
			var que = new PriorityQueue<(int u, int v), double>();
			done[0] = true;
			foreach (var e in edgesList[0]) {
				que.Enqueue((0, e.to), e.d);
			}

			while (que.Count > 0) {
				que.TryDequeue(out var tag, out double priority);
				if (done[tag.v]) {
					continue;
				}

				done[tag.v] = true;
				length += priority;
				edges[count] = (tag.u, tag.v, priority);
				++count;
				foreach (var next in edgesList[tag.v]) {
					if (done[next.to] == false) {
						que.Enqueue((tag.v, next.to), next.d);
					}
				}
			}

			return (length, edges);
		}

		private bool IsCcw(int a, int b, int c) => Point.Cross(_points[b] - _points[a], _points[c] - _points[a]) > 0.0;

		private Triangle CreateCcwTriangle(int a, int b, int c)
		{
			if (!IsCcw(a, b, c)) {
				(c, b) = (b, c);
			}

			return new Triangle(a, b, c);
		}

		private bool ContainsPoint(int p, Triangle t)
		{
			// 三角形の頂点ABCが反時計回りに配置されているので、
			// 点Pが三角形内部にあれば全部反時計回りになる。
			if (!IsCcw(t.A, t.B, p) || !IsCcw(t.B, t.C, p) || !IsCcw(t.C, t.A, p)) {
				return false;
			}

			return true;
		}

		private int AddChildTriangle(int k, Triangle child)
		{
			int newK = _triangles.Count;
			_triangles.Add(child);
			_children.Add(new List<int>());
			_children[k].Add(newK);

			return newK;
		}

		/// <summary>
		/// 2つの親三角形を持つ子三角形を追加します。
		/// </summary>
		private int AddChildTriangle(int k1, int k2, Triangle child)
		{
			// 2つの親三角形を持つ子三角形を追加
			int newK = _triangles.Count;
			_triangles.Add(child);
			_children.Add(new List<int>());
			_children[k1].Add(newK);
			_children[k2].Add(newK);

			return newK;
		}

		private int FindChild(int k, int p)
		{
			// 指定した三角形の子のうち、点が含まれるものを探す。
			foreach (var i in _children[k]) {
				if (ContainsPoint(p, _triangles[i])) {
					return i;
				}
			}

			return -1;
		}

		private void RegisterTriangle(int k, Triangle t)
		{
			AddEdgeMap(new Edge(t.A, t.B), k);
			AddEdgeMap(new Edge(t.B, t.C), k);
			AddEdgeMap(new Edge(t.C, t.A), k);
		}

		private void AddEdgeMap(Edge e, int k)
		{
			if (!_edge2Triangle.TryGetValue(e, out var pair)) {
				_edge2Triangle[e] = (k, -1);
			} else if (pair.k2 == -1) {
				_edge2Triangle[e] = (pair.k1, k);
			} else {
				// 2つ以上は持たないはず
				throw new InvalidOperationException("Edge has more than 2 triangles.");
			}
		}

		private void UnregisterTriangle(int k)
		{
			var t = _triangles[k];
			RemoveEdgeMap(new Edge(t.A, t.B), k);
			RemoveEdgeMap(new Edge(t.B, t.C), k);
			RemoveEdgeMap(new Edge(t.C, t.A), k);
		}

		private void RemoveEdgeMap(Edge e, int k)
		{
			if (_edge2Triangle.TryGetValue(e, out var pair)) {
				if (pair.k1 == k && pair.k2 == -1) {
					_edge2Triangle.Remove(e);
				} else if (pair.k1 == k) {
					// 1個しか無い場合は常にk2側が-1になるようにする
					_edge2Triangle[e] = (pair.k2, -1);
				} else if (pair.k2 == k) {
					_edge2Triangle[e] = (pair.k1, -1);
				}
			}
		}

		private bool IsIllegal(Edge e, int c, int d)
		{
			// edgeをABとしたとき、三角形ABCとABDがあり、
			// その四角形ACBDに対しての最適な分割がCDではなくABであることの判定。
			var (a, b) = (e.A, e.B);

			// 三角形ABCの外接円の中心Sを求める
			Point s1 = (_points[a] + _points[b]) / 2.0;
			Point s2 = (_points[b] + _points[c]) / 2.0;
			Point t1 = s1 + (_points[b] - _points[a]).Normal();
			Point t2 = s2 + (_points[c] - _points[b]).Normal();

			double d1 = Point.Cross(t2 - s2, s2 - s1);
			double d2 = Point.Cross(t2 - s2, t1 - s1);
			Point s = s1 + (t1 - s1) * (d1 / d2);

			// Dが外接円の内側にあると違法(非ドロネー)となる。
			// SAとSDの距離を比較する。
			double dx1 = _points[a].X - s.X;
			double dy1 = _points[a].Y - s.Y;
			double dx2 = _points[d].X - s.X;
			double dy2 = _points[d].Y - s.Y;
			return (dx1 * dx1 + dy1 * dy1) > (dx2 * dx2 + dy2 * dy2);
		}

		private void LegalizeEdge(int pivot, Edge e)
		{
			if (!_edge2Triangle.TryGetValue(e, out var set) || set.k2 == -1) {
				return;
			}

			// 辺に接する2つの三角形を取得
			var (k1, k2) = set;
			int c = _triangles[k1].Opposite(e);
			int d = _triangles[k2].Opposite(e);

			if (IsIllegal(e, c, d)) {
				// edgeをABとしたとき、現在は三角形ABCとABDがあるが、
				// これを削除して三角形ACDとBCDを追加する。(フリップ操作)
				UnregisterTriangle(k1);
				UnregisterTriangle(k2);
				_edge2Triangle.Remove(e);

				var t1 = CreateCcwTriangle(e.A, c, d);
				var t2 = CreateCcwTriangle(e.B, c, d);

				int nextK1 = AddChildTriangle(k1, k2, t1);
				int nextK2 = AddChildTriangle(k1, k2, t2);

				RegisterTriangle(nextK1, t1);
				RegisterTriangle(nextK2, t2);

				// 元の辺ABに対してpivotと反対側の点をQとしたとき、
				// 新しい辺QAに対するpivotの反対側の点Rを考えたときに、辺QAが違法になっている可能性がある。
				// 再帰的にチェックする。QBに対しても同様。
				int q = pivot != c ? c : d;
				LegalizeEdge(pivot, new Edge(e.A, q));
				LegalizeEdge(pivot, new Edge(e.B, q));
			}
		}

		private void DivideTriangle(int k, int p)
		{
			// 三角形ABC内の点Pで、ABP, BCP, CAPの3つの三角形に分割
			UnregisterTriangle(k);

			var t1 = CreateCcwTriangle(_triangles[k].A, _triangles[k].B, p);
			var t2 = CreateCcwTriangle(_triangles[k].B, _triangles[k].C, p);
			var t3 = CreateCcwTriangle(_triangles[k].C, _triangles[k].A, p);

			int k1 = AddChildTriangle(k, t1);
			int k2 = AddChildTriangle(k, t2);
			int k3 = AddChildTriangle(k, t3);

			RegisterTriangle(k1, t1);
			RegisterTriangle(k2, t2);
			RegisterTriangle(k3, t3);

			// 辺ABに対するABPとは反対側の三角形の残り一つの頂点Qを考えたとき、
			// 辺PQの方がドロネーである場合はスワップが必要。BCやCAについても同様。
			LegalizeEdge(p, new Edge(_triangles[k].A, _triangles[k].B));
			LegalizeEdge(p, new Edge(_triangles[k].B, _triangles[k].C));
			LegalizeEdge(p, new Edge(_triangles[k].C, _triangles[k].A));
		}

		private void ModifyConvexity()
		{
			// 最初に追加した外接三角形のダミー点をA、そうではない点をBとする。
			// また、辺ABを含む2つの三角形の残りの頂点をC,Dとし、これもダミーではないとする。
			// 分割によって三角形ABCと三角形ADBが出来てしまっている場合に、
			// 本当に必要な三角形BCDが結果に含まれるように構成し直す。
			for (int a = _n; a <= _n + 2; ++a) {
				for (int b = 0; b < _n; ++b) {
					var e = new Edge(b, a);
					// 三角形が両側にある場合のみ考える
					if (!_edge2Triangle.TryGetValue(e, out var set) || set.k2 == -1) {
						continue;
					}

					// 辺に接する2つの三角形を取得
					var (k1, k2) = set;

					// 辺ABに対してCが左側に来るようにする
					int c = _triangles[k1].Opposite(e);
					int d = _triangles[k2].Opposite(e);
					if (!IsCcw(a, b, c)) {
						(d, c) = (c, d);
					}

					if (!IsCcw(c, d, b)) {
						// Dの配置が不適切なので処理しない
						continue;
					}

					// 最終的な結果に含まれるのは三角形BCDだけであるが、
					// このスワップ処理後のACDの様な配置に元々なっている三角形は除去されずに残るため、
					// 整合性を取るためにACDも再度追加する。
					// _edgesを作成する時には弾くので問題無い。
					UnregisterTriangle(k1);
					UnregisterTriangle(k2);

					var t1 = CreateCcwTriangle(a, c, d);
					var t2 = CreateCcwTriangle(b, c, d);

					int newK1 = AddChildTriangle(k1, k2, t1);
					int newK2 = AddChildTriangle(k1, k2, t2);

					RegisterTriangle(newK1, t1);
					RegisterTriangle(newK2, t2);
				}
			}
		}

		private uint Xor128()
		{
			// XOR128を使った疑似乱数生成
			uint t = _xor128X ^ (_xor128X << 11);
			_xor128X = _xor128Y;
			_xor128Y = _xor128Z;
			_xor128Z = _xor128W;
			_xor128W = _xor128W ^ (_xor128W >> 19) ^ t ^ (t >> 8);

			return _xor128W;
		}

		private readonly struct Point
		{
			public readonly double X, Y;
			public Point(double x, double y)
			{
				X = x;
				Y = y;
			}

			public static Point operator +(Point a, Point b) => new(a.X + b.X, a.Y + b.Y);
			public static Point operator -(Point a, Point b) => new(a.X - b.X, a.Y - b.Y);
			public static Point operator *(Point a, double d) => new(a.X * d, a.Y * d);
			public static Point operator /(Point a, double d) => new(a.X / d, a.Y / d);

			public Point Normal() => new(Y, -X);
			public readonly double Norm() => (X * X) + (Y * Y);

			public static double Dot(Point a, Point b) => a.X * b.X + a.Y * b.Y;
			public static double Cross(Point a, Point b) => a.X * b.Y - a.Y * b.X;
		}

		private readonly struct Edge : IEquatable<Edge>
		{
			public readonly int A;
			public readonly int B;

			public Edge(int a, int b)
			{
				if (a <= b) {
					A = a;
					B = b;
				} else {
					A = b;
					B = a;
				}
			}

			public bool Equals(Edge other) => A == other.A && B == other.B;
			public override bool Equals(object obj) => obj is Edge e && Equals(e);
			public override int GetHashCode() => HashCode.Combine(A, B);
			public static bool operator ==(Edge left, Edge right) => left.Equals(right);
			public static bool operator !=(Edge left, Edge right) => !(left == right);
		}

		private readonly struct Triangle
		{
			public readonly int A, B, C;
			public Triangle(int a, int b, int c)
			{
				A = a;
				B = b;
				C = c;
			}

			public readonly int Opposite(Edge e)
			{
				if (e.A != A && e.B != A) {
					return A;
				}

				if (e.A != B && e.B != B) {
					return B;
				}

				return C;
			}
		}
	}
}

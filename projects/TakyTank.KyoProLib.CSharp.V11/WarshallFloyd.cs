namespace TakyTank.KyoProLib.CSharp.V11;

/// <summary>ワーシャルフロイド</summary>
public class WarshallFloyd
{
	// 最大でinf+inf+infが発生するのでオーバーフローしないように /4
	public const long INF = long.MaxValue / 4;
	readonly int _n;
	readonly long[][] _distances;

	public long this[int u, int v] => _distances[u][v];

	public WarshallFloyd(int n, long inf = INF)
	{
		_n = n;
		_distances = new long[n][];
		for (int i = 0; i < n; i++) {
			_distances[i] = new long[n];
			for (int j = 0; j < n; j++) {
				_distances[i][j] = inf;
			}

			_distances[i][i] = 0;
		}
	}

	public void AddEdge(int u, int v, long c)
	{
		_distances[u][v] = Math.Min(_distances[u][v], c);
	}

	public void AddEdge2W(int u, int v, long c)
	{
		_distances[u][v] = Math.Min(_distances[u][v], c);
		_distances[v][u] = Math.Min(_distances[v][u], c);
	}

	/// <summary>Addした辺で全点間最短距離を求める O(N^3)</summary>
	public void Build()
	{
		for (int k = 0; k < _n; k++) {
			for (int i = 0; i < _n; i++) {
				for (int j = 0; j < _n; j++) {
					_distances[i][j] = Math.Min(_distances[i][j], _distances[i][k] + _distances[k][j]);
				}
			}
		}
	}

	/// <summary>辺UVの距離を更新し全点間最短距離を更新する O(N^2)</summary>
	/// <remarks>
	/// コストの更新は小さい方向にしか行われない。
	/// つまり、現在の辺UVのコストより大きいCが渡されても、
	/// 元々あった辺が無い場合の最短距離に置き換わる訳では無い。
	/// </remarks>
	public void Update(int u, int v, long c)
	{
		_distances[u][v] = Math.Min(_distances[u][v], c);
		for (int i = 0; i < _n; i++) {
			for (int j = 0; j < _n; j++) {
				_distances[i][j] = Math.Min(
					_distances[i][j],
					_distances[i][u] + _distances[u][v] + _distances[v][j]);
			}
		}
	}

	public void Update2W(int u, int v, long c)
	{
		_distances[u][v] = Math.Min(_distances[u][v], c);
		_distances[v][u] = Math.Min(_distances[u][v], c);

		for (int i = 0; i < _n; i++) {
			for (int j = 0; j < _n; j++) {
				_distances[i][j] = Math.Min(
					_distances[i][j],
					_distances[i][u] + _distances[u][v] + _distances[v][j]);
				_distances[i][j] = Math.Min(
					_distances[i][j],
					_distances[i][v] + _distances[v][u] + _distances[u][j]);
			}
		}
	}
}

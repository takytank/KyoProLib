using System.Runtime.InteropServices;

namespace TakyTank.KyoProLib.CSharp.V11;

/// <summary>
/// 壁で移動不可能な方向がある2次元グリッドのクラス
/// </summary>
/// <remarks>
/// マスの中身は書き換えることがあるが、迷路の情報が書き換わることは少ないため、
/// このクラスでは迷路のマスそのものの情報は持たせず、
/// 迷路の道の情報だけを持つようにする。
/// </remarks>
public class Maze
{
	/// <summary>BFSなどをする時に使う配列</summary>
	/// <remarks>
	/// 領域を使い回すためにフィールドにする。
	/// </remarks>
	private int[,] _distances = null;

	/// <summary>迷路の水平方向のマス数</summary>
	public int Width { get; private set; }
	/// <summary>迷路の垂直方向のマス数</summary>
	public int Height { get; private set; }
	/// <summary>各マスから動ける方向の一覧</summary>
	public Direction4[,][] Moveables { get; private set; }
	/// <summary>各マスから指定方向に動けるか</summary>
	public bool[,][] CanMoves { get; private set; }
	public bool CanMove(int i, int j, Direction4 dir) => CanMoves[i, j][dir.ToIndex4()];
	/// <summary>各マスから移動可能な隣接4マスの座標情報</summary>
	public (int i, int j)[,][] Adjacence4 { get; private set; }

	/// <summary>
	/// 指定した壁の情報で迷路を作成
	/// </summary>
	/// <param name="width">迷路の水平方向のマス数</param>
	/// <param name="height">迷路の垂直方向のマス数</param>
	/// <param name="verticalWalls">
	/// 垂直な壁情報。
	/// つまり、水平方向に移動可能かどうかを表す[height, width-1]個の情報。
	/// 対応するサイズの文字列配列として与えられ、
	/// 0の場合に壁が無いことを、1の場合に壁があることを表す。
	/// </param>
	/// <param name="horizontalWalls">
	/// 水平な壁情報。
	/// つまり、垂直方向に移動可能かどうかを表す[height-1, width]個の情報。
	/// あとは垂直と同じ。
	/// </param>
	/// <param name="canStay">
	/// true: 動かないということが可能。CanMoveに Dir.N が追加される。
	/// </param>
	public Maze(int width, int height, string[] verticalWalls, string[] horizontalWalls, bool canStay = false)
	{
		var tempVerticals = new bool[height, width - 1];
		for (int i = 0; i < height; i++) {
			for (int j = 0; j < width - 1; j++) {
				tempVerticals[i, j] = verticalWalls[i][j] == '1';
			}
		}

		var tempHorizontals = new bool[height - 1, width];
		for (int i = 0; i < height - 1; i++) {
			for (int j = 0; j < width; j++) {
				tempHorizontals[i, j] = horizontalWalls[i][j] == '1';
			}
		}

		InitializeCore(width, height, tempVerticals, tempHorizontals, canStay);
	}

	/// <summary>
	/// 指定した壁の情報で迷路を作成
	/// </summary>
	/// <param name="width">迷路の水平方向のマス数</param>
	/// <param name="height">迷路の垂直方向のマス数</param>
	/// <param name="verticalWalls">
	/// 垂直な壁情報。
	/// つまり、水平方向に移動可能かどうかを表す[height, width-1]個の情報。
	/// trueの場合に壁がある。
	/// </param>
	/// <param name="horizontalWalls">
	/// 水平な壁情報。
	/// つまり、垂直方向に移動可能かどうかを表す[height-1, width]個の情報。
	/// trueの場合に壁がある。
	/// </param>
	/// <param name="canStay">
	/// true: 動かないということが可能。CanMoveに Dir.N が追加される。
	/// </param>
	public Maze(int width, int height, bool[,] verticalWalls, bool[,] horizontalWalls, bool canStay = false)
		=> InitializeCore(width, height, verticalWalls, horizontalWalls, canStay);

	private void InitializeCore(int width, int height, bool[,] verticalWalls, bool[,] horizontalWalls, bool canStay = false)
	{
		Width = width;
		Height = height;
		_distances = new int[Height, Width];

		Moveables = new Direction4[Height, Width][];
		CanMoves = new bool[Height, Width][];
		for (int i = 0; i < Height; i++) {
			for (int j = 0; j < Width; j++) {
				var moves = new List<Direction4>();
				if (canStay) {
					moves.Add(Direction4.N);
				}

				if (i > 0 && horizontalWalls[i - 1, j] == false) {
					moves.Add(Direction4.U);
				}

				if (i < Height - 1 && horizontalWalls[i, j] == false) {
					moves.Add(Direction4.D);
				}

				if (j > 0 && verticalWalls[i, j - 1] == false) {
					moves.Add(Direction4.L);
				}

				if (j < Width - 1 && verticalWalls[i, j] == false) {
					moves.Add(Direction4.R);
				}

				Moveables[i, j] = moves.ToArray();

				CanMoves[i, j] = new bool[5];
				foreach (var d in moves) {
					CanMoves[i, j][d.ToIndex4()] = true;
				}
			}
		}

		Adjacence4 = new (int i, int j)[Height, Width][];
		for (int i = 0; i < Height; i++) {
			for (int j = 0; j < Width; j++) {
				var temp = new List<(int i, int j)>();
				foreach (var dir in Moveables[i, j]) {
					if (dir == Direction4.N) {
						continue;
					}

					temp.Add(dir.Move(i, j));
				}

				Adjacence4[i, j] = temp.ToArray();
			}
		}
	}

	/// <summary>
	/// 指定した位置から距離length以内のマスをBFSで列挙
	/// </summary>
	/// <param name="si">探索開始位置Y座標</param>
	/// <param name="sj">探索開始位置X座標</param>
	/// <param name="length">探索距離</param>
	/// <returns>
	/// positions -> 距離length以内のマスの集合
	/// trace -> 経路復元のためのオブジェクト
	/// </returns>
	public ((int i, int j, int d, int id)[] positions, Trace<Direction4> trace) Bfs(int si, int sj, int length)
	{
		MemoryMarshal.CreateSpan(ref _distances[0, 0], _distances.Length).Fill(int.MaxValue);

		var que = new Queue<(int i, int j, int id)>();
		var ret = new List<(int i, int j, int d, int id)>();
		var trace = new Trace<Direction4>();
		que.Enqueue((si, sj, 0));
		_distances[si, sj] = 0;

		while (que.Count > 0) {
			var (i, j, id) = que.Dequeue();
			int nd = _distances[i, j] + 1;
			if (nd > length) {
				continue;
			}

			ret.Add((i, j, _distances[i, j], id));

			foreach (var dir in Moveables[i, j]) {
				var (ni, nj) = dir.Move(i, j);
				if (_distances[ni, nj] != int.MaxValue) {
					continue;
				}

				_distances[ni, nj] = nd;
				int nextID = trace.Add(dir, id);
				que.Enqueue((ni, nj, nextID));
			}
		}

		return (ret.ToArray(), trace);
	}

	/// <summary>
	/// 指定した位置から条件に合うマスを1つ見つけるまでBFS
	/// </summary>
	/// <param name="si">探索開始位置Y座標</param>
	/// <param name="sj">探索開始位置X座標</param>
	/// <param name="moveables">移動可能な方向</param>
	/// <param name="needs">条件に合うマスのときにtrueを返すデリゲート</param>
	/// <returns>
	/// positions -> 探索したマスの集合
	/// trace -> 経路復元のためのオブジェクト
	/// found -> true:条件に合うマスを見つけた
	/// </returns>
	public ((int i, int j, int d, int id)[] positions, Trace<Direction4> trace, bool found)
		Bfs(
		int si, int sj, Direction4[] moveables, Predicate<(int i, int j)> needs)
	{
		MemoryMarshal.CreateSpan(ref _distances[0, 0], _distances.Length).Fill(int.MaxValue);

		var que = new Queue<(int i, int j, int id)>();
		var ret = new List<(int i, int j, int d, int id)>();
		var trace = new Trace<Direction4>();
		que.Enqueue((si, sj, 0));
		_distances[si, sj] = 0;

		while (que.Count > 0) {
			var (i, j, id) = que.Dequeue();
			int nd = _distances[i, j] + 1;

			if (needs.Invoke((i, j))) {
				ret.Add((i, j, _distances[i, j], id));
				return (ret.ToArray(), trace, true);
			}

			foreach (var dir in moveables) {
				if (CanMove(i, j, dir) == false) {
					continue;
				}

				var (ni, nj) = dir.Move(i, j);
				if (_distances[ni, nj] != int.MaxValue) {
					continue;
				}

				_distances[ni, nj] = nd;
				int nextID = trace.Add(dir, id);
				que.Enqueue((ni, nj, nextID));
			}
		}

		return (ret.ToArray(), trace, false);
	}
}

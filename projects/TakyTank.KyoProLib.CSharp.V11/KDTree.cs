using System;
using System.Collections.Generic;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V11
{
	public class KDTree2
	{
		private readonly int _n;
		private readonly (long x, long y, int i)[] _points;
		private readonly Node _root;

		public KDTree2((long x, long y)[] points)
		{
			_n = points.Length;
			_points = new (long x, long y, int i)[_n];
			for (int i = 0; i < _n; i++) {
				_points[i] = (points[i].x, points[i].y, i);
			}

			_root = Build(0, _n, 0);
		}

		private Node Build(int left, int right, int depth)
		{
			if (left < right == false) {
				return null;
			}

			int mid = (left + right) >> 1;
			if ((depth & 1) != 0) {
				NthElement.Select(_points, x => x.x, left, right, mid);
			} else {
				NthElement.Select(_points, x => x.y, left, right, mid);
			}

			return new Node(Build(left, mid, depth + 1), Build(mid + 1, right, depth + 1), _points[mid]);
		}

		public (int index, long distance) FindNearest(long x, long y)
		{
			int index = -1;
			long distance = long.MaxValue;
			FindNearest(_root, x, y, 0, ref index, ref distance);

			return (index, distance);
		}

		private void FindNearest(Node node, long x, long y, int depth, ref int index, ref long minDistance)
		{
			if (node == null) {
				return;
			}

			var p = node.Point;
			long distance = (x - p.x) * (x - p.x) + (y - p.y) * (y - p.y);
			if (distance < minDistance) {
				index = p.i;
				minDistance = distance;
			}

			int nextDepth = depth + 1;
			if ((depth & 1) != 0) {
				if (node.Left != null && x - minDistance <= p.x) {
					FindNearest(node.Left, x, y, nextDepth, ref index, ref minDistance);
				}

				if (node.Right != null && p.x <= x + minDistance) {
					FindNearest(node.Right, x, y, nextDepth, ref index, ref minDistance);
				}
			} else {
				if (node.Left != null && y - minDistance <= p.y) {
					FindNearest(node.Left, x, y, nextDepth, ref index, ref minDistance);
				}

				if (node.Right != null && p.y <= y + minDistance) {
					FindNearest(node.Right, x, y, nextDepth, ref index, ref minDistance);
				}
			}
		}

		public List<int> FindPoints(long sx, long tx, long sy, long ty)
		{
			var result = new List<int>();
			FindPoints(_root, sx, tx, sy, ty, 0, result);
			return result;
		}

		private void FindPoints(Node node, long sx, long tx, long sy, long ty, int depth, List<int> result)
		{
			var (px, py, pi) = node.Point;
			if (sx <= px && px <= tx && sy <= py && py <= ty) {
				result.Add(pi);
			}

			int nextDepth = depth + 1;
			if ((depth & 1) != 0) {
				if (node.Left != null && sx <= px) {
					FindPoints(node.Left, sx, tx, sy, ty, nextDepth, result);
				}

				if (node.Right != null && px <= tx) {
					FindPoints(node.Right, sx, tx, sy, ty, nextDepth, result);
				}
			} else {
				if (node.Left != null && sy <= py) {
					FindPoints(node.Left, sx, tx, sy, ty, nextDepth, result);
				}

				if (node.Right != null && py <= ty) {
					FindPoints(node.Right, sx, tx, sy, ty, nextDepth, result);
				}
			}
		}

		class Node
		{
			public Node Left { get; set; }
			public Node Right { get; set; }
			public (long x, long y, int i) Point { get; set; }

			public Node(Node left, Node right, (long x, long y, int i) point)
			{
				Left = left;
				Right = right;
				Point = point;
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class LaplacianMatrix
	{
		private readonly int _n;
		private readonly ModInt[,] _matrix;

		public ModInt this[int i, int j] => _matrix[i, j];

		public LaplacianMatrix(int n)
		{
			_n = n;
			_matrix = new ModInt[n, n];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge(int p, int q)
		{
			_matrix[p, p] += 1;
			_matrix[q, q] += 1;
			_matrix[p, q] -= 1;
			_matrix[q, p] -= 1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ModInt Determinant() => Determinant(_n);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ModInt CountSppanningTree() => KirchhoffTheorem();
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ModInt KirchhoffTheorem()
			=> _n > 1 ? Determinant(_n - 1) : 1;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ModInt Determinant(int n)
		{
			var temp = new ModInt[n, n];
			for (int i = 0; i < n; ++i) {
				for (int j = 0; j < n; ++j) {
					temp[i, j] = this[i, j];
				}
			}

			ModInt det = 1;
			ModInt m1 = new ModInt(-1, true);
			for (int i = 0; i < n; i++) {
				if (temp[i, i].ToLong() == 0) {
					for (int j = i + 1; j < n; j++) {
						if (temp[j, i].ToLong() != 0) {
							for (int k = 0; k < n; k++) {
								(temp[i, k], temp[j, k]) = (temp[j, k], temp[i, k]);
							}

							det *= m1;
							break;
						}
					}

					if (temp[i, i].ToLong() == 0) {
						return 0;
					}
				}

				det *= temp[i, i];
				var div = ModInt.Inverse(temp[i, i]);
				for (int j = 0; j < n; j++) {
					temp[i, j] *= div;
				}

				for (int j = i + 1; j < n; j++) {
					var mul = temp[j, i];
					for (int k = 0; k < n; k++) {
						temp[j, k] -= temp[i, k] * mul;
					}
				}
			}

			return det;
		}
	}
}

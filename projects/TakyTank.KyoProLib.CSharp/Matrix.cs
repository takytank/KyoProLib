using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class ModMatrix
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ModMatrix Identity(int n)
		{
			var ret = new ModMatrix(n);
			for (int i = 0; i < n; ++i) {
				ret[i, i] = 1;
			}

			return ret;
		}

		private readonly int height_;
		private readonly int width_;
		private readonly ModInt[,] matrix_;

		public ModInt this[int height, int width]
		{
			get => matrix_[height, width];
			set => matrix_[height, width] = value;
		}

		public ModMatrix(int size) : this(size, size) { }
		public ModMatrix(int height, int width)
		{
			height_ = height;
			width_ = width;
			matrix_ = new ModInt[height_, width_];
		}

		public ModMatrix(ModInt[,] matrix)
		{
			height_ = matrix.GetLength(0);
			width_ = matrix.GetLength(1);
			matrix_ = matrix;
		}

		public static ModMatrix operator +(ModMatrix lhs, ModMatrix rhs)
		{
			System.Diagnostics.Debug.Assert(lhs.height_ == rhs.height_);
			System.Diagnostics.Debug.Assert(lhs.width_ == rhs.width_);

			var ret = new ModMatrix(lhs.height_, rhs.width_);
			for (int i = 0; i < lhs.height_; ++i) {
				for (int j = 0; j < lhs.width_; ++j) {
					ret[i, j] = lhs[i, j] + rhs[i, j];
				}
			}

			return ret;
		}

		public static ModMatrix operator -(ModMatrix lhs, ModMatrix rhs)
		{
			System.Diagnostics.Debug.Assert(lhs.height_ == rhs.height_);
			System.Diagnostics.Debug.Assert(lhs.width_ == rhs.width_);

			var ret = new ModMatrix(lhs.height_, rhs.width_);
			for (int i = 0; i < lhs.height_; ++i) {
				for (int j = 0; j < lhs.width_; ++j) {
					ret[i, j] = lhs[i, j] - rhs[i, j];
				}
			}

			return ret;
		}

		public static ModMatrix operator *(ModMatrix lhs, ModMatrix rhs)
		{
			System.Diagnostics.Debug.Assert(lhs.width_ == rhs.height_);

			var ret = new ModMatrix(lhs.height_, rhs.width_);
			for (int i = 0; i < lhs.height_; ++i) {
				for (int j = 0; j < rhs.width_; ++j) {
					for (int k = 0; k < lhs.width_; ++k) {
						ret[i, j] += lhs[i, k] * rhs[k, j];
					}
				}
			}

			return ret;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ModMatrix Pow(long k)
		{
			System.Diagnostics.Debug.Assert(height_ == width_);

			var ret = Identity(height_);
			var mul = this;
			while (k > 0) {
				if ((k & 1) != 0) {
					ret *= mul;
				}

				mul *= mul;
				k >>= 1;
			}

			return ret;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ModInt Determinant()
		{
			System.Diagnostics.Debug.Assert(height_ == width_);

			int n = height_;
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int rank, ModInt[] answer) LinearEquation(
			ModMatrix a, ModInt[] b)
		{
			int n = a.height_;
			int m = a.width_;
			var matrix = new ModInt[n, m + 1];
			for (int i = 0; i < n; ++i) {
				for (int j = 0; j < m; ++j) {
					matrix[i, j] = a[i, j];
				}

				matrix[i, m] = b[i];
			}

			var (rank, _) = GaussJordan(matrix, true);

			for (int row = rank; row < n; ++row) {
				if (matrix[row, m].ToLong() != 0) {
					return (-1, null);
				}
			}

			var answer = new ModInt[n];
			for (int i = 0; i < rank; ++i) {
				answer[i] = matrix[i, m];
			}

			return (rank, answer);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private (int rank, ModInt[,] matrix) GaussJordan(
			ModInt[,] matrix, bool isExtendedCoefficientMatrix = false)
		{
			int n = matrix.GetLength(0);
			int m = matrix.GetLength(1);
			int rank = 0;
			for (int j = 0; j < m; ++j) {
				if (isExtendedCoefficientMatrix && j == m - 1) {
					break;
				}

				int pivot = -1;
				for (int i = rank; i < n; ++i) {
					if (matrix[i, j].ToLong() != 0) {
						pivot = i;
						break;
					}
				}

				if (pivot == -1) {
					continue;
				}

				for (int jj = 0; jj < m; jj++) {
					(matrix[pivot, jj], matrix[rank, jj]) = (matrix[rank, jj], matrix[pivot, jj]);
				}

				var inv = ModInt.Inverse(matrix[rank, j]);
				for (int jj = 0; jj < m; ++jj) {
					matrix[rank, jj] = matrix[rank, jj] * inv;
				}

				for (int i = 0; i < n; ++i) {
					if (i != rank && matrix[i, j].ToLong() > 0) {
						var fac = matrix[i, j];
						for (int jj = 0; jj < m; ++jj) {
							matrix[i, jj] -= matrix[rank, jj] * fac;
						}
					}
				}

				++rank;
			}

			return (rank, matrix);
		}
	}

	public class Matrix
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix Identity(int n)
		{
			var ret = new Matrix(n);
			for (int i = 0; i < n; ++i) {
				ret[i, i] = 1;
			}

			return ret;
		}

		private readonly int height_;
		private readonly int width_;
		private readonly Rational[,] matrix_;

		public Rational this[int height, int width]
		{
			get => matrix_[height, width];
			set => matrix_[height, width] = value;
		}

		public Matrix(int size) : this(size, size) { }
		public Matrix(int height, int width)
		{
			height_ = height;
			width_ = width;
			matrix_ = new Rational[height_, width_];
		}

		public Matrix(Rational[,] matrix)
		{
			height_ = matrix.GetLength(0);
			width_ = matrix.GetLength(1);
			matrix_ = matrix;
		}

		public static Matrix operator +(Matrix lhs, Matrix rhs)
		{
			System.Diagnostics.Debug.Assert(lhs.height_ == rhs.height_);
			System.Diagnostics.Debug.Assert(lhs.width_ == rhs.width_);

			var ret = new Matrix(lhs.height_, rhs.width_);
			for (int i = 0; i < lhs.height_; ++i) {
				for (int j = 0; j < lhs.width_; ++j) {
					ret[i, j] = lhs[i, j] + rhs[i, j];
				}
			}

			return ret;
		}

		public static Matrix operator -(Matrix lhs, Matrix rhs)
		{
			System.Diagnostics.Debug.Assert(lhs.height_ == rhs.height_);
			System.Diagnostics.Debug.Assert(lhs.width_ == rhs.width_);

			var ret = new Matrix(lhs.height_, rhs.width_);
			for (int i = 0; i < lhs.height_; ++i) {
				for (int j = 0; j < lhs.width_; ++j) {
					ret[i, j] = lhs[i, j] - rhs[i, j];
				}
			}

			return ret;
		}

		public static Matrix operator *(Matrix lhs, Matrix rhs)
		{
			System.Diagnostics.Debug.Assert(lhs.width_ == rhs.height_);

			var ret = new Matrix(lhs.height_, rhs.width_);
			for (int i = 0; i < lhs.height_; ++i) {
				for (int j = 0; j < rhs.width_; ++j) {
					for (int k = 0; k < lhs.width_; ++k) {
						ret[i, j] += lhs[i, k] * rhs[k, j];
					}
				}
			}

			return ret;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Matrix Pow(long k)
		{
			System.Diagnostics.Debug.Assert(height_ == width_);

			var ret = Identity(height_);
			var mul = this;
			while (k > 0) {
				if ((k & 1) != 0) {
					ret *= mul;
				}

				mul *= mul;
				k >>= 1;
			}

			return ret;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Rational Determinant()
		{
			System.Diagnostics.Debug.Assert(height_ == width_);

			int n = height_;
			var temp = new Rational[n, n];
			for (int i = 0; i < n; ++i) {
				for (int j = 0; j < n; ++j) {
					temp[i, j] = this[i, j];
				}
			}

			Rational det = 1;
			Rational m1 = -1;
			for (int i = 0; i < n; i++) {
				if (temp[i, i].IsZero) {
					for (int j = i + 1; j < n; j++) {
						if (temp[j, i].IsZero == false) {
							for (int k = 0; k < n; k++) {
								(temp[i, k], temp[j, k]) = (temp[j, k], temp[i, k]);
							}

							det *= m1;
							break;
						}
					}

					if (temp[i, i].IsZero) {
						return 0;
					}
				}

				det *= temp[i, i];
				var div = 1 / temp[i, i];
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int rank, Rational[] answer) LinearEquation(
			Matrix a, Rational[] b)
		{
			int n = a.height_;
			int m = a.width_;
			var matrix = new Rational[n, m + 1];
			for (int i = 0; i < n; ++i) {
				for (int j = 0; j < m; ++j) {
					matrix[i, j] = a[i, j];
				}

				matrix[i, m] = b[i];
			}

			var (rank, _) = GaussJordan(matrix, true);

			for (int row = rank; row < n; ++row) {
				if (matrix[row, m] != 0) {
					return (-1, null);
				}
			}

			var answer = new Rational[n];
			for (int i = 0; i < rank; ++i) {
				answer[i] = matrix[i, m];
			}

			return (rank, answer);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private (int rank, Rational[,] matrix) GaussJordan(
			Rational[,] matrix, bool isExtendedCoefficientMatrix = false)
		{
			int n = matrix.GetLength(0);
			int m = matrix.GetLength(1);
			int rank = 0;
			for (int j = 0; j < m; ++j) {
				if (isExtendedCoefficientMatrix && j == m - 1) {
					break;
				}

				int pivot = -1;
				for (int i = rank; i < n; ++i) {
					if (matrix[i, j] != 0) {
						pivot = i;
						break;
					}
				}

				if (pivot == -1) {
					continue;
				}

				for (int jj = 0; jj < m; jj++) {
					(matrix[pivot, jj], matrix[rank, jj]) = (matrix[rank, jj], matrix[pivot, jj]);
				}

				var inv = 1 / matrix[rank, j];
				for (int jj = 0; jj < m; ++jj) {
					matrix[rank, jj] = matrix[rank, jj] * inv;
				}

				for (int i = 0; i < n; ++i) {
					if (i != rank && matrix[i, j] > 0) {
						var fac = matrix[i, j];
						for (int jj = 0; jj < m; ++jj) {
							matrix[i, jj] -= matrix[rank, jj] * fac;
						}
					}
				}

				++rank;
			}

			return (rank, matrix);
		}
	}

	public class Matrix<T>
	{
		private readonly T delta0_;
		private readonly T delta1_;
		private readonly Func<T, T, T> add_;
		private readonly Func<T, T, T> mul_;

		public Matrix(T delta0, T delta1, Func<T, T, T> add, Func<T, T, T> mul)
		{
			delta0_ = delta0;
			delta1_ = delta1;
			add_ = add;
			mul_ = mul;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T[,] Identity(int n)
		{
			var ret = new T[n, n];
			MemoryMarshal.CreateSpan<T>(ref ret[0, 0], ret.Length).Fill(delta0_);
			for (int i = 0; i < n; i++) {
				ret[i, i] = delta1_;
			}

			return ret;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T[,] Multiply(T[,] a, T[,] b)
		{
			int rowA = a.GetLength(0);
			int colA = a.GetLength(1);
			int rowB = b.GetLength(0);
			int colB = b.GetLength(1);
			System.Diagnostics.Debug.Assert(colA == rowB);

			var ret = new T[rowA, colB];
			MemoryMarshal.CreateSpan<T>(ref ret[0, 0], ret.Length).Fill(delta0_);
			for (int i = 0; i < rowA; i++) {
				for (int j = 0; j < colB; j++) {
					for (int k = 0; k < colA; k++) {
						ret[i, j] = add_(ret[i, j], mul_(a[i, k], b[k, j]));
					}
				}
			}

			return ret;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T[,] Pow(T[,] a, long k)
		{
			var ret = Identity(a.GetLength(0));
			var mul = a;
			while (k > 0) {
				if ((k & 1) != 0) {
					ret = Multiply(ret, mul);
				}

				mul = Multiply(mul, mul);
				k >>= 1;
			}

			return ret;
		}
	}
}

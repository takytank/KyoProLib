using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
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

using System;

namespace TakyTank.KyoProLib.CSharp
{
	/// <summary>正規分布クラス</summary>
	public static class NormalDistribution
	{
		/// <summary>
		/// 平均mu、標準偏差sの正規分布の[-∞, x]の累積分布を計算
		/// </summary>
		public static double CalculateCumulativeDistribution(
			double mu, double s, double x)
		{
			// ロジスティック近似
			x = (x - mu) / s;
			return 1.0 / (1.0 + Math.Exp(-0.07056 * x * x * x - 1.5976 * x));
		}

		/// <summary>
		/// 平均mu、標準偏差sの正規分布の各整数値を取る確率を計算
		/// </summary>
		/// <remarks>
		/// 各インデックスに対し
		/// [0] : -0.5 ~ 0.5
		/// [1] : 0.5 ~ 1.5
		/// [2] : 1.5 ~ 2.5
		/// の用に幅1の区間を取る確率を計算する。
		/// 但し、配列の最後は、length - 1.5 <= x である確率が入る。 
		/// </remarks>
		public static double[] CalculateProbabilities(double mu, double s, int length)
		{
			var probability = new double[length];
			double last = CalculateCumulativeDistribution(mu, s, -0.5);
			for (int j = 0; j < length - 1; j++) {
				double x = j + 0.5;
				double temp = CalculateCumulativeDistribution(mu, s, x);
				probability[j] = temp - last;
				last = temp;
			}

			probability[length - 1] = 1.0 - last;

			return probability;
		}
	}
}

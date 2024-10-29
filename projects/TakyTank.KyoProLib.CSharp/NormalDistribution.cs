using System;

namespace TakyTank.KyoProLib.CSharp
{
	/// <summary>正規分布クラス</summary>
	public static class NormalDistribution
	{
		/// <summary>
		/// 平均mu、分散s2の標準正規分布の[-∞, x]の累積分布を計算
		/// </summary>
		public static double CalculateCumulativeDistribution(
			double mu, double s2, double x)
		{
			// ロジスティック近似
			x = (x - mu) / s2;
			return 1.0 / (1.0 + Math.Exp(-0.07056 * x * x * x - 1.5976 * x));
		}
	}
}

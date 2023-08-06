using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace TakyTank.KyoProLib.CSharp.V11
{
	public class HeuristicHelper
	{
		public static void RunCases<T>(
			int runCaseCount,
			double testCaseCount,
			bool isParallel,
			Func<int, T> run,
			Func<object, int, T, (long score, int loop, int up)> outputCaseInformation,
			Action addOutput = null)
		{
#if DEBUG
			object locker = new object();
			long scoreSum = 0;
			double scoreLogSum = 0;
			long loopSum = 0;
			long upSum = 0;
			long scoreMin = long.MaxValue;
			long scoreMax = long.MinValue;
			if (isParallel) {
				Parallel.For(0, runCaseCount, i => {
					RunCases(i);
				});
			} else {
				for (int i = 0; i < runCaseCount; i++) {
					RunCases(i);
				}
			}

			void RunCases(int i)
			{
				var ret = run(i);
				var (score, loop, up) = outputCaseInformation(locker, i, ret);
				Console.Out.Flush();
				lock (locker) {
					scoreSum += score;
					scoreLogSum += Math.Log10(score);
					loopSum += loop;
					upSum += up;
					scoreMin = Math.Min(score, scoreMin);
					scoreMax = Math.Max(score, scoreMax);
				}
			}

			scoreSum = (long)(scoreSum / (runCaseCount / testCaseCount));
			scoreLogSum /= runCaseCount / testCaseCount;
			Console.WriteLine("");

			Console.WriteLine("");
			Console.WriteLine($"sum: {scoreSum}");
			Console.WriteLine($"ave: {scoreSum / testCaseCount}");
			Console.WriteLine($"min: {scoreMin}");
			Console.WriteLine($"max: {scoreMax}");
			Console.WriteLine($"log: {scoreLogSum / testCaseCount}");
			Console.WriteLine($"loop ave.: {loopSum / (double)runCaseCount:f3}");
			Console.WriteLine($"up ave.: {upSum / (double)runCaseCount:f3}");

			addOutput?.Invoke();

			Console.Out.Flush();
#else
			run(-1);
#endif
		}

		public static (bool isDebug, Stopwatch sw, StringBuilder sb, Random rnd) Initialize()
		{
			var sw = new Stopwatch();
			sw.Start();

			bool isDebug = false;
#if DEBUG
			isDebug = true;
#endif
			var sb = new StringBuilder();
			var rnd = new Random();

			return (isDebug, sw, sb, rnd);
		}

		public static Scanner CreateScanner(
			string caseDirectory, int caseNumber)
		{
#if DEBUG
			var cin = caseNumber >= 0
				? new Scanner($"{caseDirectory}{caseNumber:d4}.txt")
				: new Scanner();
#else
			var cin = new Scanner();
#endif
			return cin;
		}
	}
}

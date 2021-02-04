using System;
using BenchmarkDotNet.Running;

namespace Benchmarker
{
	class Program
	{
		static void Main()
		{
			//BenchmarkRunner.Run<MeasureLightListFor>();
			BenchmarkRunner.Run<MeasureLightListAdd>();
		}
	}
}

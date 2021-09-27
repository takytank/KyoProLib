using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BenchmarkDotNet.Attributes;
using TakyTank.KyoProLib.CSharp;
using TakyTank.KyoProLib.CSharp.V8;

namespace Benchmarker
{
	public class MeasureLightListAdd
	{
		private readonly int[] src_;
		public MeasureLightListAdd()
		{
			src_ = Enumerable.Range(0, 100000000).ToArray();
		}

		[Benchmark]
		public void ListAdd()
		{
			var list = new List<int>();
			for (int i = 0; i < src_.Length; i++) {
				list.Add(src_[i]);
			}
		}

		[Benchmark]
		public void ListAddCapacity()
		{
			var list = new List<int>(src_.Length);
			for (int i = 0; i < src_.Length; i++) {
				list.Add(src_[i]);
			}
		}

		[Benchmark]
		public void LightListAdd()
		{
			var list = new LightList<int>();
			for (int i = 0; i < src_.Length; i++) {
				list.Add(src_[i]);
			}
		}

		[Benchmark]
		public void LightListAddCapacity()
		{
			var list = new LightList<int>(src_.Length);
			for (int i = 0; i < src_.Length; i++) {
				list.Add(src_[i]);
			}
		}
	}
}

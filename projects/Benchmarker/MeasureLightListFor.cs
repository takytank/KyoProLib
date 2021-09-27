using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BenchmarkDotNet.Attributes;
using TakyTank.KyoProLib.CSharp;
using TakyTank.KyoProLib.CSharp.V8;

namespace Benchmarker
{
	public class MeasureLightListFor
	{
		private readonly int[] array_;
		private readonly List<int> list_;
		private readonly LightList<int> lightlist_;
		public MeasureLightListFor()
		{
			array_ = Enumerable.Range(0, 500000000).ToArray();
			list_ = array_.ToList();
			lightlist_ = new LightList<int>(array_.Length);
			for (int i = 0; i < array_.Length; i++) {
				lightlist_.Add(array_[i]);
			}
		}

		[Benchmark]
		public long ArrayFor()
		{
			long sum = 0;
			for (int i = 0; i < array_.Length; i++) {
				sum += array_[i];
			}

			return sum;
		}

		[Benchmark]
		public long ArrayForEach()
		{
			long sum = 0;
			foreach (var value in array_) {
				sum += value;
			}

			return sum;
		}

		[Benchmark]
		public long ArrayForSpan()
		{
			long sum = 0;
			var span = array_.AsSpan();
			for (int i = 0; i < span.Length; i++) {
				sum += span[i];
			}

			return sum;
		}

		[Benchmark]
		public long ArrayForEachSpan()
		{
			long sum = 0;
			foreach (var value in array_.AsSpan()) {
				sum += value;
			}

			return sum;
		}

		[Benchmark]
		public long ListFor()
		{
			long sum = 0;
			for (int i = 0; i < list_.Count; i++) {
				sum += list_[i];
			}

			return sum;
		}

		[Benchmark]
		public long ListForEach()
		{
			long sum = 0;
			foreach (var value in list_) {
				sum += value;
			}

			return sum;
		}

		[Benchmark]
		public long ListForSpan()
		{
			long sum = 0;
			var span = list_.AsSpan();
			for (int i = 0; i < span.Length; i++) {
				sum += span[i];
			}

			return sum;
		}

		[Benchmark]
		public long ListForEachSpan()
		{
			long sum = 0;
			foreach (var value in list_.AsSpan()) {
				sum += value;
			}

			return sum;
		}

		[Benchmark]
		public long LightListFor()
		{
			long sum = 0;
			for (int i = 0; i < lightlist_.Count; i++) {
				sum += lightlist_[i];
			}

			return sum;
		}

		[Benchmark]
		public long LightListForRef()
		{
			long sum = 0;
			for (int i = 0; i < lightlist_.Count; i++) {
				sum += lightlist_.Ref(i);
			}

			return sum;
		}

		[Benchmark]
		public long LightListForSpan()
		{
			long sum = 0;
			var span = lightlist_.AsSpan();
			for (int i = 0; i < span.Length; i++) {
				sum += span[i];
			}

			return sum;
		}

		[Benchmark]
		public long LightListForEachSpan()
		{
			long sum = 0;
			foreach (var value in lightlist_.AsSpan()) {
				sum += value;
			}

			return sum;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	public class PrefixSum
	{
		private readonly int _n;
		private readonly long[] _sums;

		public long this[int index]
		{
			set => _sums[index + 1] = value;
			get => _sums[index + 1];
		}

		public PrefixSum(int n)
		{
			_n = n;
			_sums = new long[n + 1];
		}

		public PrefixSum(long[] values)
			: this(values.Length)
		{
			for (int i = 0; i < _n; i++) {
				_sums[i + 1] = values[i];
			}
		}

		public void Build()
		{
			for (int i = 0; i < _n; i++) {
				_sums[i + 1] += _sums[i];
			}
		}

		public long Sum(int l, int r)
		{
			return _sums[r] - _sums[l];
		}
	}
}

public class SuffixSum
{
	private readonly int _n;
	private readonly long[] _sums;

	public long this[int index]
	{
		set => _sums[index] = value;
		get => _sums[index];
	}

	public SuffixSum(int n)
	{
		_n = n;
		_sums = new long[n + 1];
	}

	public SuffixSum(long[] values)
		: this(values.Length)
	{
		for (int i = 0; i < _n; i++) {
			_sums[i] = values[i];
		}
	}

	public void Build()
	{
		for (int i = _n - 1; i >= 0; i--) {
			_sums[i] += _sums[i + 1];
		}
	}

	public long Sum(int l, int r)
	{
		return _sums[l] - _sums[r];
	}
}
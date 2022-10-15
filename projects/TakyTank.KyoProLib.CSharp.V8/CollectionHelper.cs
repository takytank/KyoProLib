using System;
using System.Collections.Generic;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V8
{
	public static class CollectionHelper
	{
		public static (bool founc, long majority) BoyerMoore(IReadOnlyList<long> values)
		{
			int count = 0;
			long majority = -1;
			foreach (var v in values) {
				if (count == 0) {
					majority = v;
					++count;
				} else if (majority == v) {
					++count;
				} else {
					--count;
				}
			}

			count = 0;
			foreach (var v in values) {
				if (v == majority) {
					++count;
				}
			}

			bool found = count > values.Count / 2;
			return (found, majority);
		}
	}
}

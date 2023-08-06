using System;
using System.Collections.Generic;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V11
{
	public class HashMap<TKey, TValue> : Dictionary<TKey, TValue>
	{
		public static HashMap<TKey, TValue> Merge(
			HashMap<TKey, TValue> src1,
			HashMap<TKey, TValue> src2,
			Func<TValue, TValue, TValue> mergeValues)
		{
			if (src1.Count < src2.Count) {
				(src1, src2) = (src2, src1);
			}

			foreach (var key in src2.Keys) {
				src1[key] = mergeValues(src1[key], src2[key]);
			}

			return src1;
		}

		private readonly Func<TKey, TValue> initialzier_;
		public HashMap(Func<TKey, TValue> initialzier)
			: base()
		{
			initialzier_ = initialzier;
		}

		public HashMap(Func<TKey, TValue> initialzier, int capacity)
			: base(capacity)
		{
			initialzier_ = initialzier;
		}

		new public TValue this[TKey key]
		{
			get
			{
				if (TryGetValue(key, out TValue value)) {
					return value;
				} else {
					var init = initialzier_(key);
					base[key] = init;
					return init;
				}
			}

			set { base[key] = value; }
		}

		public HashMap<TKey, TValue> Merge(
			HashMap<TKey, TValue> src,
			Func<TValue, TValue, TValue> mergeValues)
		{
			foreach (var key in src.Keys) {
				this[key] = mergeValues(this[key], src[key]);
			}

			return this;
		}
	}
}

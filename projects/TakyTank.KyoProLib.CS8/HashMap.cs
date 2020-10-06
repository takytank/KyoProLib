using System;
using System.Collections.Generic;
using System.Text;

namespace TakyTank.KyoProLib.CS8
{
	public class HashMap<TKey, TValue> : Dictionary<TKey, TValue>
	{
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
	}
}

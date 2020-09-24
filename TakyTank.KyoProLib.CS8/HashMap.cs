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
				if (ContainsKey(key) == false) {
					base[key] = initialzier_(key);
				}

				return base[key];
			}

			set { base[key] = value; }
		}
	}
}

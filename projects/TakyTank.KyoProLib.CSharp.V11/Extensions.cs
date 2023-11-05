using System.Runtime.CompilerServices;

namespace TakyTank.KyoProLib.CSharp.V11
{
	public static class Extensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<T> AsSpan<T>(this List<T> list)
		{
			return Unsafe.As<FakeList<T>>(list).Array.AsSpan(0, list.Count);
		}

		private class FakeList<T>
		{
			public T[] Array = null;
		}
	}
}

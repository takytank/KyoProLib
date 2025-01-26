using System.Diagnostics.CodeAnalysis;

namespace TakyTank.KyoProLib.CSharp.V11;

/// <summary>long用のIEqualityComarerの独自実装</summary>
/// <remarks>
/// デフォルト実装だとハッシュ衝突するケースで使うと良くなることがある。
/// </remarks>
public class LongComparer : IEqualityComparer<long>
{
	public bool Equals(long x, long y) => x.Equals(y);
	public int GetHashCode([DisallowNull] long obj)
		=> HashCode.Combine((uint)((obj >> 32) & 0xFFFFFFFF), (uint)(obj & 0xFFFFFFFF));
}

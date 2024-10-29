using System.Runtime.CompilerServices;

namespace TakyTank.KyoProLib.CSharp.V11;

public class Array2D<T>
{
	private readonly T[] _arr;
	public int H { get; }
	public int W { get; }

	public T this[int i]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => _arr[i];
		//get => Unsafe.Add(ref _arr[0], i * W + j);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set => _arr[i] = value;
	}

	public T this[int i, int j]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => _arr[i * W + j];
		//get => Unsafe.Add(ref _arr[0], i * W + j);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set => _arr[i * W + j] = value;
	}

	public Array2D(int h, int w)
	{
		H = h;
		W = w;
		_arr = new T[h * w];
	}

	public Array2D<T> Fill(T value)
	{
		_arr.AsSpan().Fill(value);
		return this;
	}

	public Span<T> Row(int i)
	{
		return _arr.AsSpan().Slice(i * W, W);
	}

	public Array2D<T> Clone()
	{
		var newArr = new Array2D<T>(H, W);
		Array.Copy(_arr, newArr._arr, _arr.Length);
		// Unsafe.CopyBlock(ref newArr._arr[0], ref _arr[0], (uint)_arr.Length);

		return newArr;
	}
}

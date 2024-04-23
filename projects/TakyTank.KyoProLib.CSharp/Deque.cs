using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp
{
	/// <summary>
	/// 両端キュークラス
	/// </summary>
	/// <typeparam name="T">キューの中身の型</typeparam>
	public class Deque<T> : IEnumerable<T>
	{
		/// <summary>リングバッファのサイズ</summary>
		private int _capacity;
		/// <summary>両端キューの実体であるリングバッファ</summary>
		private T[] _ringBuffer;
		/// <summary>キューの先頭のインデックス</summary>
		/// <remarks>
		/// キューに格納されている要素を閉区間[front, back]の範囲で持つ。
		/// 閉区間のため、中身が空のときはbackがfront-1の位置にある。
		/// </remarks>
		private int _front;
		/// <summary>キューの末尾のインデックス</summary>
		private int _back;

		/// <summary>格納されている要素数</summary>
		public int Count { get; private set; } = 0;
		/// <summary>キューの先頭の要素</summary>
		public T Front => _ringBuffer[_front];
		/// <summary>キューの末尾の要素</summary>
		public T Back => _ringBuffer[_back];

		/// <summary>ランダムアクセス</summary>
		/// <remarks>
		/// 範囲チェックは省いてあるので注意。
		/// </remarks>
		public T this[int index]
		{
			get
			{
				index += _front;
				// バッファの終端を超えた場合は先頭に戻る
				return _ringBuffer[index < _capacity ? index : index - _capacity];
			}
		}

		/// <summary>既定のサイズでインスタンスを生成</summary>
		public Deque() : this(1024) { }
		/// <summary>
		/// 指定したサイズでインスタンスを生成
		/// </summary>
		/// <param name="capacity">リングバッファのサイズ</param>
		public Deque(int capacity)
		{
			if (capacity <= 0) {
				capacity = 1024;
			}

			_capacity = capacity;
			_ringBuffer = new T[capacity];
			Clear();
		}

		/// <summary>
		/// キューの中身を空にする O(1)
		/// </summary>
		public void Clear()
		{
			_front = 0;
			_back = _capacity - 1;
			Count = 0;
			// バッファのクリアはしない。
			// 再度バッファにアクセスされる際には新しい値に上書きされるので問題無い。
		}

		/// <summary>
		/// キューの先頭に要素を追加する amortized O(1)
		/// </summary>
		/// <param name="item">追加する要素</param>
		public void PushFront(T item)
		{
			// リングバッファが一杯の場合、バッファサイズを倍に拡張する。
			// この部分がならしO(1)。
			if (Count == _capacity) {
				Extend(_capacity * 2);
			}

			// 要素が格納されている区間を前方向に拡張。
			// バッファの先頭を超えた場合は末尾に追加する。
			--_front;
			if (_front < 0) {
				_front += _capacity;
			}

			++Count;
			_ringBuffer[_front] = item;
		}

		/// <summary>
		/// キューの末尾に要素を追加する amortized O(1)
		/// </summary>
		/// <param name="item">追加する要素</param>
		public void PushBack(T item)
		{
			// リングバッファが一杯の場合、バッファサイズを倍に拡張する。
			// この部分がならしO(1)。
			if (Count == _capacity) {
				Extend(_capacity * 2);
			}

			// 要素が格納されている区間を後ろ方向に拡張。
			// バッファの末尾を超えた場合は先頭に追加する。
			++_back;
			if (_back >= _capacity) {
				_back -= _capacity;
			}

			++Count;
			_ringBuffer[_back] = item;
		}

		/// <summary>
		/// キューの先頭から要素を取り出す O(1)
		/// </summary>
		/// <returns>取り出した要素</returns>
		public T PopFront()
		{
			if (Count == 0) {
				// 例外を投げるべき？
				return default(T);
			}

			T ret = _ringBuffer[_front];
			--Count;
			// 要素が格納されている区間の先頭を縮小。
			// バッファの末尾を超えた場合は先頭を指すようにする。
			++_front;
			if (_front >= _capacity) {
				_front -= _capacity;
			}

			return ret;
		}

		/// <summary>
		/// キューの末尾から要素を取り出す O(1)
		/// </summary>
		/// <returns>取り出した要素</returns>
		public T PopBack()
		{
			if (Count == 0) {
				// 例外を投げるべき？
				return default(T);
			}

			T ret = _ringBuffer[_back];
			--Count;
			// 要素が格納されている区間の末尾を縮小。
			// バッファの先頭を超えた場合は末尾を指すようにする。
			--_back;
			if (_back < 0) {
				_back += _capacity;
			}

			return ret;
		}

		/// <summary>
		/// バッファサイズを倍に拡張
		/// </summary>
		/// <remarks>
		/// この処理自体はO(N)かかるのであるが、
		/// 呼ばれるタイイングがサイズNのバッファーが埋まったときなので、
		/// 実際に使われるケースでの計算量はならしO(1)となる。
		/// </remarks>
		/// <param name="newSize">拡張後のサイズ</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Extend(int newSize)
		{
			var newBuffer = new T[newSize];
			if (Count > 0) {
				if (_front <= _back) {
					// ここにくるのは実質frontが0でbackが^1のときだけ。
					Array.Copy(_ringBuffer, _front, newBuffer, 0, Count);
				} else {
					// [front, 末尾] と [先頭, back] の2回に分けてコピーする必要がある。
					int prevLength = _ringBuffer.Length - _front;
					Array.Copy(_ringBuffer, _front, newBuffer, 0, prevLength);
					Array.Copy(_ringBuffer, 0, newBuffer, prevLength, _back + 1);
				}
			}

			_ringBuffer = newBuffer;
			_capacity = newSize;
			_front = 0;
			_back = Count - 1;
		}

		public IEnumerator<T> GetEnumerator()
		{
			for (int i = 0; i < Count; i++) {
				yield return this[i];
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	/// <summary>
	/// 反転可能両端キュークラス
	/// </summary>
	/// <remarks>
	/// バッファの中身は変えずに、
	/// FrontとBackの役割を論理的に入れ替えることによって反転を実現する。
	/// </remarks>
	/// <typeparam name="T">キューの中身の型</typeparam>
	public class ReverseableDeque<T> : IEnumerable<T>
	{
		/// <summary>リングバッファのサイズ</summary>
		private int _capacity;
		/// <summary>両端キューの実体であるリングバッファ</summary>
		private T[] _ringBuffer;
		/// <summary>キューの先頭のインデックス</summary>
		/// <remarks>
		/// キューに格納されている要素を閉区間[front, back]の範囲で持つ。
		/// 閉区間のため、中身が空のときはbackがfront-1の位置にある。
		/// </remarks>
		private int _front;
		/// <summary>キューの末尾のインデックス</summary>
		private int _back;
		/// <summary>true: キューが反転状態</summary>
		private bool _reverses = false;

		/// <summary>格納されている要素数</summary>
		public int Count { get; private set; } = 0;
		/// <summary>キューの先頭の要素</summary>
		public T Front => _reverses == false ? _ringBuffer[_front] : _ringBuffer[_back];
		/// <summary>キューの末尾の要素</summary>
		public T Back => _reverses == false ? _ringBuffer[_back] : _ringBuffer[_front];

		/// <summary>ランダムアクセス</summary>
		/// <remarks>
		/// 範囲チェックは省いてあるので注意。
		/// </remarks>
		public T this[int index]
		{
			get
			{
				if (_reverses) {
					// 反転時は末尾から数える
					index = Count - 1 - index;
				}

				index += _front;
				// バッファの終端を超えた場合は先頭に戻る
				return _ringBuffer[index < _capacity ? index : index - _capacity];
			}
		}

		/// <summary>既定のサイズでインスタンスを生成</summary>
		public ReverseableDeque() : this(1024) { }
		/// <summary>
		/// 指定したサイズでインスタンスを生成
		/// </summary>
		/// <param name="capacity">リングバッファのサイズ</param>
		public ReverseableDeque(int capacity)
		{
			if (capacity <= 0) {
				capacity = 1024;
			}

			_capacity = capacity;
			_ringBuffer = new T[capacity];
			Clear();
		}

		/// <summary>
		/// キューを反転する O(1)
		/// </summary>
		public void Reverse()
		{
			_reverses = !_reverses;
		}

		/// <summary>
		/// キューの中身を空にする O(1)
		/// </summary>
		public void Clear()
		{
			_reverses = false;
			_front = 0;
			_back = _capacity - 1;
			Count = 0;
			// バッファのクリアはしない。
			// 再度バッファにアクセスされる際には新しい値に上書きされるので問題無い。
		}

		/// <summary>
		/// キューの先頭に要素を追加する amortized O(1)
		/// </summary>
		/// <param name="item">追加する要素</param>
		public void PushFront(T item)
		{
			// 反転時は末尾に対して追加すればよい
			if (_reverses == false) {
				PushFrontCore(item);
			} else {
				PushBackCore(item);
			}
		}

		/// <summary>
		/// キューの末尾に要素を追加する amortized O(1)
		/// </summary>
		/// <param name="item">追加する要素</param>
		public void PushBack(T item)
		{
			// 反転時は先頭に対して追加すればよい
			if (_reverses == false) {
				PushBackCore(item);
			} else {
				PushFrontCore(item);
			}
		}

		/// <summary>
		/// キューの先頭から要素を取り出す O(1)
		/// </summary>
		/// <returns>取り出した要素</returns>
		public T PopFront()
		{
			// 反転時は末尾から取り出せばよい
			if (_reverses == false) {
				return PopFrontCore();
			} else {
				return PopBackCore();
			}
		}

		/// <summary>
		/// キューの末尾から要素を取り出す O(1)
		/// </summary>
		/// <returns>取り出した要素</returns>
		public T PopBack()
		{
			// 反転時は先頭から取り出せばよい
			if (_reverses == false) {
				return PopBackCore();
			} else {
				return PopFrontCore();
			}
		}

		/// <summary>
		/// バッファの先頭に要素を追加する amortized O(1)
		/// </summary>
		/// <param name="item">追加する要素</param>
		private void PushFrontCore(T item)
		{
			// リングバッファが一杯の場合、バッファサイズを倍に拡張する。
			// この部分がならしO(1)。
			if (Count == _capacity) {
				Extend(_capacity * 2);
			}

			// 要素が格納されている区間を前方向に拡張。
			// バッファの先頭を超えた場合は末尾に追加する。
			--_front;
			if (_front < 0) {
				_front += _capacity;
			}

			++Count;
			_ringBuffer[_front] = item;
		}

		/// <summary>
		/// バッファの末尾に要素を追加する amortized O(1)
		/// </summary>
		/// <param name="item">追加する要素</param>
		private void PushBackCore(T item)
		{
			// リングバッファが一杯の場合、バッファサイズを倍に拡張する。
			// この部分がならしO(1)。
			if (Count == _capacity) {
				Extend(_capacity * 2);
			}

			// 要素が格納されている区間を後ろ方向に拡張。
			// バッファの末尾を超えた場合は先頭に追加する。
			++_back;
			if (_back >= _capacity) {
				_back -= _capacity;
			}

			++Count;
			_ringBuffer[_back] = item;
		}

		/// <summary>
		/// バッファの先頭から要素を取り出す O(1)
		/// </summary>
		/// <returns>取り出した要素</returns>
		private T PopFrontCore()
		{
			if (Count == 0) {
				// 例外を投げるべき？
				return default(T);
			}

			T ret = _ringBuffer[_front];
			--Count;
			// 要素が格納されている区間の先頭を縮小。
			// バッファの末尾を超えた場合は先頭を指すようにする。
			++_front;
			if (_front >= _capacity) {
				_front -= _capacity;
			}

			return ret;
		}

		/// <summary>
		/// バッファの末尾から要素を取り出す O(1)
		/// </summary>
		/// <returns>取り出した要素</returns>
		private T PopBackCore()
		{
			if (Count == 0) {
				// 例外を投げるべき？
				return default(T);
			}

			T ret = _ringBuffer[_back];
			--Count;
			// 要素が格納されている区間の末尾を縮小。
			// バッファの先頭を超えた場合は末尾を指すようにする。
			--_back;
			if (_back < 0) {
				_back += _capacity;
			}

			return ret;
		}

		/// <summary>
		/// バッファサイズを倍に拡張
		/// </summary>
		/// <remarks>
		/// この処理自体はO(N)かかるのであるが、
		/// 呼ばれるタイイングがサイズNのバッファーが埋まったときなので、
		/// 実際に使われるケースでの計算量はならしO(1)となる。
		/// </remarks>
		/// <param name="newSize">拡張後のサイズ</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Extend(int newSize)
		{
			var newBuffer = new T[newSize];
			if (Count > 0) {
				if (_front <= _back) {
					// ここにくるのは実質frontが0でbackが^1のときだけ。
					Array.Copy(_ringBuffer, _front, newBuffer, 0, Count);
				} else {
					// [front, 末尾] と [先頭, back] の2回に分けてコピーする必要がある。
					int prevLength = _ringBuffer.Length - _front;
					Array.Copy(_ringBuffer, _front, newBuffer, 0, prevLength);
					Array.Copy(_ringBuffer, 0, newBuffer, prevLength, _back + 1);
				}
			}

			_ringBuffer = newBuffer;
			_capacity = newSize;
			_front = 0;
			_back = Count - 1;
		}

		public IEnumerator<T> GetEnumerator()
		{
			for (int i = 0; i < Count; i++) {
				// インデクサ内でちゃんと処理をしているので反転時も順番に取り出せる。
				yield return this[i];
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}

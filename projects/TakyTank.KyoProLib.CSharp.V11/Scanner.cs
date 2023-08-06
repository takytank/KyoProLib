using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace TakyTank.KyoProLib.CSharp.V11
{
	public class Scanner : IDisposable
	{
		private const int BUFFER_SIZE = 1024;
		private const int ASCII_SPACE = 32;
		private const int ASCII_CHAR_BEGIN = 33;
		private const int ASCII_CHAR_END = 126;
		private readonly string filePath_;
		private readonly Stream stream_;
		private readonly byte[] buf_ = new byte[BUFFER_SIZE];
		private int length_ = 0;
		private int index_ = 0;
		private bool isEof_ = false;

		public Scanner(string file = "")
		{
			if (string.IsNullOrWhiteSpace(file)) {
				stream_ = Console.OpenStandardInput();
			} else {
				filePath_ = file;
				stream_ = new FileStream(file, FileMode.Open);
			}

			Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) {
				AutoFlush = false
			});
		}

		public void Dispose()
		{
			Console.Out.Flush();
			stream_.Dispose();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string NextLine()
		{
			var sb = new StringBuilder();
			for (var b = Char(); b >= ASCII_SPACE && b <= ASCII_CHAR_END; b = (char)Read()) {
				sb.Append(b);
			}

			return sb.ToString();
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public char Char()
		{
			byte b;
			do {
				b = Read();
			} while (b < ASCII_CHAR_BEGIN || ASCII_CHAR_END < b);

			return (char)b;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string String()
		{
			var sb = new StringBuilder();
			for (var b = Char(); b >= ASCII_CHAR_BEGIN && b <= ASCII_CHAR_END; b = (char)Read()) {
				sb.Append(b);
			}

			return sb.ToString();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string[] ArrayString(int length)
		{
			var array = new string[length];
			for (int i = 0; i < length; ++i) {
				array[i] = String();
			}

			return array;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Int() => (int)Long();
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Int(int offset) => Int() + offset;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int, int) Int2(int offset = 0)
			=> (Int(offset), Int(offset));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int, int, int) Int3(int offset = 0)
			=> (Int(offset), Int(offset), Int(offset));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int, int, int, int) Int4(int offset = 0)
			=> (Int(offset), Int(offset), Int(offset), Int(offset));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int, int, int, int, int) Int5(int offset = 0)
			=> (Int(offset), Int(offset), Int(offset), Int(offset), Int(offset));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int[] ArrayInt(int length, int offset = 0)
		{
			var array = new int[length];
			for (int i = 0; i < length; ++i) {
				array[i] = Int(offset);
			}

			return array;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long Long()
		{
			long ret = 0;
			byte b;
			bool ng = false;
			do {
				b = Read();
			} while (b != '-' && (b < '0' || '9' < b));

			if (b == '-') {
				ng = true;
				b = Read();
			}

			for (; true; b = Read()) {
				if (b < '0' || '9' < b) {
					return ng ? -ret : ret;
				} else {
					ret = ret * 10 + b - '0';
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long Long(long offset) => Long() + offset;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long, long) Long2(long offset = 0)
			=> (Long(offset), Long(offset));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long, long, long) Long3(long offset = 0)
			=> (Long(offset), Long(offset), Long(offset));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long, long, long, long) Long4(long offset = 0)
			=> (Long(offset), Long(offset), Long(offset), Long(offset));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long, long, long, long, long) Long5(long offset = 0)
			=> (Long(offset), Long(offset), Long(offset), Long(offset), Long(offset));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long[] ArrayLong(int length, long offset = 0)
		{
			var array = new long[length];
			for (int i = 0; i < length; ++i) {
				array[i] = Long(offset);
			}

			return array;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public BigInteger Big() => new BigInteger(Long());
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public BigInteger Big(long offset) => Big() + offset;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (BigInteger, BigInteger) Big2(long offset = 0)
			=> (Big(offset), Big(offset));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (BigInteger, BigInteger, BigInteger) Big3(long offset = 0)
			=> (Big(offset), Big(offset), Big(offset));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (BigInteger, BigInteger, BigInteger, BigInteger) Big4(long offset = 0)
			=> (Big(offset), Big(offset), Big(offset), Big(offset));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (BigInteger, BigInteger, BigInteger, BigInteger, BigInteger) Big5(long offset = 0)
			=> (Big(offset), Big(offset), Big(offset), Big(offset), Big(offset));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public BigInteger[] ArrayBig(int length, long offset = 0)
		{
			var array = new BigInteger[length];
			for (int i = 0; i < length; ++i) {
				array[i] = Big(offset);
			}

			return array;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double Double() => double.Parse(String(), CultureInfo.InvariantCulture);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double Double(double offset) => Double() + offset;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (double, double) Double2(double offset = 0)
			=> (Double(offset), Double(offset));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (double, double, double) Double3(double offset = 0)
			=> (Double(offset), Double(offset), Double(offset));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (double, double, double, double) Double4(double offset = 0)
			=> (Double(offset), Double(offset), Double(offset), Double(offset));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (double, double, double, double, double) Double5(double offset = 0)
			=> (Double(offset), Double(offset), Double(offset), Double(offset), Double(offset));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double[] ArrayDouble(int length, double offset = 0)
		{
			var array = new double[length];
			for (int i = 0; i < length; ++i) {
				array[i] = Double(offset);
			}

			return array;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public decimal Decimal() => decimal.Parse(String(), CultureInfo.InvariantCulture);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public decimal Decimal(decimal offset) => Decimal() + offset;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (decimal, decimal) Decimal2(decimal offset = 0)
			=> (Decimal(offset), Decimal(offset));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (decimal, decimal, decimal) Decimal3(decimal offset = 0)
			=> (Decimal(offset), Decimal(offset), Decimal(offset));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (decimal, decimal, decimal, decimal) Decimal4(decimal offset = 0)
			=> (Decimal(offset), Decimal(offset), Decimal(offset), Decimal(offset));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (decimal, decimal, decimal, decimal, decimal) Decimal5(decimal offset = 0)
			=> (Decimal(offset), Decimal(offset), Decimal(offset), Decimal(offset), Decimal(offset));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public decimal[] ArrayDecimal(int length, decimal offset = 0)
		{
			var array = new decimal[length];
			for (int i = 0; i < length; ++i) {
				array[i] = Decimal(offset);
			}

			return array;
		}

		private byte Read()
		{
			if (isEof_) {
				throw new EndOfStreamException();
			}

			if (index_ >= length_) {
				index_ = 0;
				if ((length_ = stream_.Read(buf_, 0, BUFFER_SIZE)) <= 0) {
					isEof_ = true;
					return 0;
				}
			}

			return buf_[index_++];
		}

		public void Save(string text)
		{
			if (string.IsNullOrWhiteSpace(filePath_)) {
				return;
			}

			File.WriteAllText(filePath_ + "_output.txt", text);
		}
	}
}

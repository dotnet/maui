using System;

namespace Xamarin.Forms.Platform.WinPhone
{
	internal sealed class MD5CryptoServiceProvider : MD5
	{
		const int BlockSizeBytes = 64;

		static readonly uint[] K =
		{
			0xd76aa478, 0xe8c7b756, 0x242070db, 0xc1bdceee, 0xf57c0faf, 0x4787c62a, 0xa8304613, 0xfd469501, 0x698098d8, 0x8b44f7af, 0xffff5bb1, 0x895cd7be, 0x6b901122,
			0xfd987193, 0xa679438e, 0x49b40821, 0xf61e2562, 0xc040b340, 0x265e5a51, 0xe9b6c7aa, 0xd62f105d, 0x02441453, 0xd8a1e681, 0xe7d3fbc8, 0x21e1cde6, 0xc33707d6, 0xf4d50d87, 0x455a14ed, 0xa9e3e905,
			0xfcefa3f8, 0x676f02d9, 0x8d2a4c8a, 0xfffa3942, 0x8771f681, 0x6d9d6122, 0xfde5380c, 0xa4beea44, 0x4bdecfa9, 0xf6bb4b60, 0xbebfbc70, 0x289b7ec6, 0xeaa127fa, 0xd4ef3085, 0x04881d05, 0xd9d4d039,
			0xe6db99e5, 0x1fa27cf8, 0xc4ac5665, 0xf4292244, 0x432aff97, 0xab9423a7, 0xfc93a039, 0x655b59c3, 0x8f0ccc92, 0xffeff47d, 0x85845dd1, 0x6fa87e4f, 0xfe2ce6e0, 0xa3014314, 0x4e0811a1, 0xf7537e82,
			0xbd3af235, 0x2ad7d2bb, 0xeb86d391
		};

		uint[] _buff;
		ulong _count;
		uint[] _h;
		byte[] _processingBuffer; // Used to start data when passed less than a block worth.
		int _processingBufferCount; // Counts how much data we have stored that still needs processed.

		public MD5CryptoServiceProvider()
		{
			_h = new uint[4];
			_buff = new uint[16];
			_processingBuffer = new byte[BlockSizeBytes];

			Initialize();
		}

		public override void Initialize()
		{
			_count = 0;
			_processingBufferCount = 0;

			_h[0] = 0x67452301;
			_h[1] = 0xefcdab89;
			_h[2] = 0x98badcfe;
			_h[3] = 0x10325476;
		}

		protected override void Dispose(bool disposing)
		{
			if (_processingBuffer != null)
			{
				Array.Clear(_processingBuffer, 0, _processingBuffer.Length);
				_processingBuffer = null;
			}
			if (_h != null)
			{
				Array.Clear(_h, 0, _h.Length);
				_h = null;
			}
			if (_buff != null)
			{
				Array.Clear(_buff, 0, _buff.Length);
				_buff = null;
			}
		}

		protected override void HashCore(byte[] rgb, int ibStart, int cbSize)
		{
			int i;
			State = 1;

			if (_processingBufferCount != 0)
			{
				if (cbSize < BlockSizeBytes - _processingBufferCount)
				{
					Buffer.BlockCopy(rgb, ibStart, _processingBuffer, _processingBufferCount, cbSize);
					_processingBufferCount += cbSize;
					return;
				}
				i = BlockSizeBytes - _processingBufferCount;
				Buffer.BlockCopy(rgb, ibStart, _processingBuffer, _processingBufferCount, i);
				ProcessBlock(_processingBuffer, 0);
				_processingBufferCount = 0;
				ibStart += i;
				cbSize -= i;
			}

			for (i = 0; i < cbSize - cbSize % BlockSizeBytes; i += BlockSizeBytes)
				ProcessBlock(rgb, ibStart + i);

			if (cbSize % BlockSizeBytes != 0)
			{
				Buffer.BlockCopy(rgb, cbSize - cbSize % BlockSizeBytes + ibStart, _processingBuffer, 0, cbSize % BlockSizeBytes);
				_processingBufferCount = cbSize % BlockSizeBytes;
			}
		}

		protected override byte[] HashFinal()
		{
			var hash = new byte[16];
			int i, j;

			ProcessFinalBlock(_processingBuffer, 0, _processingBufferCount);

			for (i = 0; i < 4; i++)
			{
				for (j = 0; j < 4; j++)
					hash[i * 4 + j] = (byte)(_h[i] >> j * 8);
			}

			return hash;
		}

		internal void AddLength(ulong length, byte[] buffer, int position)
		{
			buffer[position++] = (byte)length;
			buffer[position++] = (byte)(length >> 8);
			buffer[position++] = (byte)(length >> 16);
			buffer[position++] = (byte)(length >> 24);
			buffer[position++] = (byte)(length >> 32);
			buffer[position++] = (byte)(length >> 40);
			buffer[position++] = (byte)(length >> 48);
			buffer[position] = (byte)(length >> 56);
		}

		void ProcessBlock(byte[] inputBuffer, int inputOffset)
		{
			uint a, b, c, d;
			int i;

			_count += BlockSizeBytes;

			for (i = 0; i < 16; i++)
			{
				_buff[i] = inputBuffer[inputOffset + 4 * i] | ((uint)inputBuffer[inputOffset + 4 * i + 1] << 8) | ((uint)inputBuffer[inputOffset + 4 * i + 2] << 16) |
						   ((uint)inputBuffer[inputOffset + 4 * i + 3] << 24);
			}

			a = _h[0];
			b = _h[1];
			c = _h[2];
			d = _h[3];

			// This function was unrolled because it seems to be doubling our performance with current compiler/VM.
			// Possibly roll up if this changes.

			// ---- Round 1 --------

			a += (((c ^ d) & b) ^ d) + K[0] + _buff[0];
			a = (a << 7) | (a >> 25);
			a += b;

			d += (((b ^ c) & a) ^ c) + K[1] + _buff[1];
			d = (d << 12) | (d >> 20);
			d += a;

			c += (((a ^ b) & d) ^ b) + K[2] + _buff[2];
			c = (c << 17) | (c >> 15);
			c += d;

			b += (((d ^ a) & c) ^ a) + K[3] + _buff[3];
			b = (b << 22) | (b >> 10);
			b += c;

			a += (((c ^ d) & b) ^ d) + K[4] + _buff[4];
			a = (a << 7) | (a >> 25);
			a += b;

			d += (((b ^ c) & a) ^ c) + K[5] + _buff[5];
			d = (d << 12) | (d >> 20);
			d += a;

			c += (((a ^ b) & d) ^ b) + K[6] + _buff[6];
			c = (c << 17) | (c >> 15);
			c += d;

			b += (((d ^ a) & c) ^ a) + K[7] + _buff[7];
			b = (b << 22) | (b >> 10);
			b += c;

			a += (((c ^ d) & b) ^ d) + K[8] + _buff[8];
			a = (a << 7) | (a >> 25);
			a += b;

			d += (((b ^ c) & a) ^ c) + K[9] + _buff[9];
			d = (d << 12) | (d >> 20);
			d += a;

			c += (((a ^ b) & d) ^ b) + K[10] + _buff[10];
			c = (c << 17) | (c >> 15);
			c += d;

			b += (((d ^ a) & c) ^ a) + K[11] + _buff[11];
			b = (b << 22) | (b >> 10);
			b += c;

			a += (((c ^ d) & b) ^ d) + K[12] + _buff[12];
			a = (a << 7) | (a >> 25);
			a += b;

			d += (((b ^ c) & a) ^ c) + K[13] + _buff[13];
			d = (d << 12) | (d >> 20);
			d += a;

			c += (((a ^ b) & d) ^ b) + K[14] + _buff[14];
			c = (c << 17) | (c >> 15);
			c += d;

			b += (((d ^ a) & c) ^ a) + K[15] + _buff[15];
			b = (b << 22) | (b >> 10);
			b += c;

			// ---- Round 2 --------

			a += (((b ^ c) & d) ^ c) + K[16] + _buff[1];
			a = (a << 5) | (a >> 27);
			a += b;

			d += (((a ^ b) & c) ^ b) + K[17] + _buff[6];
			d = (d << 9) | (d >> 23);
			d += a;

			c += (((d ^ a) & b) ^ a) + K[18] + _buff[11];
			c = (c << 14) | (c >> 18);
			c += d;

			b += (((c ^ d) & a) ^ d) + K[19] + _buff[0];
			b = (b << 20) | (b >> 12);
			b += c;

			a += (((b ^ c) & d) ^ c) + K[20] + _buff[5];
			a = (a << 5) | (a >> 27);
			a += b;

			d += (((a ^ b) & c) ^ b) + K[21] + _buff[10];
			d = (d << 9) | (d >> 23);
			d += a;

			c += (((d ^ a) & b) ^ a) + K[22] + _buff[15];
			c = (c << 14) | (c >> 18);
			c += d;

			b += (((c ^ d) & a) ^ d) + K[23] + _buff[4];
			b = (b << 20) | (b >> 12);
			b += c;

			a += (((b ^ c) & d) ^ c) + K[24] + _buff[9];
			a = (a << 5) | (a >> 27);
			a += b;

			d += (((a ^ b) & c) ^ b) + K[25] + _buff[14];
			d = (d << 9) | (d >> 23);
			d += a;

			c += (((d ^ a) & b) ^ a) + K[26] + _buff[3];
			c = (c << 14) | (c >> 18);
			c += d;

			b += (((c ^ d) & a) ^ d) + K[27] + _buff[8];
			b = (b << 20) | (b >> 12);
			b += c;

			a += (((b ^ c) & d) ^ c) + K[28] + _buff[13];
			a = (a << 5) | (a >> 27);
			a += b;

			d += (((a ^ b) & c) ^ b) + K[29] + _buff[2];
			d = (d << 9) | (d >> 23);
			d += a;

			c += (((d ^ a) & b) ^ a) + K[30] + _buff[7];
			c = (c << 14) | (c >> 18);
			c += d;

			b += (((c ^ d) & a) ^ d) + K[31] + _buff[12];
			b = (b << 20) | (b >> 12);
			b += c;

			// ---- Round 3 --------

			a += (b ^ c ^ d) + K[32] + _buff[5];
			a = (a << 4) | (a >> 28);
			a += b;

			d += (a ^ b ^ c) + K[33] + _buff[8];
			d = (d << 11) | (d >> 21);
			d += a;

			c += (d ^ a ^ b) + K[34] + _buff[11];
			c = (c << 16) | (c >> 16);
			c += d;

			b += (c ^ d ^ a) + K[35] + _buff[14];
			b = (b << 23) | (b >> 9);
			b += c;

			a += (b ^ c ^ d) + K[36] + _buff[1];
			a = (a << 4) | (a >> 28);
			a += b;

			d += (a ^ b ^ c) + K[37] + _buff[4];
			d = (d << 11) | (d >> 21);
			d += a;

			c += (d ^ a ^ b) + K[38] + _buff[7];
			c = (c << 16) | (c >> 16);
			c += d;

			b += (c ^ d ^ a) + K[39] + _buff[10];
			b = (b << 23) | (b >> 9);
			b += c;

			a += (b ^ c ^ d) + K[40] + _buff[13];
			a = (a << 4) | (a >> 28);
			a += b;

			d += (a ^ b ^ c) + K[41] + _buff[0];
			d = (d << 11) | (d >> 21);
			d += a;

			c += (d ^ a ^ b) + K[42] + _buff[3];
			c = (c << 16) | (c >> 16);
			c += d;

			b += (c ^ d ^ a) + K[43] + _buff[6];
			b = (b << 23) | (b >> 9);
			b += c;

			a += (b ^ c ^ d) + K[44] + _buff[9];
			a = (a << 4) | (a >> 28);
			a += b;

			d += (a ^ b ^ c) + K[45] + _buff[12];
			d = (d << 11) | (d >> 21);
			d += a;

			c += (d ^ a ^ b) + K[46] + _buff[15];
			c = (c << 16) | (c >> 16);
			c += d;

			b += (c ^ d ^ a) + K[47] + _buff[2];
			b = (b << 23) | (b >> 9);
			b += c;

			// ---- Round 4 --------

			a += ((~d | b) ^ c) + K[48] + _buff[0];
			a = (a << 6) | (a >> 26);
			a += b;

			d += ((~c | a) ^ b) + K[49] + _buff[7];
			d = (d << 10) | (d >> 22);
			d += a;

			c += ((~b | d) ^ a) + K[50] + _buff[14];
			c = (c << 15) | (c >> 17);
			c += d;

			b += ((~a | c) ^ d) + K[51] + _buff[5];
			b = (b << 21) | (b >> 11);
			b += c;

			a += ((~d | b) ^ c) + K[52] + _buff[12];
			a = (a << 6) | (a >> 26);
			a += b;

			d += ((~c | a) ^ b) + K[53] + _buff[3];
			d = (d << 10) | (d >> 22);
			d += a;

			c += ((~b | d) ^ a) + K[54] + _buff[10];
			c = (c << 15) | (c >> 17);
			c += d;

			b += ((~a | c) ^ d) + K[55] + _buff[1];
			b = (b << 21) | (b >> 11);
			b += c;

			a += ((~d | b) ^ c) + K[56] + _buff[8];
			a = (a << 6) | (a >> 26);
			a += b;

			d += ((~c | a) ^ b) + K[57] + _buff[15];
			d = (d << 10) | (d >> 22);
			d += a;

			c += ((~b | d) ^ a) + K[58] + _buff[6];
			c = (c << 15) | (c >> 17);
			c += d;

			b += ((~a | c) ^ d) + K[59] + _buff[13];
			b = (b << 21) | (b >> 11);
			b += c;

			a += ((~d | b) ^ c) + K[60] + _buff[4];
			a = (a << 6) | (a >> 26);
			a += b;

			d += ((~c | a) ^ b) + K[61] + _buff[11];
			d = (d << 10) | (d >> 22);
			d += a;

			c += ((~b | d) ^ a) + K[62] + _buff[2];
			c = (c << 15) | (c >> 17);
			c += d;

			b += ((~a | c) ^ d) + K[63] + _buff[9];
			b = (b << 21) | (b >> 11);
			b += c;

			_h[0] += a;
			_h[1] += b;
			_h[2] += c;
			_h[3] += d;
		}

		void ProcessFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
		{
			ulong total = _count + (ulong)inputCount;
			var paddingSize = (int)(56 - total % BlockSizeBytes);

			if (paddingSize < 1)
				paddingSize += BlockSizeBytes;

			var fooBuffer = new byte[inputCount + paddingSize + 8];

			for (var i = 0; i < inputCount; i++)
				fooBuffer[i] = inputBuffer[i + inputOffset];

			fooBuffer[inputCount] = 0x80;
			for (int i = inputCount + 1; i < inputCount + paddingSize; i++)
				fooBuffer[i] = 0x00;

			// I deal in bytes. The algorithm deals in bits.
			ulong size = total << 3;
			AddLength(size, fooBuffer, inputCount + paddingSize);
			ProcessBlock(fooBuffer, 0);

			if (inputCount + paddingSize + 8 == 128)
				ProcessBlock(fooBuffer, 64);
		}

		~MD5CryptoServiceProvider()
		{
			Dispose(false);
		}
	}
}
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.Internals
{
	public class GIFDecoderFormatException : Exception
	{
		public GIFDecoderFormatException()
		{
			;
		}

		public GIFDecoderFormatException(string message) : base(message)
		{
			;
		}

		public GIFDecoderFormatException(string message, Exception innerException) : base(message, innerException)
		{
			;
		}
	}

	public class GIFDecoderStreamReader
	{
		Stream _stream;
		long _currentPosition;
		int _currentBlockSize;
		byte[] _blockBuffer = new byte[256];

		public GIFDecoderStreamReader(Stream stream)
		{
			_stream = stream;
		}

		public long CurrentPosition
		{
			get
			{
#if DEBUG
				if (_stream.CanSeek)
					System.Diagnostics.Debug.Assert(_stream.Position == _currentPosition);
#endif
				return _currentPosition;
			}
		}

		public byte[] CurrentBlockBuffer
		{
			get { return _blockBuffer; }
		}

		public int CurrentBlockSize
		{
			get { return _currentBlockSize; }
		}

		public int Read()
		{
			_currentPosition++;
			return _stream.ReadByte();
		}

		public int ReadShort()
		{
			return Read() | (Read() << 8);
		}

		public string ReadString(int length)
		{
			var buffer = new StringBuilder(length);
			for (int i = 0; i < length; i++)
				buffer.Append((char)_stream.ReadByte());

			_currentPosition += length;
			return buffer.ToString();
		}

		public async Task<int> ReadAsync(byte[] buffer, int toRead)
		{
			int totalBytesRead = 0;
			if (toRead > 0)
			{
				Debug.Assert(toRead <= buffer.Length);

				int bytesRead = 0;
				while (totalBytesRead < toRead)
				{
					bytesRead = await _stream.ReadAsync(buffer, totalBytesRead, toRead - totalBytesRead);
					if (bytesRead == -1)
					{
						break;
					}
					totalBytesRead += bytesRead;
				}
			}

			_currentPosition += totalBytesRead;
			return totalBytesRead;
		}

		public async Task<int> ReadBlockAsync()
		{
			_currentBlockSize = Read();
			int bytesRead = await ReadAsync(_blockBuffer, _currentBlockSize).ConfigureAwait(false);

			if (bytesRead < _currentBlockSize)
			{
				throw new GIFDecoderFormatException("Current block to small.");
			}

			Debug.Assert(_currentBlockSize == bytesRead);
			return bytesRead;
		}

		public async Task SkipBlockAsync()
		{
			_currentBlockSize = Read();
			while (_currentBlockSize > 0)
			{
				if (_stream.CanSeek)
				{
					_stream.Seek(_currentBlockSize, SeekOrigin.Current);
					_currentPosition += _currentBlockSize;
				}
				else
				{
					await ReadAsync(_blockBuffer, _currentBlockSize).ConfigureAwait(false);
				}

				_currentBlockSize = Read();
			}
		}
	}

	public class GIFColorTable
	{
		int[] _colorTable = new int[256];
		byte[] _colorData = null;
		short _size = 0;
		int _transparencyIndex = -1;
		int _oldColorValue = -1;

		GIFColorTable(short size)
		{
			// Each color uses 3 bytes.
			_colorData = new byte[3 * size];
			_size = size;
		}

		public int[] Data
		{
			get { return _colorTable; }
		}

		public void SetTransparency(int transparencyIndex)
		{
			Debug.Assert(transparencyIndex < _colorTable.Length);

			ResetTransparency();

			_oldColorValue = _colorTable[transparencyIndex];
			_colorTable[transparencyIndex] = 0;
			_transparencyIndex = transparencyIndex;
		}

		public void ResetTransparency()
		{
			if (_transparencyIndex != -1)
			{
				_colorTable[_transparencyIndex] = _oldColorValue;
				_transparencyIndex = _oldColorValue = -1;
			}
		}

		public static async Task<GIFColorTable> CreateColorTableAsync(GIFDecoderStreamReader stream, short size)
		{
			var colorTable = new GIFColorTable(size);
			await colorTable.ParseAsync(stream).ConfigureAwait(false);
			return colorTable;
		}

		async Task ParseAsync(GIFDecoderStreamReader stream)
		{
			int toRead = _colorData.Length;
			int bytesRead = await stream.ReadAsync(_colorData, toRead).ConfigureAwait(false);
			if (bytesRead < toRead)
				throw new GIFDecoderFormatException("Invalid color table size.");

			int currentColor = 0;
			int currentColorData = 0;
			while (currentColor < _size)
			{
				int r = _colorData[currentColorData++];
				int g = _colorData[currentColorData++];
				int b = _colorData[currentColorData++];

				var rgb = (r << 16) | (g << 8) | b;
				_colorTable[currentColor++] = (int)(0xFF000000 | rgb);
			}
		}
	}

	public class GIFHeader
	{
		GIFHeader()
		{
			;
		}

		public string TypeIdentifier { get; private set; }

		public string Version { get; private set; }

		public int Width { get; private set; }

		public int Height { get; private set; }

		public int BackgroundColorIndex { get; private set; }

		public int BackgroundColor { get; private set; }

		public GIFColorTable GlobalColorTable { get; private set; }

		public int PixelAspectRatio { get; private set; }

		public bool IsGIFHeader
		{
			get
			{
				return !string.IsNullOrEmpty(TypeIdentifier) && TypeIdentifier.StartsWith("GIF", StringComparison.OrdinalIgnoreCase);
			}
		}

		public static async Task<GIFHeader> CreateHeaderAsync(GIFDecoderStreamReader stream, bool skipTypeIdentifier = false)
		{
			GIFHeader header = new GIFHeader();
			await header.ParseAsync(stream, skipTypeIdentifier).ConfigureAwait(false);
			if (!header.IsGIFHeader)
				header = null;

			return header;
		}

		bool UseGlobalColorTable(int flags)
		{
			return ((flags & 0x80) != 0);
		}

		short GlobalColorTableSize(int flags)
		{
			return (short)(2 << (flags & 7));
		}

		async Task ParseAsync(GIFDecoderStreamReader stream, bool skipTypeIdentifier)
		{
			if (!skipTypeIdentifier)
				TypeIdentifier = stream.ReadString(3);
			else
				TypeIdentifier = "GIF";

			if (IsGIFHeader)
			{
				Version = stream.ReadString(3);
				Width = stream.ReadShort();
				Height = stream.ReadShort();

				int flags = stream.Read();
				BackgroundColorIndex = stream.Read();
				PixelAspectRatio = stream.Read();

				if (UseGlobalColorTable(flags))
				{
					GlobalColorTable = await GIFColorTable.CreateColorTableAsync(stream, GlobalColorTableSize(flags)).ConfigureAwait(false);
					BackgroundColor = GlobalColorTable.Data[BackgroundColorIndex];
				}
			}
			else
			{
				throw new GIFDecoderFormatException("Unknown GIF format type identifier: " + TypeIdentifier);
			}
		}
	}

	public class GIFBitmap
	{
		public enum DisposeMethod
		{
			NoAction = 0,
			LeaveInPlace = 1,
			RestoreToBackground = 2,
			RestoreToPrevious = 3
		};

		public class Rect
		{
			public Rect(int x, int y, int width, int height)
			{
				X = x;
				Y = y;
				Width = width;
				Height = height;
			}

			public int X { get; }

			public int Y { get; }

			public int Width { get; }

			public int Height { get; }
		}

		public int[] Data { get; set; }

		public long DataPosition { get; private set; }

		public GIFBitmap.Rect Bounds { get; private set; }

		public DisposeMethod Dispose { get; private set; }

		public int BackgroundColor { get; private set; }

		public bool IsTransparent { get; private set; }

		public int TransparencyIndex { get; private set; }

		public bool IsInterlaced { get; private set; }

		public int Delay { get; private set; }

		public int LoopCount { get; private set; }

		public GIFColorTable ColorTable { get; private set; }

		class GIFBlockCodes
		{
			public const int ImageSeparator = 0x2C;
			public const int Extension = 0x21;
			public const int Trailer = 0x3B;
			public const int GraphicsControlExtension = 0xF9;
			public const int ApplicationExtensionBlock = 0xFF;
		}

		const string NetscapeApplicationExtensionID = "NETSCAPE2.0";

		GIFHeader _header;

		GIFBitmap(GIFHeader header)
		{
			_header = header;
			LoopCount = 0;
			Delay = 10;
		}

		void SetDisposeMethod(int flags)
		{
			Dispose = (DisposeMethod)((flags & 0x1C) >> 2);
			if (Dispose == DisposeMethod.NoAction)
				Dispose = DisposeMethod.LeaveInPlace;
		}

		void SetTransparency(int flags, int index)
		{
			IsTransparent = (flags & 1) != 0;
			TransparencyIndex = index;
		}

		void SetDelay(int delay)
		{
			// Convert to milliseconds.
			Delay = Math.Max(10, delay * 10);
		}

		bool UseLocalColorTable(int flags)
		{
			return ((flags & 0x80) != 0);
		}

		short LocalColorTableSize(int flags)
		{
			return (short)(Math.Pow(2, (flags & 0x07) + 1));
		}

		bool UseInterlace(int flags)
		{
			return ((flags & 0x40) != 0);
		}

		void ParseGraphicControlExtension(GIFDecoderStreamReader stream)
		{
			int blockSize = stream.Read();
			if (blockSize != 4)
				throw new GIFDecoderFormatException("Invalid graphic control extension size.");

			int flags = stream.Read();
			SetDisposeMethod(flags);
			SetDelay(stream.ReadShort());
			SetTransparency(flags, stream.Read());

			// Consume block terminator.
			stream.Read();
		}

		async Task ParseNetscapeApplicationExtensionAsync(GIFDecoderStreamReader stream)
		{
			int blockSize = await stream.ReadBlockAsync().ConfigureAwait(false);
			while (blockSize > 0)
			{
				if (stream.CurrentBlockBuffer[0] == 1 && blockSize == 3)
				{
					int count = (stream.CurrentBlockBuffer[2] << 8) | stream.CurrentBlockBuffer[1];

					if (count == 0)
						LoopCount = int.MaxValue;
					else if (count != 0)
						LoopCount = count;
				}
				blockSize = await stream.ReadBlockAsync().ConfigureAwait(false);
			}
		}

		async Task ParseApplicationExtensionAsync(GIFDecoderStreamReader stream)
		{
			var blockSize = await stream.ReadBlockAsync().ConfigureAwait(false);
			if (blockSize >= NetscapeApplicationExtensionID.Length)
			{
				var buffer = stream.CurrentBlockBuffer;
				string identifier = System.Text.Encoding.UTF8.GetString(buffer, 0, NetscapeApplicationExtensionID.Length);
				if (identifier.Equals(NetscapeApplicationExtensionID, StringComparison.OrdinalIgnoreCase))
				{
					await ParseNetscapeApplicationExtensionAsync(stream).ConfigureAwait(false);
					return;
				}
			}
			await stream.SkipBlockAsync().ConfigureAwait(false);
			return;
		}

		async Task ParseGIFBitmapHeaderAsync(GIFDecoderStreamReader stream)
		{
			Bounds = new GIFBitmap.Rect(stream.ReadShort(), stream.ReadShort(), stream.ReadShort(), stream.ReadShort());
			ColorTable = _header.GlobalColorTable;

			int flags = stream.Read();
			if (UseLocalColorTable(flags))
			{
				ColorTable = await GIFColorTable.CreateColorTableAsync(stream, LocalColorTableSize(flags)).ConfigureAwait(false);
			}

			BackgroundColor = _header.BackgroundColor;
			IsInterlaced = UseInterlace(flags);
		}

		async Task ParseImageDescriptorAsync(GIFDecoderStreamReader stream, GIFBitmapDecoder decoder, GIFBitmap previousBitmap, bool ignoreImageData)
		{
			await ParseGIFBitmapHeaderAsync(stream).ConfigureAwait(false);
			if (IsTransparent)
				ColorTable.SetTransparency(TransparencyIndex);

			DataPosition = stream.CurrentPosition;

			if (!ignoreImageData)
			{
				// Decode LZW data stream.
				await decoder.DecodeAsync(stream, _header.Width, _header.Height).ConfigureAwait(false);

				// Compose bitmap from decoded data stream.
				decoder.Compose(_header, this, previousBitmap);

				// Consume block terminator.
				await stream.SkipBlockAsync().ConfigureAwait(false);
			}
			else
			{
				// Read pass variable length LZW data stream.
				// First byte is LZW code size followed by data blocks repeated until block terminator.
				stream.Read();
				await stream.SkipBlockAsync().ConfigureAwait(false);
			}

			if (IsTransparent)
				ColorTable.ResetTransparency();
		}

		async Task ParseExtensionAsync(GIFDecoderStreamReader stream)
		{
			int blockCode = stream.Read();
			switch (blockCode)
			{
				case GIFBlockCodes.GraphicsControlExtension:
					ParseGraphicControlExtension(stream);
					break;
				case GIFBlockCodes.ApplicationExtensionBlock:
					await ParseApplicationExtensionAsync(stream).ConfigureAwait(false);
					break;
				default:
					await stream.SkipBlockAsync().ConfigureAwait(false);
					break;
			}
		}

		public static async Task<GIFBitmap> CreateBitmapAsync(GIFDecoderStreamReader stream, GIFHeader header, GIFBitmapDecoder decoder, GIFBitmap previousBitmap, bool ignoreImageData = false)
		{
			GIFBitmap currentBitmap = null;
			bool haveImage = false;
			bool done = false;

			while (!done)
			{
				int blockCode = stream.Read();
				if (blockCode == -1)
				{
					currentBitmap = null;
					break;
				}

				switch (blockCode)
				{
					case GIFBlockCodes.ImageSeparator:
						if (currentBitmap == null)
							currentBitmap = new GIFBitmap(header);
						await currentBitmap.ParseImageDescriptorAsync(stream, decoder, previousBitmap, ignoreImageData).ConfigureAwait(false);
						haveImage = true;
						done = true;
						break;
					case GIFBlockCodes.Extension:
						if (currentBitmap == null)
							currentBitmap = new GIFBitmap(header);
						await currentBitmap.ParseExtensionAsync(stream).ConfigureAwait(false);
						break;
					case GIFBlockCodes.Trailer:
						done = true;
						if (!haveImage)
							currentBitmap = null;
						break;
					default:
						break;
				}
			}

			return currentBitmap;
		}
	}

	public class GIFBitmapDecoder
	{
		short[] _prefix;
		byte[] _suffix;
		byte[] _pixelStack;
		byte[] _pixels;

		const int DecoderStackSize = 4096;

		void InitializeBuffers(int pixelCount)
		{
			if (_pixels == null || _pixels.Length < pixelCount)
			{
				_pixels = new byte[pixelCount];
			}
			if (_prefix == null)
			{
				_prefix = new short[DecoderStackSize];
			}
			if (_suffix == null)
			{
				_suffix = new byte[DecoderStackSize];
			}
			if (_pixelStack == null)
			{
				_pixelStack = new byte[DecoderStackSize + 1];
			}
		}

		void RestoreToBackground(GIFHeader header, GIFBitmap currentBitmap, GIFBitmap previousBitmap, int[] bitmapData)
		{
			int color = 0;
			if (!currentBitmap.IsTransparent)
			{
				color = previousBitmap.BackgroundColor;
			}

			var previousBitmapBounds = previousBitmap.Bounds;
			for (int currentRow = 0; currentRow < previousBitmapBounds.Height; currentRow++)
			{
				int startBitmapIndex = (previousBitmapBounds.Y + currentRow) * header.Width + previousBitmapBounds.X;
				int endBitmapIndex = startBitmapIndex + previousBitmapBounds.Width;
				for (int currentBitmapIndex = startBitmapIndex; currentBitmapIndex < endBitmapIndex; currentBitmapIndex++)
				{
					bitmapData[currentBitmapIndex] = color;
				}
			}
		}

		public void Compose(GIFHeader header, GIFBitmap currentBitmap, GIFBitmap previousBitmap)
		{
			int[] bitmapData = null;
			var width = header.Width;
			var height = header.Height;

			if (previousBitmap != null && previousBitmap.Dispose != GIFBitmap.DisposeMethod.NoAction)
			{
				if (previousBitmap.Data != null)
				{
					bitmapData = previousBitmap.Data;
					if (previousBitmap.Dispose == GIFBitmap.DisposeMethod.RestoreToBackground)
					{
						RestoreToBackground(header, currentBitmap, previousBitmap, bitmapData);
					}
				}
			}

			// Reuse previous bitmap buffer or allocate new.
			if (bitmapData == null)
			{
				bitmapData = new int[width * height];
			}

			int interlacePass = 1;
			int interlaceRowInc = 8;
			int interlaceStartRow = 0;
			var bounds = currentBitmap.Bounds;
			var isInterlaced = currentBitmap.IsInterlaced;
			var colorTable = currentBitmap.ColorTable.Data;

			for (int sourceRow = 0; sourceRow < bounds.Height; sourceRow++)
			{
				int targetRow = sourceRow;
				if (isInterlaced)
				{
					if (interlaceStartRow >= bounds.Height)
					{
						interlacePass++;
						switch (interlacePass)
						{
							case 2:
								interlaceStartRow = 4;
								break;
							case 3:
								interlaceStartRow = 2;
								interlaceRowInc = 4;
								break;
							case 4:
								interlaceStartRow = 1;
								interlaceRowInc = 2;
								break;
							default:
								break;
						}
					}
					targetRow = interlaceStartRow;
					interlaceStartRow += interlaceRowInc;
				}
				targetRow += bounds.Y;
				if (targetRow < height)
				{
					int startBitmapIndex = targetRow * width;
					int currentBitmapIndex = startBitmapIndex + bounds.X;
					int endBitmapIndex = currentBitmapIndex + bounds.Width;
					if ((startBitmapIndex + width) < endBitmapIndex)
					{
						endBitmapIndex = startBitmapIndex + width;
					}

					int currentPixelIndex = sourceRow * bounds.Width;
					while (currentBitmapIndex < endBitmapIndex)
					{
						int colorIndex = _pixels[currentPixelIndex++];
						int color = colorTable[colorIndex];
						if (color != 0)
						{
							bitmapData[currentBitmapIndex] = color;
						}
						currentBitmapIndex++;
					}
				}
			}

			currentBitmap.Data = bitmapData;
		}

		public async Task DecodeAsync(GIFDecoderStreamReader stream, int width, int height)
		{
			int pixelCount = width * height;
			InitializeBuffers(pixelCount);

			int nullCode = -1;
			int inCode = nullCode;
			int oldCode = nullCode;
			int currentCode = nullCode;

			int dataSize = stream.Read();
			int codeSize = dataSize + 1;
			int codeMask = (1 << codeSize) - 1;

			int clearCode = 1 << dataSize;
			int endOfInformationCode = clearCode + 1;
			int availableCode = clearCode + 2;

			for (currentCode = 0; currentCode < clearCode; currentCode++)
			{
				_prefix[currentCode] = 0;
				_suffix[currentCode] = (byte)currentCode;
			}

			int datum = 0;
			int bits = 0;
			int count = 0;
			int firstCode = 0;
			int currentStackIndex = 0;
			int currentPixelIndex = 0;
			int currentBitIndex = 0;
			int i = 0;

			for (i = 0; i < pixelCount;)
			{
				if (currentStackIndex == 0)
				{
					if (bits < codeSize)
					{
						if (count == 0)
						{
							count = await stream.ReadBlockAsync().ConfigureAwait(false);
							if (count <= 0)
							{
								break;
							}
							currentBitIndex = 0;
						}
						datum += (stream.CurrentBlockBuffer[currentBitIndex] << bits);
						bits += 8;
						currentBitIndex++;
						count--;
						continue;
					}

					currentCode = datum & codeMask;
					datum >>= codeSize;
					bits -= codeSize;
					if ((currentCode > availableCode) || (currentCode == endOfInformationCode))
					{
						break;
					}

					if (currentCode == clearCode)
					{
						codeSize = dataSize + 1;
						codeMask = (1 << codeSize) - 1;
						availableCode = clearCode + 2;
						oldCode = nullCode;
						continue;
					}

					if (oldCode == nullCode)
					{
						_pixelStack[currentStackIndex++] = _suffix[currentCode];
						oldCode = currentCode;
						firstCode = currentCode;
						continue;
					}

					inCode = currentCode;
					if (currentCode == availableCode)
					{
						_pixelStack[currentStackIndex++] = (byte)firstCode;
						currentCode = oldCode;
					}

					while (currentCode > clearCode)
					{
						_pixelStack[currentStackIndex++] = _suffix[currentCode];
						currentCode = _prefix[currentCode];
					}

					firstCode = _suffix[currentCode];
					if (availableCode >= DecoderStackSize)
					{
						break;
					}

					_pixelStack[currentStackIndex++] = (byte)firstCode;
					_prefix[availableCode] = (short)oldCode;
					_suffix[availableCode] = (byte)firstCode;
					availableCode++;
					if (((availableCode & codeMask) == 0) && (availableCode < DecoderStackSize))
					{
						codeSize++;
						codeMask += availableCode;
					}
					oldCode = inCode;
				}
				currentStackIndex--;
				_pixels[currentPixelIndex++] = _pixelStack[currentStackIndex];
				i++;
			}

			for (i = currentPixelIndex; i < pixelCount; i++)
			{
				_pixels[i] = 0;
			}
		}
	}

	public abstract class GIFImageParser
	{
		protected abstract void StartParsing();
		protected abstract void AddBitmap(GIFHeader header, GIFBitmap bitmap, bool ignoreImageData);
		protected abstract void FinishedParsing();

		public async Task ParseAsync(Stream stream, bool skipTypeIdentifier = false, bool ignoreImageData = false)
		{
			if (stream != null)
			{
				GIFBitmap previousBitmap = null;
				GIFBitmap currentBitmap = null;

				GIFBitmapDecoder decoder = new GIFBitmapDecoder();
				GIFDecoderStreamReader reader = new GIFDecoderStreamReader(stream);

				StartParsing();

				GIFHeader header = await GIFHeader.CreateHeaderAsync(reader, skipTypeIdentifier).ConfigureAwait(false);

				currentBitmap = await GIFBitmap.CreateBitmapAsync(reader, header, decoder, previousBitmap, ignoreImageData).ConfigureAwait(false);
				while (currentBitmap != null)
				{
					AddBitmap(header, currentBitmap, ignoreImageData);
					previousBitmap = currentBitmap;
					currentBitmap = await GIFBitmap.CreateBitmapAsync(reader, header, decoder, previousBitmap, ignoreImageData).ConfigureAwait(false);
				}

				FinishedParsing();
			}
			else
			{
				throw new ArgumentNullException(nameof(stream));
			}
		}
	}
}
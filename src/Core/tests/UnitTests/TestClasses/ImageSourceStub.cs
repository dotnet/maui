using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.UnitTests
{
	class ImageSourceStub : IImageSource
	{
		public bool IsEmpty { get; set; }
	}

	class FileImageSourceStub : ImageSourceStub, IFileImageSource
	{
		public string File { get; set; }
	}

	class FontImageSourceStub : ImageSourceStub, IFontImageSource
	{
		public Color Color { get; set; }

		public Font Font { get; set; }

		public string Glyph { get; set; }
	}
}
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public interface ICountedImageSourceStub : IImageSource
	{
		bool Wait { get; }

		bool IsResolutionDependent { get; }

		Color Color { get; }
	}

	public partial class CountedImageSourceStub : ImageSourceStub, ICountedImageSourceStub
	{
		public CountedImageSourceStub()
		{
		}

		public CountedImageSourceStub(Color color, bool wait = false)
		{
			Color = color;
			Wait = wait;
		}

		public bool Wait { get; set; } = false;

		public bool IsResolutionDependent { get; set; } = false;

		public Color Color { get; set; }
	}
}
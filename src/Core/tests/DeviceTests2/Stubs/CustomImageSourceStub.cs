using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public interface ICustomImageSourceStub : IImageSource
	{
		Color Color { get; }
	}

	public partial class CustomImageSourceStub : ImageSourceStub, ICustomImageSourceStub
	{
		public CustomImageSourceStub()
		{
		}

		public CustomImageSourceStub(Color color)
		{
			Color = color;
		}

		public Color Color { get; set; }
	}
}
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class FontImageSourceServiceTests
	{
		[Fact]
		public Task CanRenderSystemFontFamily()
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var manager = new FontManager(new FontRegistrar(fontLoader: null));
				var service = new FontImageSourceService(manager);
				var imageSource = new FontImageSourceStub
				{
					Glyph = "X",
					Font = Font.OfSize("Segoe UI", 24),
					Color = Colors.Red,
				};

				var result = service.RenderImageSource(imageSource, 1);

				Assert.NotNull(result);
			});
		}
	}
}

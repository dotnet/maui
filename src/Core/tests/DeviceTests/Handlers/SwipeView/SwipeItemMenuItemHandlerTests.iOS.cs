using System.Threading.Tasks;
using CoreGraphics;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using UIKit;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.SwipeView)]
	public class SwipeItemMenuItemHandlerTests : CoreHandlerTestBase
	{
		[Fact]
		public Task TextColorCanBeCleared()
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var item = new SwipeItemMenuItemStub
				{
					Text = "Delete",
					TextColor = Colors.Red
				};
				var handler = CreateHandler<SwipeItemMenuItemHandler>(item);

				Assert.Equal(Colors.Red, handler.PlatformView.CurrentTitleColor.ToColor());

				item.TextColor = null;
				handler.UpdateValue(nameof(ITextStyle.TextColor));

				Assert.NotEqual(Colors.Red, handler.PlatformView.CurrentTitleColor.ToColor());
			});
		}

		[Fact]
		public async Task ColorlessFontIconUsesTemplateRenderingAndTracksTextColor()
		{
			await InvokeOnMainThreadAsync(async () =>
			{
				var item = new SwipeItemMenuItemStub();
				var handler = CreateHandler<SwipeItemMenuItemHandler>(item);
				handler.PlatformView.Frame = new CGRect(0, 0, 100, 100);

				item.Source = new FontImageSourceStub
				{
					Glyph = "X",
					Font = Font.Default.WithSize(30)
				};

				await SwipeItemMenuItemHandler.MapSourceAsync(handler, item);

				var image = handler.PlatformView.ImageForState(UIControlState.Normal);
				Assert.NotNull(image);
				Assert.Equal(UIImageRenderingMode.AlwaysTemplate, image.RenderingMode);

				item.TextColor = Colors.Red;
				handler.UpdateValue(nameof(ITextStyle.TextColor));

				await AssertEventually(() => handler.PlatformView.TintColor?.ToColor() == Colors.Red);
			});
		}
	}
}

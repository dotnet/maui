using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Xunit;

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

				Assert.NotSame(
					DependencyProperty.UnsetValue,
					handler.PlatformView.ReadLocalValue(SwipeItem.ForegroundProperty));

				item.TextColor = null;
				handler.UpdateValue(nameof(ITextStyle.TextColor));

				Assert.Same(
					DependencyProperty.UnsetValue,
					handler.PlatformView.ReadLocalValue(SwipeItem.ForegroundProperty));
			});
		}

		[Fact]
		public Task TintedPackagedFileUsesFlattenedMauiAssetName()
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var icon = Assert.IsType<BitmapIconSource>(
					SwipeItemMenuItemHandler.CreateTintedIconSource(
						new FileImageSourceStub("Resources/Images/delete.png"),
						MauiContext));

				Assert.Equal(new Uri("ms-appx:///delete.png"), icon.UriSource);
			});
		}

		[Fact]
		public Task TintedRootedFileFallsBackToUntintedLoader()
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var icon = SwipeItemMenuItemHandler.CreateTintedIconSource(
					new FileImageSourceStub(@"C:\images\delete.png"),
					MauiContext);

				Assert.Null(icon);
			});
		}
	}
}

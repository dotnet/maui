using System.Threading.Tasks;
using Android.Widget;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.SwipeView)]
	public class SwipeItemMenuItemHandlerTests : CoreHandlerTestBase
	{
		[Fact]
		public async Task IconTintCanBeClearedWithoutMutatingSharedDrawable()
		{
			await InvokeOnMainThreadAsync(async () =>
			{
				var tintedItem = new SwipeItemMenuItemStub
				{
					IconColor = Colors.Blue,
					Source = new FileImageSourceStub("red.png")
				};
				var tintedHandler = CreateHandler<SwipeItemMenuItemHandler>(tintedItem);
				var tintedButton = Assert.IsAssignableFrom<TextView>(tintedHandler.PlatformView);

				await AssertEventually(() => GetTopDrawable(tintedButton)?.ColorFilter is not null);

				var untintedItem = new SwipeItemMenuItemStub
				{
					Source = new FileImageSourceStub("red.png")
				};
				var untintedHandler = CreateHandler<SwipeItemMenuItemHandler>(untintedItem);
				var untintedButton = Assert.IsAssignableFrom<TextView>(untintedHandler.PlatformView);

				await AssertEventually(() => GetTopDrawable(untintedButton) is not null);
				Assert.Null(GetTopDrawable(untintedButton).ColorFilter);

				tintedItem.IconColor = null;
				tintedHandler.UpdateValue(nameof(ISwipeItemMenuItemIconColor.IconColor));

				await AssertEventually(() => GetTopDrawable(tintedButton) is not null &&
					GetTopDrawable(tintedButton).ColorFilter is null);
			});
		}

		static global::Android.Graphics.Drawables.Drawable GetTopDrawable(TextView textView)
		{
			var drawables = textView.GetCompoundDrawables();
			return drawables.Length > 1 ? drawables[1] : null;
		}
	}
}

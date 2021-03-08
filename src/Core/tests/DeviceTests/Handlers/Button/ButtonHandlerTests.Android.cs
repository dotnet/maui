using System.Threading.Tasks;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Xunit;
using AColor = global::Android.Graphics.Color;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ButtonHandlerTests
	{
		[Fact]
		public async Task PaddingInitializesCorrectly()
		{
			var button = new ButtonStub()
			{
				Text = "Test",
				Padding = new Thickness(5, 10, 15, 20)
			};

			var handler = await CreateHandlerAsync(button);
			var appCompatButton = (AppCompatButton)handler.View;
			var (left, top, right, bottom) = (appCompatButton.PaddingLeft, appCompatButton.PaddingTop, appCompatButton.PaddingRight, appCompatButton.PaddingBottom);

			var context = handler.View.Context;

			var expectedLeft = context.ToPixels(5);
			var expectedTop = context.ToPixels(10);
			var expectedRight = context.ToPixels(15);
			var expectedBottom = context.ToPixels(20);

			Assert.Equal(expectedLeft, left);
			Assert.Equal(expectedTop, top);
			Assert.Equal(expectedRight, right);
			Assert.Equal(expectedBottom, bottom);
		}

		AppCompatButton GetNativeButton(ButtonHandler buttonHandler) =>
			(AppCompatButton)buttonHandler.View;

		string GetNativeText(ButtonHandler buttonHandler) =>
			GetNativeButton(buttonHandler).Text;

		Color GetNativeTextColor(ButtonHandler buttonHandler)
		{
			int currentTextColorInt = GetNativeButton(buttonHandler).CurrentTextColor;
			AColor currentTextColor = new AColor(currentTextColorInt);

			return currentTextColor.ToColor();
		}

		Thickness GetNativePadding(ButtonHandler buttonHandler)
		{
			var appCompatButton = GetNativeButton(buttonHandler);
			return ToThicknees(appCompatButton);

			static Thickness ToThicknees(AppCompatButton appCompatButton)
			{
				var onePx = appCompatButton.Context.ToPixels(1);


				return new Thickness(appCompatButton.PaddingLeft,
					appCompatButton.PaddingTop, appCompatButton.PaddingRight, appCompatButton.PaddingBottom);
			}
		}

		Task PerformClick(IButton button)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				GetNativeButton(CreateHandler(button)).PerformClick();
			});
		}
	}
}
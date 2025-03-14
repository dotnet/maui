using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;
using System.ComponentModel;

namespace Microsoft.Maui.DeviceTests
{
	public partial class BoxViewTests
	{
		MauiShapeView GetNativeBoxView(ShapeViewHandler boxViewViewHandler) =>
			boxViewViewHandler.PlatformView;

		Task<float> GetPlatformOpacity(ShapeViewHandler handler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetNativeBoxView(handler);
				return nativeView.Alpha;
			});
		}

		//src/Compatibility/Core/tests/Android/TranslationTests.cs
		[Fact]
		[Description("The Translation property of a BoxView should match with native Translation")]
		public async Task BoxViewTranslationConsistent()
		{
			var boxView = new BoxView()
			{
				HeightRequest = 100,
				WidthRequest = 200,
				TranslationX = 50,
				TranslationY = -20
			};

			var handler = await CreateHandlerAsync<ShapeViewHandler>(boxView);
			var nativeView = GetNativeBoxView(handler);
			await InvokeOnMainThreadAsync(() =>
			{
				var translation = nativeView.TranslationX;
				var density = Microsoft.Maui.Devices.DeviceDisplay.Current.MainDisplayInfo.Density;
				var expectedInPixels = density * boxView.TranslationX;

				Assert.Equal(expectedInPixels, translation, 1.0);

				var translationY = nativeView.TranslationY;
				var expectedYInPixels = density * boxView.TranslationY;
				Assert.Equal(expectedYInPixels, translationY, 1.0);
			});
		}
	}
}
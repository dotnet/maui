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
				AssertTranslationMatches(nativeView, boxView.TranslationX, boxView.TranslationY);
			});
		}
	}
}
using System;
using System.Threading.Tasks;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;
using System.ComponentModel;

namespace Microsoft.Maui.DeviceTests
{
	[Collection(RunInNewWindowCollection)]
	public partial class CheckBoxTests
	{
		AppCompatCheckBox GetNativeCheckBox(CheckBoxHandler checkBoxHandler) =>
			checkBoxHandler.PlatformView;

		Task<float> GetPlatformOpacity(CheckBoxHandler CheckBoxHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetNativeCheckBox(CheckBoxHandler);
				return nativeView.Alpha;
			});
		}

		//src/Compatibility/Core/tests/Android/TranslationTests.cs
		[Fact]
		[Description("The Translation property of a CheckBox should match with native Translation")]
		public async Task CheckBoxTranslationConsistent()
		{
			var checkBox = new CheckBox()
			{
				TranslationX = 50,
				TranslationY = -20
			};

			var handler = await CreateHandlerAsync<CheckBoxHandler>(checkBox);
			var nativeView = GetNativeCheckBox(handler);
			await InvokeOnMainThreadAsync(() =>
			{
				var translation = nativeView.TranslationX;
				var density = Microsoft.Maui.Devices.DeviceDisplay.Current.MainDisplayInfo.Density;
				var expectedInPixels = density * checkBox.TranslationX;

				Assert.Equal(expectedInPixels, translation, 1.0);

				var translationY = nativeView.TranslationY;
				var expectedYInPixels = density * checkBox.TranslationY;
				Assert.Equal(expectedYInPixels, translationY, 1.0);
			});
		}
	}
}
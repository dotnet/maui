using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class LabelHandlerTests
	{
		[Fact(DisplayName = "Html Text Initializes Correctly")]
		public async Task HtmlTextInitializesCorrectly()
		{
			var label = new LabelStub()
			{
				TextType = TextType.Html,
				Text = "<h2><strong>Test1&nbsp;</strong>Test2</h2>"
			};

			var platformText = await GetValueAsync(label, (handler) =>
			{
				return handler.PlatformView.Text;
			});

			Assert.NotNull(platformText);
		}

		TextBlock GetPlatformLabel(LabelHandler labelHandler) =>
			labelHandler.PlatformView;

		string GetNativeText(LabelHandler labelHandler) =>
			GetPlatformLabel(labelHandler).Text;

		Color GetNativeTextColor(LabelHandler labelHandler)
		{
			var platformLabel = GetPlatformLabel(labelHandler);

			var foreground = platformLabel.Foreground;

			if (foreground is SolidColorBrush solidColorBrush)
				return solidColorBrush.Color.ToColor();

			return null;
		}

		UI.Xaml.TextAlignment GetNativeHorizontalTextAlignment(LabelHandler labelHandler) =>
			GetPlatformLabel(labelHandler).TextAlignment;
	}
}
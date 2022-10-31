using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.DeviceTests
{
	public partial class LabelHandlerTests
	{
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

		Task ValidateHasColor(ILabel label, Color color, Action action = null)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var platformLabel = GetPlatformLabel(CreateHandler(label));
				action?.Invoke();
				platformLabel.AssertContainsColorAsync(color);
			});
		}
	}
}
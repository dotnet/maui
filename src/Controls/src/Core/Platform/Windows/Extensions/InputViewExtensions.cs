#nullable disable
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Specifics = Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.InputView;

namespace Microsoft.Maui.Controls.Platform
{
	public static class InputViewExtensions
	{
		public static void UpdateDetectReadingOrderFromContent(this TextBox platformControl, Entry entry)
		{
			if (entry.IsSet(Specifics.DetectReadingOrderFromContentProperty))
				platformControl.SetTextReadingOrder(entry.OnThisPlatform().GetDetectReadingOrderFromContent());
		}

		public static void UpdateDetectReadingOrderFromContent(this TextBox platformControl, Editor editor)
		{
			if (editor.IsSet(Specifics.DetectReadingOrderFromContentProperty))
				platformControl.SetTextReadingOrder(editor.OnThisPlatform().GetDetectReadingOrderFromContent());
		}

		internal static void SetTextReadingOrder(this TextBox platformControl, bool detectReadingOrderFromContent) =>
			platformControl.TextReadingOrder = detectReadingOrderFromContent
				? TextReadingOrder.DetectFromContent
				: TextReadingOrder.UseFlowDirection;
	}
}
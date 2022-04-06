using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using Specifics = Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.InputView;

namespace Microsoft.Maui.Controls.Platform
{
	public static class InputViewExtensions
	{
		public static void UpdateDetectReadingOrderFromContent(this TextBox platformControl, Entry entry)
		{
			if (entry.IsSet(Specifics.DetectReadingOrderFromContentProperty))
			{
				platformControl.SetTextReadingOrder(entry.OnThisPlatform().GetDetectReadingOrderFromContent());
			}
		}

		public static void UpdateDetectReadingOrderFromContent(this TextBox platformControl, Editor editor)
		{
			if (editor.IsSet(Specifics.DetectReadingOrderFromContentProperty))
			{
				platformControl.SetTextReadingOrder(editor.OnThisPlatform().GetDetectReadingOrderFromContent());
			}
		}

		public static void UpdateDetectReadingOrderFromContent(this TextBlock platformControl, Label label)
		{
			if (label.IsSet(Specifics.DetectReadingOrderFromContentProperty))
			{
				platformControl.SetTextReadingOrder(label.OnThisPlatform().GetDetectReadingOrderFromContent());
			}
		}

		internal static void SetTextReadingOrder(this TextBox platformControl, bool detectReadingOrderFromContent)
		{
			if (detectReadingOrderFromContent)
			{
				platformControl.TextReadingOrder = TextReadingOrder.DetectFromContent;
			}
			else
			{
				platformControl.TextReadingOrder = TextReadingOrder.UseFlowDirection;
			}
		}

		internal static void SetTextReadingOrder(this TextBlock platformControl, bool detectReadingOrderFromContent)
		{
			if (detectReadingOrderFromContent)
			{
				platformControl.TextReadingOrder = TextReadingOrder.DetectFromContent;
			}
			else
			{
				platformControl.TextReadingOrder = TextReadingOrder.UseFlowDirection;
			}
		}
	}
}
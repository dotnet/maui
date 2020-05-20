using System.Maui.Core.Controls;
using WButton = System.Windows.Controls.Button;

namespace System.Maui.Platform
{
	internal static class ViewExtensions
	{
		public static void SetText(this MauiButton button, string text)
		{
			button.Content = text;
		}
		public static void SetTextColor(this MauiButton button, Color color, Color defaultColor)
		{
			button.UpdateDependencyColor(WButton.ForegroundProperty, color);
		}
	}
}

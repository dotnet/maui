using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Platform
{
	public static class MauiComboBoxExtensions
	{
		public static void UpdateText(this MauiComboBox platformControl, InputView inputView)
		{
			platformControl.Text = TextTransformUtilites.GetTransformedText(inputView.Text, inputView.TextTransform);
		}
	}
}

using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Platform
{
	public static class MauiComboBoxExtensions
	{
		public static void UpdateText(this MauiComboBox platformControl, IPicker inputView, TextTransform tranform)
		{
			platformControl.Text = TextTransformUtilites.GetTransformedText(inputView.Text, tranform);
		}
	}
}

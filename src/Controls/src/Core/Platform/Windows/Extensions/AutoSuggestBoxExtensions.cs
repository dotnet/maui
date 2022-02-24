#nullable enable
using Microsoft.Maui.Controls.Internals;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Controls.Platform
{
	internal static class AutoSuggestBoxExtensions
	{
		public static void UpdateText(this AutoSuggestBox platformControl, InputView inputView)
		{
			platformControl.Text = TextTransformUtilites.GetTransformedText(inputView.Text, inputView.TextTransform);
		}
	}

}

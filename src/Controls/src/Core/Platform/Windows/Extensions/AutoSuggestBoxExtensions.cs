using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Controls.Platform
{
	internal static class AutoSuggestBoxExtensions
	{
		public static void UpdateIsSpellCheckEnabled(this AutoSuggestBox platformControl, SearchBar searchBar)
		{
			var queryTextBox = platformControl.GetFirstDescendant<TextBox>();

			if (queryTextBox == null)
				return;

			queryTextBox.IsSpellCheckEnabled = searchBar.OnThisPlatform().GetIsSpellCheckEnabled();
		}

		public static void UpdateText(this AutoSuggestBox platformControl, InputView inputView)
		{
			platformControl.Text = TextTransformUtilites.GetTransformedText(inputView.Text, inputView.TextTransform);
		}
	}
}

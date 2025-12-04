using System.Diagnostics;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class SearchBarPage
	{
		public SearchBarPage()
		{
			InitializeComponent();
		}

		void OnSearchBarFocused(object sender, FocusEventArgs e)
		{
			var text = ((SearchBar)sender).Text;
			DisplayAlertAsync("Focused", text, "Ok");
		}

		void OnSearchBarUnfocused(object sender, FocusEventArgs e)
		{
			var text = ((SearchBar)sender).Text;
			DisplayAlertAsync("Unfocused", text, "Ok");
		}

		void OnSearchBarTextChanged(object sender, TextChangedEventArgs args)
		{
			var text = ((SearchBar)sender).Text;
			Debug.WriteLine($"SearchBar Text changed: {text}");
		}
	}
}
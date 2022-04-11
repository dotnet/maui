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

		void OnSearchBarTextChanged(object sender, TextChangedEventArgs args)
		{
			var text = ((SearchBar)sender).Text;
			Debug.WriteLine($"SearchBar Text changed: {text}");
		}
	}
}
using Microsoft.Maui.ManualTests.Models;
using Microsoft.Maui.ManualTests.ViewModels;

namespace Microsoft.Maui.ManualTests.Views
{
	public partial class VerticalListSingleSelectionPage : ContentPage
	{
		public VerticalListSingleSelectionPage()
		{
			InitializeComponent();
			BindingContext = new MonkeysViewModel();
			UpdateSelectionData(Enumerable.Empty<Monkey>(), Enumerable.Empty<Monkey>());
		}

		void OnCollectionViewSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			UpdateSelectionData(e.PreviousSelection, e.CurrentSelection);
		}

		void UpdateSelectionData(IEnumerable<object> previousSelectedItems, IEnumerable<object> currentSelectedItems)
		{
			string previous = (previousSelectedItems.FirstOrDefault() as Monkey)?.Name;
			string current = (currentSelectedItems.FirstOrDefault() as Monkey)?.Name;
			previousSelectedItemLabel.Text = string.IsNullOrWhiteSpace(previous) ? "[none]" : previous;
			currentSelectedItemLabel.Text = string.IsNullOrWhiteSpace(current) ? "[none]" : current;
		}
	}
}

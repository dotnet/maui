using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 22417, "[Windows] The list does not show newly added items", PlatformAffected.UWP)]
	public partial class Issue22417 : ContentPage
	{
		ObservableCollection<Issue22000Model> _exampleItems = new();

		public Issue22417()
		{
			InitializeComponent();

			_exampleItems.Add(new Issue22000Model("First", "First CarouselView item", Colors.Red));
			_exampleItems.Add(new Issue22000Model("Second", "Second CarouselView item", Colors.LightBlue));
			_exampleItems.Add(new Issue22000Model("Third", "Third CarouselView item", Colors.Pink));
			_exampleItems.Add(new Issue22000Model("Fourth", "Fourth CarouselView item", Colors.GreenYellow));
			_exampleItems.Add(new Issue22000Model("Fifth", "Fifth CarouselView item", Colors.Purple));

			TestCarouselView.ItemsSource = _exampleItems;
		}

		void OnAddItemClicked(object sender, EventArgs e)
		{
			_exampleItems.Add(new Issue22000Model("Sixth", "Sixth CarouselView item", Colors.CornflowerBlue));
			TestCarouselView.ScrollTo(5, animate: false);
		}
	}
}
using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 26857, "ListView ScrollTo position always remains at the start even when set to Center or End", PlatformAffected.All)]
	public class Issue26857 : ContentPage
	{
		private ListView ListView;
		private ObservableCollection<string> Items;

		public Issue26857()
		{
			Items = new ObservableCollection<string>
			{
				"Item 1",
				"Item 2",
				"Item 3",
				"Item 4",
				"Item 5",
				"Item 6",
				"Item 7",
				"Item 8",
				"Item 9",
				"Item 10",
				"Item 11",
				"Item 12",
				"Item 13",
				"Item 14",
				"Item 15",
			};

			ListView = new ListView
			{
				ItemsSource = Items,
				SelectedItem = "Item 3",
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center,
				WidthRequest = 300,
				HeightRequest = 220,
			};

			var button = new Button
			{
				Text = "ChangeSelectedItem",
				AutomationId = "Button",
				HorizontalOptions = LayoutOptions.Center
			};
			button.Clicked += Button_Clicked;

			var stackLayout = new StackLayout
			{
				Orientation = StackOrientation.Vertical,
				Spacing = 20
			};

			stackLayout.Children.Add(ListView);
			stackLayout.Children.Add(button);
			this.Content = stackLayout;
		}

		private void ScrollToSelectedItem()
		{
			if (ListView?.SelectedItem != null)
			{
				ListView.ScrollTo(ListView.SelectedItem, ScrollToPosition.Center, true);
			}
		}

		private void Button_Clicked(object sender, EventArgs e)
		{
			var currentIndex = Items.IndexOf(ListView.SelectedItem as string);
			if (currentIndex < Items.Count - 2)
			{
				ListView.SelectedItem = Items[currentIndex + 2];
				ScrollToSelectedItem();
			}
		}
	}
}
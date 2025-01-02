using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 26857, "ListView ScrollTo position always remains at the start even when set to Center or End", PlatformAffected.All)]
	public class Issue26857 : ContentPage
	{
		private ListView ListView;
		private ObservableCollection<string> Items;
		private Label label;
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

			var upButton = new Button
			{
				Text = "Up",
				AutomationId = "UpButton"
			};
			upButton.Clicked += UpButton_Clicked;

			var downButton = new Button
			{
				Text = "Down",
				AutomationId = "DownButton"
			};
			downButton.Clicked += DownButton_Clicked;

			label = new Label
			{
				AutomationId = "SelectedItemLabel"
			};

			var stackLayout = new StackLayout
			{
				Orientation = StackOrientation.Vertical,
				Spacing = 20
			};

			stackLayout.Children.Add(ListView);
			stackLayout.Children.Add(upButton);
			stackLayout.Children.Add(downButton);
			stackLayout.Children.Add(label);
			this.Content = stackLayout;
		}

		private void ScrollToSelectedItem()
		{
			if (ListView?.SelectedItem != null)
			{
				ListView.ScrollTo(ListView.SelectedItem, ScrollToPosition.Center, true);
				label.Text = ListView.SelectedItem as string;
			}
		}

		private void UpButton_Clicked(object sender, EventArgs e)
		{
			var currentIndex = Items.IndexOf(ListView.SelectedItem as string);
			if (currentIndex > 0)
			{
				ListView.SelectedItem = Items[currentIndex - 2];
				ScrollToSelectedItem();
			}
		}

		private void DownButton_Clicked(object sender, EventArgs e)
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
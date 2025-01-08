using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 26945, "ListView ScrollTo position always remains at the start even when set to Center or End without animation", PlatformAffected.All)]
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
				"Item 16",
				"Item 17",
				"Item 18",
				"Item 19",
				"Item 20",
				"Item 21",
				"Item 22",
				"Item 23",
				"Item 24",
				"Item 25",
			};

			ListView = new ListView
			{
				ItemsSource = Items,
				SelectedItem = "Item 10",
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center,
				WidthRequest = 300,
				HeightRequest = 220,
			};

			var horizontalStack = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				HorizontalOptions = LayoutOptions.Center,
				Spacing = 20
			};

			var startButton = new Button
			{
				Text = "Start",
				AutomationId = "StartButton",
			};
			startButton.Clicked += ScrollPositionStart_Clicked;

			var centerButton = new Button
			{
				Text = "Center",
				AutomationId = "CenterButton",
			};
			centerButton.Clicked += ScrollPositionCenter_Clicked;

			var endButton = new Button
			{
				Text = "End",
				AutomationId = "EndButton",
			};
			endButton.Clicked += ScrollPositionEnd_Clicked;

			horizontalStack.Children.Add(startButton);
			horizontalStack.Children.Add(centerButton);
			horizontalStack.Children.Add(endButton);

			var stackLayout = new StackLayout
			{
				Orientation = StackOrientation.Vertical,
				Spacing = 20
			};

			stackLayout.Children.Add(ListView);
			stackLayout.Children.Add(horizontalStack);
			this.Content = stackLayout;
		}

		private void ScrollToSelectedItem(ScrollToPosition toPosition)
		{
			if (ListView?.SelectedItem != null)
			{
				switch (toPosition)
				{
					case ScrollToPosition.Start:
						ListView.ScrollTo(ListView.SelectedItem, ScrollToPosition.Start, false);
						break;
					case ScrollToPosition.Center:
						ListView.ScrollTo(ListView.SelectedItem, ScrollToPosition.Center, false);
						break;
					case ScrollToPosition.End:
						ListView.ScrollTo(ListView.SelectedItem, ScrollToPosition.End, false);
						break;
				}
			}
		}

		private void ScrollPositionStart_Clicked(object sender, EventArgs e)
		{
			var currentIndex = Items.IndexOf(ListView.SelectedItem as string);
			ListView.SelectedItem = Items[currentIndex + 2];
			ScrollToSelectedItem(ScrollToPosition.Start);
		}

		private void ScrollPositionCenter_Clicked(object sender, EventArgs e)
		{
			var currentIndex = Items.IndexOf(ListView.SelectedItem as string);
			ListView.SelectedItem = Items[currentIndex + 2];
			ScrollToSelectedItem(ScrollToPosition.Center);
		}

		private void ScrollPositionEnd_Clicked(object sender, EventArgs e)
		{
			var currentIndex = Items.IndexOf(ListView.SelectedItem as string);
			ListView.SelectedItem = Items[currentIndex + 2];
			ScrollToSelectedItem(ScrollToPosition.End);
		}
	}
}
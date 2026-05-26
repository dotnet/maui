using System.ComponentModel;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 28343, "Progress spinner is not disabled after setting content on disabled RefreshView.", PlatformAffected.iOS)]
	public class Issue28343 : ContentPage
	{
		public Issue28343()
		{
			var refreshView = new RefreshView
			{
				IsEnabled = false,
				Content = CreateContent(),
			};

			Grid grid = new Grid
			{
				{
					new Label
					{
						Text = "Refresh Not Triggered",
						AutomationId = "RefreshNotTriggered"
					}, 0, 0 },
				{
					new Button
					{
						Text = "Set To Enabled",
						AutomationId = "SetToEnabled",
						Command = new Command(() =>
						{
							refreshView.IsEnabled = true;
						})
					}, 0, 1 },
				{
					new Button
					{
						Text = "Reset Content",
						AutomationId = "ResetContent",
						Command = new Command(() =>
						{
							refreshView.Content = CreateContent();
						})
					}, 0, 2 },
				{
					new Button
					{
						Text = "Scroll Up",
						AutomationId = "ScrollToUpButton",
						Command = new Command(() =>
						{
							if (refreshView.Content is CollectionView collectionView)
							{
								collectionView.ScrollTo(0, position: ScrollToPosition.Start, animate: false);
							}
						})
					}, 0, 3 },
				{ refreshView, 0, 4 }
			};

			refreshView.Command = new Command(() =>
			{
				grid.RemoveAt(0);
				grid.Insert(0, new Label() { Text = "Refresh Triggered", AutomationId = "RefreshTriggered" });
			});

			Content = grid;

			grid.RowDefinitions[0].Height = GridLength.Auto;
			grid.RowDefinitions[1].Height = GridLength.Auto;
			grid.RowDefinitions[2].Height = GridLength.Auto;
			grid.RowDefinitions[3].Height = GridLength.Auto;
			grid.RowDefinitions[4].Height = GridLength.Star;
		}

		View CreateContent()
		{
			return new CollectionView
			{
				AutomationId = "CollectionView",
				ItemsSource = Enumerable.Range(0, 100).Select(x => $"ListItem{x}"),
				ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label
					{
						HeightRequest = 100,
						BackgroundColor = Colors.Green,
						TextColor = Colors.White
					};

					label.SetBinding(Label.TextProperty, ".");
					label.SetBinding(Label.AutomationIdProperty, ".");

					return label;
				})
			};
		}
	}
}

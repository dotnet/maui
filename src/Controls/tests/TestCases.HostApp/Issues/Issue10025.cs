namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 10025, "Assigning null to the SelectedItem of the CollectionView in the SelectionChanged event does not clear the selection as expected", PlatformAffected.UWP)]
public class Issue10025 : ContentPage
{
	CollectionView collectionView;
	public Issue10025()
	{
		Label descriptionLabel = new Label
		{
			AutomationId = "DescriptionLabel",
			Text = "The test passes if the SelectedItem is set to null and no visual selection indicator is displayed; otherwise, it fails",
			Margin = new Thickness(10),
		};

		collectionView = new CollectionView
		{
			SelectionMode = SelectionMode.Single,
			ItemsSource = new List<string> { "Item1", "Item2" },
			ItemTemplate = new DataTemplate(() =>
			{
				Label label = new Label();

				label.SetBinding(Label.TextProperty, ".");
				label.SetBinding(Label.AutomationIdProperty, ".");

				return label;
			})
		};

		collectionView.SelectionChanged += SelectionChangedEvent;

		Content = new VerticalStackLayout
		{
			Padding = new Thickness(20),
			Children =
			{
				descriptionLabel,
				collectionView
			}
		};
	}

	private void SelectionChangedEvent(object sender, SelectionChangedEventArgs e)
	{
		collectionView.SelectedItem = null;
	}
}

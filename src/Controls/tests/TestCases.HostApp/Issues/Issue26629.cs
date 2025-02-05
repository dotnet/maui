namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 26629, "ScrollView resizes when content changes", PlatformAffected.All)]
	public class Issue26629 : ContentPage
	{
		public Issue26629()
		{
			var grid = new Grid
			{
				Margin = 40,
				RowSpacing = 16,
				BackgroundColor = Colors.Beige,
				RowDefinitions = new RowDefinitionCollection(
					new RowDefinition(GridLength.Auto),
					new RowDefinition(GridLength.Auto),
					new RowDefinition(GridLength.Auto),
					new RowDefinition(GridLength.Star)),
				ColumnDefinitions = new ColumnDefinitionCollection(new ColumnDefinition(GridLength.Star)),
			};
			var scrollView = new ScrollView { AutomationId = "TheScrollView" };
			var scrollViewVsl = new VerticalStackLayout();
			var button = new Button
			{
				Text = "Add Label",
				AutomationId = "AddLabelButton",
			};

			var sizeLabel = new Label
			{
				AutomationId = "SizeLabel",
			};
			sizeLabel.SetBinding(Label.TextProperty, new Binding(nameof(View.Height), source: scrollView));

			var scrollOffsetLabel = new Label
			{
				AutomationId = "ScrollOffsetLabel",
			};
			scrollOffsetLabel.SetBinding(Label.TextProperty, new Binding(nameof(ScrollView.ScrollY), source: scrollView));

			var i = 0;
			scrollView.BackgroundColor = Colors.LightBlue;
			scrollView.Padding = 16;
			scrollView.VerticalOptions = LayoutOptions.Start;
			scrollViewVsl.Children.Add(CreateLabel("Label0"));
			button.Clicked += (sender, args) =>
			{
				scrollViewVsl.Children.Add(CreateLabel($"Label{++i}"));
			};

			scrollView.Content = scrollViewVsl;
			grid.Add(button, 0, 0);
			grid.Add(sizeLabel, 0, 1);
			grid.Add(scrollOffsetLabel, 0, 2);
			grid.Add(scrollView, 0, 3);

			Content = grid;
		}

		static Label CreateLabel(string automationId)
		{
			return new Label
			{
				Text = "Huge",
				FontSize = 100,
				BackgroundColor = Colors.SlateBlue,
				AutomationId = automationId,
			};
		}
	}
}
namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28657, "iOS - Rotating the simulator would cause clipping on the description text", PlatformAffected.iOS)]
class Issue28657 : ContentPage
{
	public Issue28657()
	{
		var cv = new CollectionView();
		cv.ItemTemplate = new DataTemplate(() =>
		{
			var label = new Label();
			label.BackgroundColor = Colors.DeepSkyBlue;
			label.LineBreakMode = LineBreakMode.CharacterWrap;
			label.SetBinding(Label.TextProperty, ".");
			var vsl = new VerticalStackLayout { Padding = 8 };
			vsl.Add(label);
			vsl.BackgroundColor = Colors.SlateBlue;
			return vsl;
		});
		cv.ItemsSource = Enumerable.Range(0, 30)
			.Select(x => x + ": Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec a diam lectus. Sed sit amet ipsum mauris. Maecenas congue ligula ac quam viverra nec consectetur ante hendrerit.")
			.ToList();
		var label = new Label
		{
			AutomationId = "StubLabel",
			Text = "Rotate the device until you get back to portrait mode. The text should wrap normally on each row.",
		};
		var grid = new Grid
		{
			RowDefinitions = new RowDefinitionCollection(
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star))
		};
		grid.Add(label);
		grid.Add(cv, 0, 1);
		Content = grid;
	}
}

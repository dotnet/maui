namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 19007, "Incomplete Label Display on macOS and IOS When Padding is Applied", PlatformAffected.iOS)]
	public class Issue19007 : ContentPage
	{
		public Issue19007()
		{
			var grid = new Grid();
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });

			var label = new Label
			{
				Background = Colors.Red,
				Text = ".NET Multi-platform App UI (.NET MAUI) is a cross-platform framework for creating mobile and desktop apps with C# and XAML.",
				Padding = new Thickness(20),
				AutomationId = "Label"
			};

			grid.Children.Add(label);
			Grid.SetRow(label, 0);
			Grid.SetColumn(label, 0);

			Content = grid;
		}
	}
}

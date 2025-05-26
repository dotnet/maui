using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29643, "iOS soft keyboard causes CollectionView to jump", PlatformAffected.iOS)]
public class Issue29643 : ContentPage
{
	public Issue29643()
	{
		var grid = new Grid();
		var itemTemplate = new DataTemplate(() =>
		{
			var border = new Border
			{
				StrokeShape = new RoundRectangle { CornerRadius = 16 },
				Margin = new Thickness(16),
				Background = Brush.White
			};
			var vsl = new VerticalStackLayout { Spacing = 16 };
			border.Content = vsl;

			vsl.SetBinding(BindableLayout.ItemsSourceProperty, new Binding("Labels"));
			BindableLayout.SetItemTemplate(vsl, new DataTemplate(() =>
			{
				var entry = new Entry
				{
					FontSize = 18,
					HeightRequest = 40,
					BackgroundColor = Colors.LightGoldenrodYellow,
					Margin = new Thickness(16, 0)
				};

				entry.SetBinding(Entry.PlaceholderProperty, new Binding("."));
				entry.SetBinding(Element.AutomationIdProperty, new Binding("."));

				return entry;
			}));

			return border;
		});
		var cv = new CollectionView {
			AutomationId = "CV",
			ItemsSource = new List<Items>
			{
				new(15),
				new(3),
				new(15),
				new(3),
				new(15),
				new(3),
				new(15),
				new(3),
			},
			ItemTemplate = itemTemplate
		};
		grid.Add(cv);
		Content = grid;
		Background = Brush.LightSeaGreen;
	}

	private class Items
	{
		private static int IdCounter;

		public List<string> Labels { get; }

		public Items(int count)
		{
			Labels = [];
			for (int i = 0; i < count; i++)
			{
				Labels.Add($"Input{++IdCounter}");
			}
		}
	}
}
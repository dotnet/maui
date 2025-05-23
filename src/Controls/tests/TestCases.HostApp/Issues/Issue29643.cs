using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29643, "iOS soft keyboard causes CollectionView to jump", PlatformAffected.iOS)]
public class Issue29643 : ContentPage
{
	public Issue29643()
	{
		var grid = new Grid();
		var cv = new CollectionView { ItemsSource = new List<string>
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
				"Item 10"
			},
			ItemTemplate = new SmallBigTemplateSelector()
		};
		grid.Add(cv);
		Content = grid;
		Background = Brush.LightSeaGreen;
	}
	
	private class SmallBigTemplateSelector : DataTemplateSelector
	{
		private readonly DataTemplate _smallTemplate = new(() => CreateTemplateContent(3));
		private readonly DataTemplate _bigTemplate = new(() => CreateTemplateContent(15));

		static object CreateTemplateContent(int inputCount)
		{
			var border = new Border
			{
				StrokeShape = new RoundRectangle { CornerRadius = 16 },
				Margin = new Thickness(16),
				Background = Brush.White
			};
			var vsl = new VerticalStackLayout
			{
				Spacing = 16
			};
			border.Content = vsl;
			
			var label = new Label
			{
				FontSize = 24,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};
			
			label.SetBinding(Label.TextProperty, ".");
			vsl.Add(label);

			for (int i = 0; i < inputCount; i++)
			{
				var entry = new Entry
				{
					Placeholder = $"Input {i + 1}",
					FontSize = 18,
					HeightRequest = 40,
					BackgroundColor = Colors.LightGoldenrodYellow,
					Margin = new Thickness(16, 0)
				};
				vsl.Add(entry);
			}
			
			return border;
		}
		
		protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
		{
			var str = (string)item;
			var numericValue = int.Parse(str.Split(' ')[1]);
			return numericValue % 2 == 0 ? _smallTemplate : _bigTemplate;
		}
	}
}
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29595, "iOS CV: GridItemsLayout not left-aligning a single item", PlatformAffected.iOS)]
public class Issue29595 : ContentPage
{
	readonly ObservableCollection<string> _items = [];

	public Issue29595()
	{
		var grid = new Grid();

		var cv = new CollectionView
		{
			Margin = 10,
			VerticalOptions = LayoutOptions.Fill,
			ItemsLayout = new GridItemsLayout(3, ItemsLayoutOrientation.Vertical)
			{
				HorizontalItemSpacing = 8,
				VerticalItemSpacing = 8
			},
			ItemTemplate = GetItemTemplate(),
			ItemsSource = _items
		};

		grid.Add(cv);

		Content = grid;
	}

	static DataTemplate GetItemTemplate(double fontSize = 14)
	{
		return new DataTemplate(() =>
		{
			var border = new Border
			{
				StrokeThickness = 0,
				StrokeShape = new RoundRectangle { CornerRadius = 32 }
			};

			var innerGrid = new Grid
			{
				BackgroundColor = Colors.WhiteSmoke,
				RowDefinitions =
				{
					new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
					new RowDefinition { Height = GridLength.Auto }
				}
			};

			var image = new FFImageLoadingStubImage
			{
				Aspect = Aspect.AspectFill,
				Source = "dotnet_bot.png"
			};
			Grid.SetRow(image, 0);

			var label = new Label
			{
				Text = "Test",
				AutomationId = "StubLabel",
				FontSize = fontSize,
				FontFamily = "OpenSansRegular",
				TextColor = Colors.Black,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};
			Grid.SetRow(label, 1);

			innerGrid.Add(image);
			innerGrid.Add(label);

			border.Content = innerGrid;
			return border;
		});
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await Task.Delay(300);
		_items.Add("item1");
	}
}

/// <summary>
/// This is a normal image which simulates FFImageLoading loading behavior which may trigger an additional measure pass
/// once the image is loaded, and the new measure could be different from the previous one.
/// </summary>
file class FFImageLoadingStubImage : Image
{
	int counter;

	protected async override void OnPropertyChanged(string propertyName = null)
	{
		base.OnPropertyChanged(propertyName);

		if (propertyName == SourceProperty.PropertyName)
		{
			++counter;
			await Task.Delay(100);
			InvalidateMeasure();
		}
	}

	protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
	{
		var desiredSize = base.OnMeasure(double.PositiveInfinity, double.PositiveInfinity);
		var desiredWidth = double.IsNaN(desiredSize.Request.Width) ? 0 : desiredSize.Request.Width + counter;
		var desiredHeight = double.IsNaN(desiredSize.Request.Height) ? 0 : desiredSize.Request.Height;

		if (double.IsNaN(widthConstraint))
			widthConstraint = 0;
		if (double.IsNaN(heightConstraint))
			heightConstraint = 0;

		if (Math.Abs(desiredWidth) < double.Epsilon || Math.Abs(desiredHeight) < double.Epsilon)
			return new SizeRequest(new Size(0, 0));

		if (double.IsPositiveInfinity(widthConstraint) && double.IsPositiveInfinity(heightConstraint))
		{
			return new SizeRequest(new Size(desiredWidth, desiredHeight));
		}

		if (double.IsPositiveInfinity(widthConstraint))
		{
			var factor = heightConstraint / desiredHeight;
			return new SizeRequest(new Size(desiredWidth * factor, desiredHeight * factor));
		}

		if (double.IsPositiveInfinity(heightConstraint))
		{
			var factor = widthConstraint / desiredWidth;
			return new SizeRequest(new Size(desiredWidth * factor, desiredHeight * factor));
		}

		var fitsWidthRatio = widthConstraint / desiredWidth;
		var fitsHeightRatio = heightConstraint / desiredHeight;

		if (double.IsNaN(fitsWidthRatio))
			fitsWidthRatio = 0;
		if (double.IsNaN(fitsHeightRatio))
			fitsHeightRatio = 0;

		if (Math.Abs(fitsWidthRatio) < double.Epsilon && Math.Abs(fitsHeightRatio) < double.Epsilon)
			return new SizeRequest(new Size(0, 0));

		if (Math.Abs(fitsWidthRatio) < double.Epsilon)
			return new SizeRequest(new Size(desiredWidth * fitsHeightRatio, desiredHeight * fitsHeightRatio));

		if (Math.Abs(fitsHeightRatio) < double.Epsilon)
			return new SizeRequest(new Size(desiredWidth * fitsWidthRatio, desiredHeight * fitsWidthRatio));

		var ratioFactor = Math.Min(fitsWidthRatio, fitsHeightRatio);

		return new SizeRequest(new Size(desiredWidth * ratioFactor, desiredHeight * ratioFactor));
	}
}

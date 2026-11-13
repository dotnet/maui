using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31680, "System.IndexOutOfRangeException when scrolling CollectionView with image CarouselView", PlatformAffected.Android)]

public class Issue31680 : ContentPage
{
    ObservableCollection<Listing> _items = new();

    public ObservableCollection<Listing> Items
    {
        get => _items;
        set
        {
            _items = value;
            OnPropertyChanged();
        }
    }

    public Issue31680()
    {
        BindingContext = this;

        var collectionView = new CollectionView
		{
			VerticalOptions = LayoutOptions.Fill,
			HorizontalOptions = LayoutOptions.Fill,
			AutomationId = "MainCollectionView"
        };

        collectionView.SetBinding(ItemsView.ItemsSourceProperty, nameof(Items));

        collectionView.ItemTemplate = new DataTemplate(() =>
        {
            var border = new Border
            {
                StrokeShape = new RoundRectangle { CornerRadius = 8 },
                Padding = 8,
                BackgroundColor = Colors.White,
                Margin = new Thickness(4)
            };

            var carouselView = new CarouselView
            {
                HeightRequest = 200,
                VerticalOptions = LayoutOptions.Fill,
            };
            carouselView.SetBinding(ItemsView.ItemsSourceProperty, "ImageUrls");

            carouselView.ItemTemplate = new DataTemplate(() =>
            {
                var image = new Image
                {
                    Aspect = Aspect.AspectFill
                };
                image.SetBinding(Image.SourceProperty, ".");
                return image;
            });

            var titleLabel = new Label
            {
                FontAttributes = FontAttributes.Bold,
                FontSize = 16
            };
            titleLabel.SetBinding(Label.TextProperty, "Title");

            var addressLabel = new Label
            {
                FontSize = 12
            };
            addressLabel.SetBinding(Label.TextProperty, "Address");

            var priceLabel = new Label
            {
                HorizontalOptions = LayoutOptions.End
            };
            priceLabel.SetBinding(Label.TextProperty, new Binding("Price", stringFormat: "€{0:N0}"));

            var favoriteButton = new Button { Text = "Favorite" };
            var openButton = new Button { Text = "Open" };

            var buttonLayout = new HorizontalStackLayout
            {
                Spacing = 10,
                Children = { favoriteButton, openButton }
            };

            var stack = new VerticalStackLayout
            {
                Spacing = 6,
                Children = { carouselView, titleLabel, addressLabel, priceLabel, buttonLayout }
            };

            border.Content = stack;

            return border;
        });

		collectionView.Footer = new Label {Text = "Footer", AutomationId="Footer"};

       var label = new Label
		{
			Text = "CollectionView with CarouselView Items",
			FontSize = 20,
			AutomationId="TitleLabel",
			FontAttributes = FontAttributes.Bold,
			HorizontalOptions = LayoutOptions.Center,
			Margin = new Thickness(0, 10, 0, 10)
		};

		Grid.SetRow(label, 0);
		Grid.SetRow(collectionView, 1);

		Content = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },   
				new RowDefinition { Height = GridLength.Star }
			},
			Children =
			{
				label,
				collectionView
			}
		};

        LoadData();
    }

    void LoadData()
    {
        var random = new Random();
        var items = new ObservableCollection<Listing>();

        for (int i = 1; i <= 10; i++)
        {
            var imageUrls = new List<string>();
            int imageCount = random.Next(3, 6);

            for (int j = 0; j < imageCount; j++)
            {
                imageUrls.Add($"https://picsum.photos/id/{random.Next(1, 1084)}/400/300");
            }

            items.Add(new Listing
            {
                Id = Guid.NewGuid().ToString(),
                Title = $"Beautiful Property {i}",
                Address = $"Street Address {i}, City {i}",
                Price = random.Next(100000, 1000000),
                ImageUrls = imageUrls
            });
        }

        Items = items;
    }
}

public class Listing
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public List<string> ImageUrls { get; set; } = new();
}


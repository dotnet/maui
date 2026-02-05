using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32017, "Image shifts downward when window is resized smaller", PlatformAffected.UWP)]
public class Issue32017 : ContentPage
{
	double width = 0;
	double height = 0;
	double originalWindowWidth = 0;
	double originalWindowHeight = 0;
	CarouselView _carouselView;

	public Issue32017()
	{
		_carouselView = new CarouselView
		{
			AutomationId = "TestCarouselView",
			WidthRequest = 350,
			HeightRequest = 570,
			HorizontalOptions = LayoutOptions.Fill,
			VerticalOptions = LayoutOptions.Fill,
			Loop = false,
			ItemTemplate = new DataTemplate(() =>
			{
				var grid = new Grid
				{
					Padding = 50,
					WidthRequest = 340,
					HorizontalOptions = LayoutOptions.Fill,
					VerticalOptions = LayoutOptions.Fill
				};

				var image = new Image
				{
					Aspect = Aspect.AspectFill,
					VerticalOptions = LayoutOptions.Fill,
					HorizontalOptions = LayoutOptions.Fill,
					AutomationId = "RecipeImage"
				};
				image.SetBinding(Image.SourceProperty, nameof(Issue32017Recipe.ImageUrl));

				var label = new Label
				{
					Padding = 10,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					LineBreakMode = LineBreakMode.WordWrap,
					MaxLines = 3,
					FontSize = 16,
					AutomationId = "RecipeLabel"
				};
				label.SetBinding(Label.TextProperty, nameof(Issue32017Recipe.RecipeName));

				grid.Children.Add(image);
				grid.Children.Add(label);

				return grid;
			})
		};

		var viewModel = new Issue32017RecipeViewModel();
		_carouselView.ItemsSource = viewModel.Items;

		// Create a button to reduce window width
		var reduceWidthButton = new Button
		{
			Text = "Reduce Window Width",
			AutomationId = "ReduceWidthButton"
		};

		reduceWidthButton.Clicked += (sender, e) =>
		{
			if (Window != null)
			{
				// Store original size on first click
				if (originalWindowWidth == 0)
				{
					originalWindowWidth = Window.Width;
					originalWindowHeight = Window.Height;
				}

				var newWidth = Window.Width / 2;
				Window.Width = newWidth;
			}
		};

		// Create a button to restore window to original size
		var restoreWidthButton = new Button
		{
			Text = "Restore Window Size",
			AutomationId = "RestoreWidthButton"
		};

		restoreWidthButton.Clicked += (sender, e) =>
		{
			if (Window != null && originalWindowWidth > 0)
			{
				Window.Width = originalWindowWidth;
				Window.Height = originalWindowHeight;
			}
		};

		Content = new StackLayout
		{
			Margin = 20,
			Children =
			{
				new Label
				{
					Text = "Your recipes",
					FontSize = 30,
					FontAttributes = FontAttributes.Bold,
					AutomationId = "TitleLabel"
				},
				reduceWidthButton,
				restoreWidthButton,
				_carouselView
			}
		};
	}

	protected override void OnSizeAllocated(double width, double height)
	{
		base.OnSizeAllocated(width, height);
		if (width != this.width || height != this.height)
		{
			this.width = width;
			this.height = height;
			if (width > height)
			{
				_carouselView.HeightRequest = 200;
				_carouselView.WidthRequest = width - 100;
			}
			else
			{
				_carouselView.HeightRequest = height - 150;
				_carouselView.WidthRequest = 350;
			}
		}
	}

	public class Issue32017Recipe
	{
		public string RecipeName { get; set; }
		public string ImageUrl { get; set; }
	}

	public class Issue32017RecipeViewModel
	{
		public ObservableCollection<Issue32017Recipe> Items { get; set; }

		public Issue32017RecipeViewModel()
		{
			Items = new ObservableCollection<Issue32017Recipe>
			{
				new Issue32017Recipe { RecipeName = "Delicious Pasta", ImageUrl = "https://www.koreanbapsang.com/wp-content/uploads/2019/10/DSC_1183-2.jpg" },
				new Issue32017Recipe { RecipeName = "Grilled Chicken", ImageUrl = "https://mblogthumb-phinf.pstatic.net/MjAxNzEyMjlfMTQ0/MDAxNTE0NTM2MTcwMzM5.feATDxTPqCzmnlXqheAC87Fk0pMo_9uz3fj8FDu1zgwg.qdar-w_Xggvqp9IB8bPMwGMRaCt_CvGgDfqFCwbt6Zog.JPEG.sundoong2/image_1532650841514535869603.jpg?type=w800" },
				new Issue32017Recipe { RecipeName = "Fresh Salad", ImageUrl = "https://www.inspiredtaste.net/wp-content/uploads/2018/09/Easy-Oven-Baked-Salmon-Recipe-2-1200.jpg" },
			};
		}
	}
}
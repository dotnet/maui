namespace Xamarin.Forms.Controls.GalleryPages.GradientGalleries
{
	public class GradientsGallery : ContentPage
	{
		public GradientsGallery()
		{
			Title = "Brushes Galleries";

			var descriptionLabel =
				new Label { Text = "Brushes Galleries", Margin = new Thickness(2, 2, 2, 2) };

			var button = new Button
			{
				Text = "Enable Brushes",
				AutomationId = "EnableBrushes"
			};
			button.Clicked += ButtonClicked;

			var navigationBarButton = new Button
			{
				FontSize = 10,
				HeightRequest = Device.RuntimePlatform == Device.Android ? 40 : 30,
				Text = "Gradient NavigationPage Gallery"
			};

			navigationBarButton.Clicked += (sender, args) =>
			{
				Application.Current.MainPage = new GradientNavigationPageGallery();
			};

			var tabsButton = new Button
			{
				FontSize = 10,
				HeightRequest = Device.RuntimePlatform == Device.Android ? 40 : 30,
				Text = "Gradient Tabs Gallery"
			};

			tabsButton.Clicked += (sender, args) =>
			{
				Navigation.PushAsync(new GradientTabsGallery());
			};

			var layout = new StackLayout
			{
				Children =
				{
					descriptionLabel,
					button,
					GalleryBuilder.NavButton("Gradient Views", () =>
						new GradientViewsGallery(), Navigation),
					GalleryBuilder.NavButton("SolidColorBrush Converter Gallery", () =>
						new SolidColorBrushConverterGallery(), Navigation),
					GalleryBuilder.NavButton("LinearGradientBrush Points Gallery", () =>
						new LinearGradientPointsGallery(), Navigation),
					GalleryBuilder.NavButton("LinearGradientBrush Explorer", () =>
						new LinearGradientExplorerGallery(), Navigation),
					GalleryBuilder.NavButton("RadialGradient Explorer", () =>
						new RadialGradientExplorerGallery(), Navigation),
					GalleryBuilder.NavButton("Bindable Brush Gallery", () =>
						new BindableBrushGallery(), Navigation),
					GalleryBuilder.NavButton("Animate Brush Gallery", () =>
						new AnimateBrushGallery(), Navigation),
					navigationBarButton,
					tabsButton,
					GalleryBuilder.NavButton("Shapes using Brush Gallery", () =>
						new ShapesBrushGallery(), Navigation),
					GalleryBuilder.NavButton("CSS Gradients Explorer", () =>
						new CssGradientsGallery(), Navigation),
					GalleryBuilder.NavButton("CSS Gradients Playground", () =>
						new CssGradientsPlayground(), Navigation)
				}
			};

			if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
			{
				layout.Children.Add(GalleryBuilder.NavButton("Gradient Views (Visual)", () =>
				new VisualGradientViewsGallery(), Navigation));
			}

			Content = new ScrollView
			{
				Content = layout
			};
		}
		
		void ButtonClicked(object sender, System.EventArgs e)
		{
			var button = sender as Button;

			button.Text = "Brushes Enabled!";
			button.TextColor = Color.Black;
			button.IsEnabled = false;

			Device.SetFlags(new[] { ExperimentalFlags.BrushExperimental, ExperimentalFlags.ExpanderExperimental, ExperimentalFlags.ShapesExperimental, ExperimentalFlags.SwipeViewExperimental });
		}
	}
}
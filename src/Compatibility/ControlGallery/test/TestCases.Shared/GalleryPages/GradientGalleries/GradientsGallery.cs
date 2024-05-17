using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.GradientGalleries
{
	public class GradientsGallery : ContentPage
	{
		public GradientsGallery()
		{
			Title = "Brushes Galleries";

			var descriptionLabel =
				new Label { Text = "Brushes Galleries", Margin = new Thickness(2, 2, 2, 2) };

			var navigationBarButton = new Button
			{
				FontSize = 10,
				HeightRequest = DeviceInfo.Platform == DevicePlatform.Android ? 40 : 30,
				Text = "Gradient NavigationPage Gallery"
			};

			navigationBarButton.Clicked += (sender, args) =>
			{
				Application.Current.MainPage = new GradientNavigationPageGallery();
			};

			var tabsButton = new Button
			{
				FontSize = 10,
				HeightRequest = DeviceInfo.Platform == DevicePlatform.Android ? 40 : 30,
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
					GalleryBuilder.NavButton("GradientBrush Offset Gallery", () =>
						new GradientBrushDefaultOffsetGallery(), Navigation),
					GalleryBuilder.NavButton("Bindable Brush Gallery", () =>
						new BindableBrushGallery(), Navigation),
					GalleryBuilder.NavButton("Update Brush Colors Gallery", () =>
						new UpdateGradientColorGallery(), Navigation),
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

			if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
			{
				layout.Children.Add(GalleryBuilder.NavButton("Gradient Views (Visual)", () =>
				new VisualGradientViewsGallery(), Navigation));
			}

			Content = new ScrollView
			{
				Content = layout
			};
		}
	}
}
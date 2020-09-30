using System;
using Xamarin.Forms.Shapes;

namespace Xamarin.Forms.Controls.GalleryPages.ShapesGalleries
{
	public class AddRemoveClipGallery : ContentPage
	{
		readonly Image _image;
		readonly Grid _grid;

		public AddRemoveClipGallery()
		{
			Title = "Add/Remove Clip Gallery";

			var layout = new StackLayout
			{
				Padding = 12
			};

			var imageInfo = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				Text = "Image"
			};

			_image = new Image
			{
				Aspect = Aspect.AspectFill,
				Source = new FileImageSource { File = "crimson.jpg" },
				HorizontalOptions = LayoutOptions.Center,
				HeightRequest = 150,
				WidthRequest = 150
			};

			var gridInfo = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				Text = "Grid"
			};

			_grid = new Grid
			{
				BackgroundColor = Color.Red,
				HorizontalOptions = LayoutOptions.Center,
				HeightRequest = 150,
				WidthRequest = 150
			};

			var buttonLayout = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				HorizontalOptions = LayoutOptions.Center
			};

			var addButton = new Button
			{
				Text = "Add EllipseGeometry",
				WidthRequest = 150
			};

			addButton.Clicked += OnAddButtonClicked;

			var removeButton = new Button
			{
				Text = "Remove EllipseGeometry",
				WidthRequest = 150
			};

			removeButton.Clicked += OnRemoveButtonClicked;

			buttonLayout.Children.Add(addButton);
			buttonLayout.Children.Add(removeButton);

			layout.Children.Add(imageInfo);
			layout.Children.Add(_image);
			layout.Children.Add(gridInfo);
			layout.Children.Add(_grid);
			layout.Children.Add(buttonLayout);

			Content = layout;
		}

		void OnAddButtonClicked(object sender, EventArgs e)
		{
			var ellipseGeometry = new EllipseGeometry
			{
				Center = new Point(75, 75),
				RadiusX = 60,
				RadiusY = 60
			};

			_image.Clip = _grid.Clip = ellipseGeometry;
		}

		void OnRemoveButtonClicked(object sender, EventArgs e)
		{
			_image.Clip = _grid.Clip = null;
		}
	}
}
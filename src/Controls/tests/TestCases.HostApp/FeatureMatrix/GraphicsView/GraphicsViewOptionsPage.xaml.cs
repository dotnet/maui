using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public partial class GraphicsViewOptionsPage : ContentPage
{
	private GraphicsViewViewModel _viewModel;

	public GraphicsViewOptionsPage(GraphicsViewViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	private void ApplyButton_Clicked(object sender, EventArgs e)
	{
		Navigation.PopAsync();
	}

	private void OnDrawableChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton != null && Enum.TryParse(radioButton.Content.ToString(), out DrawableType selectedDrawable))
		{
			_viewModel.SelectedDrawable = selectedDrawable;

			// Update Drawable property based on selected shape
			IDrawable drawable = selectedDrawable switch
			{
				DrawableType.Square => new SquareDrawable(_viewModel),
				DrawableType.Triangle => new TriangleDrawable(_viewModel),
				DrawableType.Ellipse => new EllipseDrawable(_viewModel),
				DrawableType.Line => new LineDrawable(_viewModel),
				DrawableType.String => new StringDrawable(_viewModel),
				DrawableType.Image => new ImageDrawable(_viewModel),
				DrawableType.TransparentEllipse => new TransparentEllipseDrawable(_viewModel),
				_ => null
			};

			_viewModel.UpdateDrawable(drawable);

			// Capture RectF dimensions after rendering
			if (drawable != null)
			{
				double width = _viewModel.WidthRequest;
				double height = _viewModel.HeightRequest;
				// Use default X=0, Y=0 for initial values since the actual values will be set during Draw()
				_viewModel.UpdateDrawableDimensions(0, 0, width, height);
			}
		}
	}

	private void OnIsEnabledCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		_viewModel.IsEnabled = IsEnabledTrueRadio.IsChecked;
	}

	private void OnIsVisibleCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (IsVisibleTrueRadio.IsChecked)
			_viewModel.IsVisible = true;
		else
			_viewModel.IsVisible = false;
	}

	private void OnShadowInputChanged(object sender, TextChangedEventArgs e)
	{
		var input = ShadowInputEntry.Text;
		var parts = input.Split(',');

		// Ensure Shadow is initialized
		if (_viewModel.Shadow == null)
		{
			_viewModel.Shadow = new Shadow();
		}

		if (parts.Length == 4 &&
			double.TryParse(parts[0], out double offsetX) &&
			double.TryParse(parts[1], out double offsetY) &&
			double.TryParse(parts[2], out double radius) &&
			double.TryParse(parts[3], out double opacity))
		{
			_viewModel.Shadow.Offset = new Point(offsetX, offsetY);
			_viewModel.Shadow.Radius = (float)radius;
			_viewModel.Shadow.Opacity = (float)opacity;
		}
		else
		{
			// Handle invalid input gracefully
			_viewModel.Shadow.Offset = new Point(0, 0);
			_viewModel.Shadow.Radius = 0;
			_viewModel.Shadow.Opacity = 0;
		}
	}
}

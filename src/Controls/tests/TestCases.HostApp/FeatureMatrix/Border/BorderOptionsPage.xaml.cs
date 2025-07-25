using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample;

public partial class OptionsPage : ContentPage
{
	private BorderViewModel _viewModel;

	public OptionsPage(BorderViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	private void ApplyButton_Clicked(object sender, EventArgs e)
	{
		Navigation.PopAsync();
	}
	private void OnPaddingChanged(object sender, TextChangedEventArgs e)
	{

		string[] parts = PaddingEntry.Text.Split(',');
		if (parts.Length == 4 &&
			double.TryParse(parts[0], out double left) &&
			double.TryParse(parts[1], out double top) &&
			double.TryParse(parts[2], out double right) &&
			double.TryParse(parts[3], out double bottom))
		{
			_viewModel.Padding = new Thickness(left, top, right, bottom);
		}
	}
	private void OnStrokeThicknessChanged(object sender, TextChangedEventArgs e)
	{
		if (double.TryParse(StrokeThicknessEntry.Text, out double strokeThickness))
		{
			_viewModel.StrokeThickness = strokeThickness;
		}
	}
	private void OnStrokeColorClicked(object sender, EventArgs e)
	{
		if (sender is Button button && button.BackgroundColor != Colors.Transparent)
		{
			_viewModel.Stroke = button.BackgroundColor;
		}
	}
	private void ShapeChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!e.Value || !(sender is RadioButton rb) || !(BindingContext is BorderViewModel vm))
			return;

		vm.StrokeShape = rb.Content.ToString() switch
		{
			"Rectangle" => vm.CreateRectangleShape(),
			"RoundRectangle" => vm.CreateRoundRectangleShape(),
			"Ellipse" => vm.CreateEllipseShape(),
			"Path" => vm.CreatePathShape(),
			"Polygon" => vm.CreatePolygonShape(),
			_ => new Rectangle()
		};
	}
	private void OnDashOffsetTextChanged(object sender, TextChangedEventArgs e)
	{
		if (BindingContext is BorderViewModel viewModel && !string.IsNullOrEmpty(e.NewTextValue))
		{
			try
			{
				if (double.TryParse(e.NewTextValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double offset))
				{
					viewModel.StrokeDashOffset = offset;
				}
			}
			catch
			{
				// Optionally handle invalid input
				DashOffsetEntry.Text = "0"; // Reset to default value
			}
		}
	}
	private void StrokeDashArrayEntry_TextChanged(object sender, TextChangedEventArgs e)
	{
		if (BindingContext is BorderViewModel viewModel)
		{
			try
			{
				var values = e.NewTextValue
					.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
					.Select(s => double.Parse(s.Trim(), System.Globalization.CultureInfo.InvariantCulture))
					.ToList();

				viewModel.StrokeDashArray = new DoubleCollection(values.ToArray());
			}
			catch
			{
				// Optional: handle invalid input (e.g., show error, ignore change)
			}
		}
	}
	private void LineCapChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!e.Value)
			return;

		if (sender == FlatLineCapRadio)
			_viewModel.StrokeLineCap = Microsoft.Maui.Controls.Shapes.PenLineCap.Flat;
		else if (sender == RoundLineCapRadio)
			_viewModel.StrokeLineCap = Microsoft.Maui.Controls.Shapes.PenLineCap.Round;
		else if (sender == SquareLineCapRadio)
			_viewModel.StrokeLineCap = Microsoft.Maui.Controls.Shapes.PenLineCap.Square;
	}
	private void OnMiterLimitChanged(object sender, TextChangedEventArgs e)
	{
		if (_viewModel == null)
			return;

		if (double.TryParse(MiterLimitEntry.Text, out var miterLimit))
		{
			_viewModel.StrokeMiterLimit = miterLimit;
		}
	}

	private void OnLineJoinChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender is RadioButton rb && rb.IsChecked)
		{
			switch (rb.Content?.ToString())
			{
				case "Miter":
					_viewModel.StrokeLineJoin = PenLineJoin.Miter;
					break;
				case "Bevel":
					_viewModel.StrokeLineJoin = PenLineJoin.Bevel;
					break;
				case "Round":
					_viewModel.StrokeLineJoin = PenLineJoin.Round;
					break;
			}
		}
	}
	private void OnShadowEntryChanged(object sender, TextChangedEventArgs e)
	{
		if (BindingContext is BorderViewModel viewModel)
		{
			try
			{
				double offsetX = double.Parse(OffsetXEntry.Text);
				double offsetY = double.Parse(OffsetYEntry.Text);
				double radius = double.Parse(RadiusEntry.Text);
				float opacity = float.Parse(OpacityEntry.Text, System.Globalization.CultureInfo.InvariantCulture);

				viewModel.UpdateShadow(offsetX, offsetY, radius, opacity);
			}
			catch
			{
				// Optional: Show validation error
			}
		}
	}
	private void OnContentOptionChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender is RadioButton rb && rb.IsChecked)
		{
			switch (rb.Content.ToString())
			{
				case "Label":
					_viewModel.SetLabelContent();
					break;
				case "Button":
					_viewModel.SetButtonContent();
					break;
				case "Image":
					_viewModel.SetImageContent();
					break;
			}
		}
	}
}
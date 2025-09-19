namespace Maui.Controls.Sample;

public partial class ShapesOptionsPage : ContentPage
{
	private ShapesViewModel _viewModel;
	public ShapesOptionsPage(ShapesViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;

		// Set initial property section visibility
		UpdatePropertySectionVisibility(_viewModel.SelectedShapeType);
	}
	private void OnShapeTypeChanged(object sender, CheckedChangedEventArgs e)
	{
		if (e.Value && sender is RadioButton radioButton && radioButton.Content is string shapeTypeName)
		{
			if (Enum.TryParse<ShapeType>(shapeTypeName, out var shapeType))
			{
				_viewModel.SelectedShapeType = shapeType;
				UpdatePropertySectionVisibility(shapeType);
			}
		}
	}
	private void UpdatePropertySectionVisibility(ShapeType shapeType)
	{
		// Hide all property sections first
		RectanglePropertiesSection.IsVisible = false;
		EllipsePropertiesSection.IsVisible = false;
		LinePropertiesSection.IsVisible = false;
		PolygonPropertiesSection.IsVisible = false;
		PolylinePropertiesSection.IsVisible = false;
		PathPropertiesSection.IsVisible = false;

		// Show the relevant property section
		switch (shapeType)
		{
			case ShapeType.Rectangle:
				RectanglePropertiesSection.IsVisible = true;
				break;
			case ShapeType.Ellipse:
				EllipsePropertiesSection.IsVisible = true;
				break;
			case ShapeType.Line:
				LinePropertiesSection.IsVisible = true;
				break;
			case ShapeType.Polygon:
				PolygonPropertiesSection.IsVisible = true;
				break;
			case ShapeType.Polyline:
				PolylinePropertiesSection.IsVisible = true;
				break;
			case ShapeType.Path:
				PathPropertiesSection.IsVisible = true;
				break;
		}
	}

	private void OnFillColorChanged(object sender, CheckedChangedEventArgs e)
	{
		if (e.Value && sender is RadioButton radioButton && radioButton.Content is string colorName)
		{
			if (colorName.ToLower(System.Globalization.CultureInfo.InvariantCulture) == "none")
			{
				_viewModel.HasFillColor = false;
				_viewModel.FillColor = null;
			}
			else
			{
				_viewModel.HasFillColor = true;
				var color = GetColorByName(colorName);
				_viewModel.FillColor = color;
			}
		}
	}

	private void OnStrokeColorChanged(object sender, CheckedChangedEventArgs e)
	{
		if (e.Value && sender is RadioButton radioButton && radioButton.Content is string colorName)
		{
			var color = GetColorByName(colorName);
			_viewModel.StrokeColor = color;
		}
	}

	private Microsoft.Maui.Graphics.Color GetColorByName(string colorName)
	{
		return colorName.ToLower(System.Globalization.CultureInfo.InvariantCulture) switch
		{
			"red" => Colors.Red,
			"blue" => Microsoft.Maui.Graphics.Colors.Blue,
			"green" => Microsoft.Maui.Graphics.Colors.Green,
			"yellow" => Microsoft.Maui.Graphics.Colors.Yellow,
			"orange" => Microsoft.Maui.Graphics.Colors.Orange,
			"purple" => Microsoft.Maui.Graphics.Colors.Purple,
			"pink" => Microsoft.Maui.Graphics.Colors.Pink,
			"cyan" => Microsoft.Maui.Graphics.Colors.Cyan,
			"brown" => Microsoft.Maui.Graphics.Colors.Brown,
			"gold" => Microsoft.Maui.Graphics.Colors.Gold,
			"black" => Microsoft.Maui.Graphics.Colors.Black,
			"gray" => Microsoft.Maui.Graphics.Colors.Gray,
			"white" => Microsoft.Maui.Graphics.Colors.White,
			_ => Microsoft.Maui.Graphics.Colors.Black
		};
	}
	private void ApplyButton_Clicked(object sender, EventArgs e)
	{
		Navigation.PopAsync();
	}
}
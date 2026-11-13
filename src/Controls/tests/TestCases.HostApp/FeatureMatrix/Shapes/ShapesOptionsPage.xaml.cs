using Microsoft.Maui.Controls.Shapes;

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
		RectanglePropertiesSection.IsVisible = shapeType == ShapeType.Rectangle;
		RoundRectanglePropertiesSection.IsVisible = shapeType == ShapeType.RoundRectangle;
		EllipsePropertiesSection.IsVisible = shapeType == ShapeType.Ellipse;
		LinePropertiesSection.IsVisible = shapeType == ShapeType.Line;
		PolygonPropertiesSection.IsVisible = shapeType == ShapeType.Polygon;
		PolylinePropertiesSection.IsVisible = shapeType == ShapeType.Polyline;
		PathPropertiesSection.IsVisible = shapeType == ShapeType.Path;
	}

	private void OnFillColorChanged(object sender, CheckedChangedEventArgs e)
	{
		if (e.Value && sender is RadioButton radioButton && radioButton.Content is string colorName)
		{
			if (colorName.ToLower(System.Globalization.CultureInfo.InvariantCulture) == "none")
			{
				_viewModel.HasFillColor = false;
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

	private void OnAspectChanged(object sender, CheckedChangedEventArgs e)
	{
		if (e.Value && sender is RadioButton radioButton && radioButton.Content is string aspectName)
		{
			if (Enum.TryParse<Stretch>(aspectName, out var aspect))
				_viewModel.Aspect = aspect;
		}
	}

	private void OnStrokeLineCapChanged(object sender, CheckedChangedEventArgs e)
	{
		if (e.Value && sender is RadioButton radioButton && radioButton.Content is string capName)
		{
			if (Enum.TryParse<PenLineCap>(capName, out var cap))
				_viewModel.StrokeLineCap = cap;
		}
	}

	private void OnStrokeLineJoinChanged(object sender, CheckedChangedEventArgs e)
	{
		if (e.Value && sender is RadioButton radioButton && radioButton.Content is string joinName)
		{
			if (Enum.TryParse<PenLineJoin>(joinName, out var join))
				_viewModel.StrokeLineJoin = join;
		}
	}

	private void OnFillRuleChanged(object sender, CheckedChangedEventArgs e)
	{
		if (e.Value && sender is RadioButton radioButton && radioButton.Content is string fillRuleName)
		{
			if (Enum.TryParse<FillRule>(fillRuleName, out var fillRule))
				_viewModel.FillRule = fillRule;
		}
	}

	private static Microsoft.Maui.Graphics.Color GetColorByName(string colorName)
	{
		return colorName.ToLower(System.Globalization.CultureInfo.InvariantCulture) switch
		{
			"red" => Microsoft.Maui.Graphics.Colors.Red,
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public enum DrawableType
{
	Square,
	Triangle,
	Ellipse,
	Line,
	String,
	Image,
	TransparentEllipse,
}

public class GraphicsViewViewModel : INotifyPropertyChanged
{
	public Action RequestInvalidate { get; set; }
	public string DrawableTypeLabel => SelectedDrawable.ToString();
	private DrawableType _selectedDrawable = DrawableType.Square;
	private bool _isEnabled = true;
	private bool _isVisible = true;
	private Color _currentDrawColor = Colors.Blue;
	private Random _random = new Random();

	public event PropertyChangedEventHandler PropertyChanged;

	public Color CurrentDrawColor
	{
		get => _currentDrawColor;
		set
		{
			if (_currentDrawColor != value)
			{
				_currentDrawColor = value;
				OnPropertyChanged();
			}
		}
	}

	public DrawableType SelectedDrawable
	{
		get => _selectedDrawable;
		set
		{
			if (_selectedDrawable != value)
			{
				_selectedDrawable = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(DrawableTypeLabel));
				UpdateDrawable();
				RequestInvalidate?.Invoke();
			}
		}
	}

	public bool IsEnabled
	{
		get => _isEnabled;
		set
		{
			if (_isEnabled != value)
			{
				_isEnabled = value;
				OnPropertyChanged();
			}
		}
	}

	public bool IsVisible
	{
		get => _isVisible;
		set
		{
			if (_isVisible != value)
			{
				_isVisible = value;
				OnPropertyChanged();
			}
		}
	}

	private string _interactionEvent = "No interactions yet";
	private List<string> _interactionHistory = new List<string>();

	public string InteractionEvent
	{
		get => _interactionEvent;
		set
		{
			if (_interactionEvent != value)
			{
				_interactionEvent = value;
				OnPropertyChanged();
			}
		}
	}

	public void AddInteractionEvent(string eventName)
	{
		//string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
		string eventLabel = $"{eventName}";

		_interactionHistory.Add(eventLabel);

		// Keep only the last 10 events to prevent UI from becoming too cluttered
		if (_interactionHistory.Count > 5)
		{
			_interactionHistory.RemoveAt(0);
		}

		// Update the displayed text with all events
		InteractionEvent = string.Join("\n", _interactionHistory);
	}

	public void ClearInteractionHistory()
	{
		_interactionHistory.Clear();
		InteractionEvent = "No interactions yet";
	}

	private double _heightRequest = 100;
	public double HeightRequest
	{
		get => _heightRequest;
		set
		{
			if (_heightRequest != value)
			{
				_heightRequest = value;
				OnPropertyChanged();
			}
		}
	}

	private double _widthRequest = 100;
	public double WidthRequest
	{
		get => _widthRequest;
		set
		{
			if (_widthRequest != value)
			{
				_widthRequest = value;
				OnPropertyChanged();

			}
		}
	}

	private Shadow _shadow;
	public Shadow Shadow
	{
		get => _shadow;
		set
		{
			if (_shadow != value)
			{
				_shadow = value;
				OnPropertyChanged();
			}
		}
	}

	public IDrawable Drawable { get; private set; }

	private readonly Dictionary<DrawableType, (double X, double Y, double Width, double Height)> _drawableDimensions = new();

	public GraphicsViewViewModel()
	{
		UpdateDrawable();
	}

	private void UpdateDrawable()
	{
		Drawable = GetDrawable(SelectedDrawable);
		OnPropertyChanged(nameof(Drawable));

		// Ensure RectF height and width match HeightRequest and WidthRequest
		if (Drawable is IDrawable drawable)
		{
			RectF rect = new RectF(0, 0, (float)WidthRequest, (float)HeightRequest);
			Debug.WriteLine($"RectF Height: {rect.Height}, Width: {rect.Width}");

			// Notify changes for DrawableDimensions
			OnPropertyChanged(nameof(DrawableDimensions));
		}

		// Force GraphicsView to redraw
		RequestInvalidate?.Invoke();
	}

	private IDrawable GetDrawable(DrawableType drawableType)
	{
		return drawableType switch
		{
			DrawableType.Square => new SquareDrawable(this),
			DrawableType.Triangle => new TriangleDrawable(this),
			DrawableType.Ellipse => new EllipseDrawable(this),
			DrawableType.Line => new LineDrawable(this),
			DrawableType.String => new StringDrawable(this),
			DrawableType.Image => new ImageDrawable(this),
			DrawableType.TransparentEllipse => new TransparentEllipseDrawable(this),
			_ => null,
		};
	}

	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public double SquareDrawableHeight => Drawable is SquareDrawable squareDrawable ? squareDrawable.RectFHeight : 0;
	public double SquareDrawableWidth => Drawable is SquareDrawable squareDrawable ? squareDrawable.RectFWidth : 0;
	public double TriangleDrawableHeight => Drawable is TriangleDrawable triangleDrawable ? triangleDrawable.RectFHeight : 0;
	public double TriangleDrawableWidth => Drawable is TriangleDrawable triangleDrawable ? triangleDrawable.RectFWidth : 0;

	public double DrawableHeight
	{
		get
		{
			return Drawable switch
			{
				SquareDrawable squareDrawable => squareDrawable.RectFHeight,
				TriangleDrawable triangleDrawable => triangleDrawable.RectFHeight,
				_ => 0
			};
		}
	}

	public double DrawableWidth
	{
		get
		{
			return Drawable switch
			{
				SquareDrawable squareDrawable => squareDrawable.RectFWidth,
				TriangleDrawable triangleDrawable => triangleDrawable.RectFWidth,
				_ => 0
			};
		}
	}

	public string DrawableDimensions
	{
		get
		{
			var dimensions = GetDrawableDimensions(SelectedDrawable);
			return dimensions.HasValue
				? $"X: {dimensions.Value.X:F1}, Y: {dimensions.Value.Y:F1}, Width: {dimensions.Value.Width:F1}, Height: {dimensions.Value.Height:F1}"
				: "X: 0, Y: 0, Width: 0, Height: 0";
		}
	}

	public void UpdateDrawableDimensions(DrawableType type, double x, double y, double width, double height)
	{
		_drawableDimensions[type] = (x, y, width, height);
		OnPropertyChanged(nameof(DrawableDimensions));
	}

	public void StoreDrawableDimensions(DrawableType drawableType, double x, double y, double width, double height)
	{
		_drawableDimensions[drawableType] = (x, y, width, height);
	}

	public (double X, double Y, double Width, double Height)? GetDrawableDimensions(DrawableType drawableType)
	{
		return _drawableDimensions.TryGetValue(drawableType, out var dimensions) ? dimensions : null;
	}

	public void UpdateDrawable(IDrawable drawable)
	{
		Drawable = drawable;
	}

	public void UpdateDrawableDimensions(double width, double height)
	{
		StoreDrawableDimensions(SelectedDrawable, 0, 0, width, height);
		OnPropertyChanged(nameof(DrawableDimensions));
	}

	public void UpdateDrawableDimensions(double x, double y, double width, double height)
	{
		StoreDrawableDimensions(SelectedDrawable, x, y, width, height);
		OnPropertyChanged(nameof(DrawableDimensions));
	}
}

public class SquareDrawable : IDrawable
{
	private readonly GraphicsViewViewModel _viewModel;

	public SquareDrawable(GraphicsViewViewModel viewModel)
	{
		_viewModel = viewModel;
	}

	public double RectFHeight { get; private set; }
	public double RectFWidth { get; private set; }

	public void Draw(ICanvas canvas, RectF dirtyRect)
	{
		canvas.SaveState();

		RectFHeight = dirtyRect.Height;
		RectFWidth = dirtyRect.Width;
		Debug.WriteLine(dirtyRect);
		// Notify ViewModel about updated dimensions including X and Y
		_viewModel.UpdateDrawableDimensions(dirtyRect.X, dirtyRect.Y, RectFWidth, RectFHeight);

		// Check if GraphicsView has valid dimensions (like your simple sample)
		if (dirtyRect.Width <= 0 || dirtyRect.Height <= 0)
		{
			canvas.RestoreState();
			return;
		}

		float centerX = dirtyRect.Width / 2;
		float centerY = dirtyRect.Height / 2;

		// Use the actual rectangle dimensions with scaling, not just the minimum
		float width = dirtyRect.Width * 0.8f;  // Use 80% of width
		float height = dirtyRect.Height * 0.8f; // Use 80% of height

		// Draw a rectangle that respects the actual GraphicsView proportions
		float rectX = centerX - width / 2;
		float rectY = centerY - height / 2;

		// SetShadow - used once across all drawables
		canvas.SetShadow(new SizeF(3, 3), 5, Colors.Gray);

		// FillRoundedRectangle - using actual dimensions
		canvas.FillColor = _viewModel.CurrentDrawColor;
		float cornerRadius = 5;
		canvas.FillRoundedRectangle(rectX, rectY, width, height, cornerRadius);

		// DrawRoundedRectangle - outline the rounded rectangle
		canvas.StrokeColor = Colors.Black;
		canvas.StrokeSize = 2;
		canvas.DrawRoundedRectangle(rectX, rectY, width, height, cornerRadius);

		canvas.RestoreState();
	}
}

public class TriangleDrawable : IDrawable
{
	private readonly GraphicsViewViewModel _viewModel;

	public TriangleDrawable(GraphicsViewViewModel viewModel)
	{
		_viewModel = viewModel;
	}

	public double RectFHeight { get; private set; }
	public double RectFWidth { get; private set; }

	public void Draw(ICanvas canvas, RectF dirtyRect)
	{
		canvas.SaveState();

		RectFHeight = dirtyRect.Height;
		RectFWidth = dirtyRect.Width;

		// Notify ViewModel about updated dimensions including X and Y
		_viewModel.UpdateDrawableDimensions(dirtyRect.X, dirtyRect.Y, RectFWidth, RectFHeight);

		// Check if GraphicsView has valid dimensions
		if (dirtyRect.Width <= 0 || dirtyRect.Height <= 0)
		{
			canvas.RestoreState();
			return;
		}

		float centerX = dirtyRect.Width / 2;
		float centerY = dirtyRect.Height / 2;

		// Use the actual rectangle dimensions with scaling
		float width = dirtyRect.Width * 0.8f;  // Use 80% of width
		float height = dirtyRect.Height * 0.8f; // Use 80% of height

		// Calculate triangle bounds that respect the actual GraphicsView dimensions
		float triangleX = centerX - width / 2;
		float triangleY = centerY - height / 2;

		float strokeWidth = 3;

		RectF triangleRect = new RectF(triangleX, triangleY, width, height);

		// DrawPath/FillPath - triangle that fills the available rectangle
		PathF path = new PathF();
		path.MoveTo(triangleRect.Left, triangleRect.Bottom);
		path.LineTo(triangleRect.Right, triangleRect.Bottom);
		path.LineTo(triangleRect.Center.X, triangleRect.Top);
		path.Close();

		canvas.FillColor = _viewModel.CurrentDrawColor;
		canvas.FillPath(path);

		// StrokeLineJoin - used once across all drawables
		canvas.StrokeColor = Colors.Black;
		canvas.StrokeSize = strokeWidth;
		canvas.StrokeLineJoin = LineJoin.Round;
		canvas.DrawPath(path);

		canvas.RestoreState();
	}
}

public class EllipseDrawable : IDrawable
{
	private readonly GraphicsViewViewModel _viewModel;

	public EllipseDrawable(GraphicsViewViewModel viewModel)
	{
		_viewModel = viewModel;
	}

	public void Draw(ICanvas canvas, RectF dirtyRect)
	{
		canvas.SaveState();

		// Notify ViewModel about updated dimensions including X and Y
		_viewModel.UpdateDrawableDimensions(dirtyRect.X, dirtyRect.Y, dirtyRect.Width, dirtyRect.Height);

		// Check if GraphicsView has valid dimensions
		if (dirtyRect.Width <= 0 || dirtyRect.Height <= 0)
		{
			canvas.RestoreState();
			return;
		}

		float centerX = dirtyRect.Width / 2;
		float centerY = dirtyRect.Height / 2;

		// Use the actual rectangle dimensions with scaling
		float width = dirtyRect.Width * 0.8f;  // Use 80% of width
		float height = dirtyRect.Height * 0.8f; // Use 80% of height

		float strokeWidth = 2;

		// Create ellipse rect that respects the actual GraphicsView dimensions
		RectF ellipseRect = new RectF(
			centerX - width / 2,
			centerY - height / 2,
			width,
			height
		);

		// Fill the ellipse with the selected color
		canvas.FillColor = _viewModel.CurrentDrawColor;
		canvas.FillEllipse(ellipseRect);

		// DrawEllipse - outline the ellipse with proper bounds
		canvas.StrokeColor = Colors.Black;
		canvas.StrokeSize = strokeWidth;
		canvas.DrawEllipse(ellipseRect);

		// DrawArc and FillArc - scaled to the ellipse size
		float arcWidth = width / 4;
		float arcHeight = height / 4;

		// Ensure arc with stroke fits within bounds
		float arcStrokeWidth = 3;
		float maxArcWidth = arcWidth - (arcStrokeWidth / 2 + 1);
		float maxArcHeight = arcHeight - (arcStrokeWidth / 2 + 1);

		if (maxArcWidth > 0 && maxArcHeight > 0)
		{
			canvas.FillColor = Colors.Yellow;
			canvas.FillArc(centerX - maxArcWidth, centerY - maxArcHeight, maxArcWidth * 2, maxArcHeight * 2, 0, 90, true);

			canvas.StrokeColor = Colors.Orange;
			canvas.StrokeSize = arcStrokeWidth;
			canvas.DrawArc(centerX - maxArcWidth, centerY - maxArcHeight, maxArcWidth * 2, maxArcHeight * 2, 180, 90, true, false);
		}

		canvas.RestoreState();
	}
}

public class LineDrawable : IDrawable
{
	private readonly GraphicsViewViewModel _viewModel;

	public LineDrawable(GraphicsViewViewModel viewModel)
	{
		_viewModel = viewModel;
	}

	public void Draw(ICanvas canvas, RectF dirtyRect)
	{
		canvas.SaveState();

		// Notify ViewModel about updated dimensions including X and Y
		_viewModel.UpdateDrawableDimensions(dirtyRect.X, dirtyRect.Y, dirtyRect.Width, dirtyRect.Height);

		// Check if GraphicsView has valid dimensions
		if (dirtyRect.Width <= 0 || dirtyRect.Height <= 0)
		{
			canvas.RestoreState();
			return;
		}

		float centerX = dirtyRect.Width / 2;
		float centerY = dirtyRect.Height / 2;
		float size = Math.Min(dirtyRect.Width, dirtyRect.Height) * 0.7f;

		float strokeWidth = 4;
		float padding = strokeWidth / 2 + 1;

		// Calculate line bounds centered in the canvas
		float lineSize = size;
		float lineX1 = centerX - lineSize / 2;
		float lineY1 = centerY - lineSize / 2;
		float lineX2 = centerX + lineSize / 2;
		float lineY2 = centerY + lineSize / 2;

		// StrokeLineCap and StrokeDashPattern - used once across all drawables
		canvas.StrokeColor = _viewModel.CurrentDrawColor;
		canvas.StrokeSize = strokeWidth;
		canvas.StrokeLineCap = LineCap.Round;
		canvas.StrokeDashPattern = new float[] { 15, 5, 10, 5 };

		// Draw diagonal line centered in the canvas
		canvas.DrawLine(lineX1, lineY1, lineX2, lineY2);

		canvas.RestoreState();
	}
}

public class StringDrawable : IDrawable
{
	private readonly GraphicsViewViewModel _viewModel;

	public StringDrawable(GraphicsViewViewModel viewModel)
	{
		_viewModel = viewModel;
	}

	public void Draw(ICanvas canvas, RectF dirtyRect)
	{
		canvas.SaveState();

		// Notify ViewModel about updated dimensions including X and Y
		_viewModel.UpdateDrawableDimensions(dirtyRect.X, dirtyRect.Y, dirtyRect.Width, dirtyRect.Height);

		// Check if GraphicsView has valid dimensions
		if (dirtyRect.Width <= 0 || dirtyRect.Height <= 0)
		{
			canvas.RestoreState();
			return;
		}

		// Calculate center and size for the string drawable (like your simple sample)
		float centerX = dirtyRect.Width / 2;
		float centerY = dirtyRect.Height / 2;
		float size = Math.Min(dirtyRect.Width, dirtyRect.Height) * 0.7f;

		// ClipPath - used once across all drawables
		PathF clipPath = new PathF();
		float clipSize = size;
		float clipX = centerX - clipSize / 2;
		float clipY = centerY - clipSize / 2;

		clipPath.MoveTo(clipX + 10, clipY);
		clipPath.LineTo(clipX + clipSize - 10, clipY);
		clipPath.LineTo(clipX + clipSize, clipY + 10);
		clipPath.LineTo(clipX + clipSize, clipY + clipSize - 10);
		clipPath.LineTo(clipX + clipSize - 10, clipY + clipSize);
		clipPath.LineTo(clipX + 10, clipY + clipSize);
		clipPath.LineTo(clipX, clipY + clipSize - 10);
		clipPath.LineTo(clipX, clipY + 10);
		clipPath.Close();
		canvas.ClipPath(clipPath);

		float maxRadius = size / 3;

		float strokeWidth = 2;
		float circleRadius = maxRadius - (strokeWidth / 2 + 1);

		if (circleRadius > 0)
		{
			canvas.FillColor = Colors.LightYellow;
			canvas.FillCircle(centerX, centerY, circleRadius);

			canvas.StrokeColor = Colors.Blue;
			canvas.StrokeSize = strokeWidth;
			canvas.DrawCircle(centerX, centerY, circleRadius);

			canvas.FontColor = Colors.Blue;
			canvas.FontSize = 14;
			canvas.Font = Microsoft.Maui.Graphics.Font.Default;

			string displayText = "Hello";

			// Position text at the center of the circle, not relative to the clip area
			float textWidth = circleRadius * 2; // Use full circle diameter for text width
			float textHeight = 20; // Reduce height to fit better
			float textX = centerX - textWidth / 2;
			float textY = centerY - textHeight / 2;

			// Ensure text area is valid and draw it centered in the circle
			if (textWidth > 0 && textHeight > 0)
			{
				canvas.DrawString(displayText, textX, textY, textWidth, textHeight, HorizontalAlignment.Center, VerticalAlignment.Center);
			}
		}

		canvas.RestoreState();
	}
}

public class ImageDrawable : IDrawable
{
	private readonly GraphicsViewViewModel _viewModel;
	private Microsoft.Maui.Graphics.IImage _image;

	public ImageDrawable(GraphicsViewViewModel viewModel)
	{
		_viewModel = viewModel;
		LoadImage();
	}

	private void LoadImage()
	{
		try
		{
			var assembly = GetType().GetTypeInfo().Assembly;

			// Try different possible resource names
			string[] possibleNames = {
					"Maui.Controls.Sample.Resources.Images.royals.png",
					"Controls.TestCases.HostApp.Resources.Images.royals.png",
					"royals.png",
					"Resources.Images.royals.png"
				};

			foreach (var resourceName in possibleNames)
			{
				using (var stream = assembly.GetManifestResourceStream(resourceName))
				{
					if (stream != null)
					{
						_image = Microsoft.Maui.Graphics.Platform.PlatformImage.FromStream(stream);
						Debug.WriteLine($"Successfully loaded image with resource name: {resourceName}");
						return;
					}
				}
			}

			// If we get here, none of the resource names worked
			Debug.WriteLine("Could not find embedded image resource. Available resources:");
			foreach (var name in assembly.GetManifestResourceNames())
			{
				Debug.WriteLine($" - {name}");
			}
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"Error loading image: {ex.Message}");
			_image = null;
		}
	}

	public void Draw(ICanvas canvas, RectF dirtyRect)
	{
		canvas.SaveState();

		// Notify ViewModel about updated dimensions including X and Y
		_viewModel.UpdateDrawableDimensions(dirtyRect.X, dirtyRect.Y, dirtyRect.Width, dirtyRect.Height);

		// Check if GraphicsView has valid dimensions
		if (dirtyRect.Width <= 0 || dirtyRect.Height <= 0)
		{
			canvas.RestoreState();
			return;
		}

		// Calculate center and size for the image (like your simple sample)
		float centerX = dirtyRect.Width / 2;
		float centerY = dirtyRect.Height / 2;
		float size = Math.Min(dirtyRect.Width, dirtyRect.Height) * 0.7f;

		// Draw a simple background frame
		float frameWidth = 10;
		float frameSize = size;
		float frameX = centerX - frameSize / 2;
		float frameY = centerY - frameSize / 2;

		// Draw frame background
		canvas.FillColor = Colors.Gold;
		canvas.FillRectangle(frameX, frameY, frameSize, frameSize);

		// Draw inner area for the image
		float innerX = frameX + frameWidth;
		float innerY = frameY + frameWidth;
		float innerSize = frameSize - (2 * frameWidth);

		canvas.FillColor = Colors.LightGray;
		canvas.FillRectangle(innerX, innerY, innerSize, innerSize);

		if (_image != null)
		{
			// Calculate image position and size to fit within the inner area
			float padding = 5;
			float availableWidth = innerSize - (2 * padding);
			float availableHeight = innerSize - (2 * padding);

			if (availableWidth > 0 && availableHeight > 0)
			{
				// Calculate scale to fit image while maintaining aspect ratio
				float scaleX = availableWidth / _image.Width;
				float scaleY = availableHeight / _image.Height;
				float scale = Math.Min(scaleX, scaleY);

				float imageWidth = _image.Width * scale;
				float imageHeight = _image.Height * scale;

				// Center the image within the inner area
				float imageX = innerX + (innerSize - imageWidth) / 2;
				float imageY = innerY + (innerSize - imageHeight) / 2;

				// Draw the actual image
				canvas.DrawImage(_image, imageX, imageY, imageWidth, imageHeight);

				Debug.WriteLine($"Drew image at ({imageX}, {imageY}) with size ({imageWidth}, {imageHeight})");
			}
		}
		else
		{
			// Fallback: draw text if image couldn't be loaded
			canvas.FontColor = Colors.Red;
			canvas.FontSize = 12;
			canvas.Font = Microsoft.Maui.Graphics.Font.Default;

			string errorText = "Image not found";
			float textX = innerX + 5;
			float textY = innerY + 20;
			float textWidth = innerSize - 10;
			float textHeight = 20;

			if (textWidth > 0 && textHeight > 0)
			{
				canvas.DrawString(errorText, textX, textY, textWidth, textHeight, HorizontalAlignment.Center, VerticalAlignment.Top);
			}

			Debug.WriteLine("Image could not be loaded, showing error message");
		}

		canvas.RestoreState();
	}
}

public class TransparentEllipseDrawable : IDrawable
{
	private readonly GraphicsViewViewModel _viewModel;

	public TransparentEllipseDrawable(GraphicsViewViewModel viewModel)
	{
		_viewModel = viewModel;
	}

	public void Draw(ICanvas canvas, RectF dirtyRect)
	{
		canvas.SaveState();

		// Notify ViewModel about updated dimensions including X and Y
		_viewModel.UpdateDrawableDimensions(dirtyRect.X, dirtyRect.Y, dirtyRect.Width, dirtyRect.Height);

		// Check if GraphicsView has valid dimensions
		if (dirtyRect.Width <= 0 || dirtyRect.Height <= 0)
		{
			canvas.RestoreState();
			return;
		}

		// Calculate center and size for the ellipse (following the scaling pattern)
		float centerX = dirtyRect.Width / 2;
		float centerY = dirtyRect.Height / 2;
		float size = Math.Min(dirtyRect.Width, dirtyRect.Height) * 0.7f;

		float ellipseSize = size * 0.8f; // Make it slightly smaller for better visibility
		float ellipseX = centerX - ellipseSize / 2;
		float ellipseY = centerY - ellipseSize / 2;
		var outerRect = new RectF(ellipseX, ellipseY, ellipseSize, ellipseSize);

		// Set transparent fill color - this demonstrates the issue on Android
		Color transparentColor = Colors.Transparent;

		canvas.SetFillPaint(transparentColor.AsPaint(), outerRect);

		canvas.SetShadow(new SizeF(0, 2),
						Microsoft.Maui.Devices.DeviceInfo.Platform == Microsoft.Maui.Devices.DevicePlatform.Android ? 4 : 3,
						Color.FromArgb("#59000000"));

		// Draw the transparent ellipse with shadow
		canvas.FillEllipse(outerRect.X, outerRect.Y, outerRect.Width, outerRect.Height);

		float arcSize = ellipseSize * 0.5f;
		float arcX = outerRect.X + (outerRect.Width - arcSize) / 2;
		float arcY = outerRect.Y + (outerRect.Height - arcSize) / 2;

		canvas.FillColor = Colors.Blue;
		canvas.DrawArc(arcX, arcY, arcSize, arcSize, 45, 90, true, false);

		canvas.RestoreState();
	}
}


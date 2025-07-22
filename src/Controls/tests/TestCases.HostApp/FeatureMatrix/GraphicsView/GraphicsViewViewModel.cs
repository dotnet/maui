using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Reflection;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
    public enum DrawableType
    {
        Square,
        Triangle,
        Ellipse,
        Line,
        String,
        Image,
    }

    public class GraphicsViewViewModel : INotifyPropertyChanged
    {
        public Action RequestInvalidate { get; set; }
        public string DrawableTypeLabel => SelectedDrawable.ToString();
        private DrawableType _selectedDrawable = DrawableType.Square;
        private FlowDirection _flowDirection = FlowDirection.LeftToRight;
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

        public void ChangeToRandomColor()
        {
            var colors = new Color[] 
            { 
                Colors.Blue, Colors.Red, Colors.Green, Colors.Purple, 
                Colors.Orange, Colors.DarkBlue, Colors.DarkRed, Colors.DarkGreen,
                Colors.Magenta, Colors.Cyan, Colors.Yellow, Colors.Pink
            };
            
            Color newColor;
            do
            {
                newColor = colors[_random.Next(colors.Length)];
            } while (newColor == CurrentDrawColor); // Ensure we get a different color
            
            CurrentDrawColor = newColor;
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

        public FlowDirection FlowDirection
        {
            get => _flowDirection;
            set
            {
                if (_flowDirection != value)
                {
                    _flowDirection = value;
                    OnPropertyChanged();
                    //RequestInvalidate?.Invoke();
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
                    //RequestInvalidate?.Invoke();
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
                    // RequestInvalidate?.Invoke();
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
            RectFHeight = dirtyRect.Height;
            RectFWidth = dirtyRect.Width;
            Debug.WriteLine(dirtyRect);
            // Notify ViewModel about updated dimensions including X and Y
            _viewModel.UpdateDrawableDimensions(dirtyRect.X, dirtyRect.Y, RectFWidth, RectFHeight);

            // SetShadow - used once across all drawables
            canvas.SetShadow(new SizeF(3, 3), 5, Colors.Gray);

            // FillRoundedRectangle - used once across all drawables
            canvas.FillColor = _viewModel.CurrentDrawColor;
            float cornerRadius = 15;
            canvas.FillRoundedRectangle(dirtyRect.X + 10, dirtyRect.Y + 10, 
                dirtyRect.Width - 20, dirtyRect.Height - 20, cornerRadius);

            // DrawRoundedRectangle - outline the rounded rectangle
            canvas.StrokeColor = Colors.Black;
            canvas.StrokeSize = 2;
            canvas.DrawRoundedRectangle(dirtyRect.X + 10, dirtyRect.Y + 10, 
                dirtyRect.Width - 20, dirtyRect.Height - 20, cornerRadius);

            // Reset shadow
            canvas.SetShadow(SizeF.Zero, 0, Colors.Transparent);

            // Add "HelloWorld" label with flow direction support
            DrawFlowDirectionLabel(canvas, dirtyRect);
        }

        private void DrawFlowDirectionLabel(ICanvas canvas, RectF rect)
        {
            canvas.FontColor = Colors.White;
            canvas.FontSize = 14;
            canvas.Font = Microsoft.Maui.Graphics.Font.Default;
            
            string text = "HelloWorld";
            
            // Define a proper bounding box for the text following documentation pattern
            float textX = rect.Left + 10;
            float textY = rect.Center.Y - 15; // Adjust Y to center vertically
            float textWidth = Math.Max(100, rect.Width - 20); // Ensure minimum width
            float textHeight = 30;
            
            canvas.DrawString(text, textX, textY, textWidth, textHeight, HorizontalAlignment.Left, VerticalAlignment.Top);
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
            RectFHeight = dirtyRect.Height;
            RectFWidth = dirtyRect.Width;

            // Notify ViewModel about updated dimensions including X and Y
            _viewModel.UpdateDrawableDimensions(dirtyRect.X, dirtyRect.Y, RectFWidth, RectFHeight);

            // DrawPath/FillPath - used once across all drawables
            PathF path = new PathF();
            path.MoveTo(dirtyRect.Left, dirtyRect.Bottom);
            path.LineTo(dirtyRect.Right, dirtyRect.Bottom);
            path.LineTo(dirtyRect.Center.X, dirtyRect.Top);
            path.Close();
            
            canvas.FillColor = _viewModel.CurrentDrawColor;
            canvas.FillPath(path);

            // StrokeLineJoin - used once across all drawables
            canvas.StrokeColor = Colors.Black;
            canvas.StrokeSize = 3;
            canvas.StrokeLineJoin = LineJoin.Round;
            canvas.DrawPath(path);

            // Add "HelloWorld" label with flow direction support
            DrawFlowDirectionLabel(canvas, dirtyRect);
        }

        private void DrawFlowDirectionLabel(ICanvas canvas, RectF rect)
        {
            canvas.FontColor = Colors.White;
            canvas.FontSize = 14;
            canvas.Font = Microsoft.Maui.Graphics.Font.Default;
            
            string text = "HelloWorld";
            
            // Define a proper bounding box for the text following documentation pattern
            float textX = rect.Left + 10;
            float textY = rect.Center.Y - 15; // Adjust Y to center vertically
            float textWidth = Math.Max(100, rect.Width - 20); // Ensure minimum width
            float textHeight = 30;
            
            canvas.DrawString(text, textX, textY, textWidth, textHeight, HorizontalAlignment.Left, VerticalAlignment.Top);
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
            // Notify ViewModel about updated dimensions including X and Y
            _viewModel.UpdateDrawableDimensions(dirtyRect.X, dirtyRect.Y, dirtyRect.Width, dirtyRect.Height);

            // Fill the ellipse with the selected color
            canvas.FillColor = _viewModel.CurrentDrawColor;
            canvas.FillEllipse(dirtyRect);

            // DrawEllipse - outline the ellipse
            canvas.StrokeColor = Colors.Black;
            canvas.StrokeSize = 2;
            canvas.DrawEllipse(dirtyRect);

            // DrawArc and FillArc - used once across all drawables
            float centerX = dirtyRect.Center.X;
            float centerY = dirtyRect.Center.Y;
            float arcRadius = Math.Min(dirtyRect.Width, dirtyRect.Height) / 4;
            
            canvas.FillColor = Colors.Yellow;
            canvas.FillArc(centerX - arcRadius, centerY - arcRadius, arcRadius * 2, arcRadius * 2, 0, 90, true);
            
            canvas.StrokeColor = Colors.Orange;
            canvas.StrokeSize = 3;
            canvas.DrawArc(centerX - arcRadius, centerY - arcRadius, arcRadius * 2, arcRadius * 2, 180, 90, true, false);
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
            // Notify ViewModel about updated dimensions including X and Y
            _viewModel.UpdateDrawableDimensions(dirtyRect.X, dirtyRect.Y, dirtyRect.Width, dirtyRect.Height);

            // StrokeLineCap and StrokeDashPattern - used once across all drawables
            canvas.StrokeColor = _viewModel.CurrentDrawColor;
            canvas.StrokeSize = 4;
            canvas.StrokeLineCap = LineCap.Round;
            canvas.StrokeDashPattern = new float[] { 15, 5, 10, 5 };
            canvas.DrawLine(dirtyRect.Left, dirtyRect.Top, dirtyRect.Right, dirtyRect.Bottom);

            // Reset dash pattern
            canvas.StrokeDashPattern = null;
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
            // Notify ViewModel about updated dimensions including X and Y
            _viewModel.UpdateDrawableDimensions(dirtyRect.X, dirtyRect.Y, dirtyRect.Width, dirtyRect.Height);

            // ClipPath - used once across all drawables
            canvas.SaveState();
            PathF clipPath = new PathF();
            clipPath.MoveTo(dirtyRect.X + 10, dirtyRect.Y);
            clipPath.LineTo(dirtyRect.Right - 10, dirtyRect.Y);
            clipPath.LineTo(dirtyRect.Right, dirtyRect.Y + 10);
            clipPath.LineTo(dirtyRect.Right, dirtyRect.Bottom - 10);
            clipPath.LineTo(dirtyRect.Right - 10, dirtyRect.Bottom);
            clipPath.LineTo(dirtyRect.X + 10, dirtyRect.Bottom);
            clipPath.LineTo(dirtyRect.X, dirtyRect.Bottom - 10);
            clipPath.LineTo(dirtyRect.X, dirtyRect.Y + 10);
            clipPath.Close();
            canvas.ClipPath(clipPath);

            // DrawCircle - use circle background instead of rectangle
            float centerX = dirtyRect.Center.X;
            float centerY = dirtyRect.Center.Y;
            float circleRadius = Math.Min(dirtyRect.Width, dirtyRect.Height) / 3;
            
            canvas.FillColor = Colors.LightYellow;
            canvas.FillCircle(centerX, centerY, circleRadius);
            
            canvas.StrokeColor = Colors.Blue;
            canvas.StrokeSize = 2;
            canvas.DrawCircle(centerX, centerY, circleRadius);

            // Set font properties following the documentation pattern
            canvas.FontColor = Colors.Blue;
            canvas.FontSize = 18;
            canvas.Font = Microsoft.Maui.Graphics.Font.Default;
            
            // Define the text and bounding box
            string displayText = "Hello";
            
            // Use a proper bounding box within the dirtyRect, leaving some padding
            float padding = 20;
            float textX = dirtyRect.X + padding;
            float textY = dirtyRect.Y + padding;
            float textWidth = dirtyRect.Width - (2 * padding);
            float textHeight = dirtyRect.Height - (2 * padding);

            // Center aligned text
            canvas.DrawString(displayText, textX, textY + 40, textWidth, 30, HorizontalAlignment.Center, VerticalAlignment.Top);
            
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
            // Notify ViewModel about updated dimensions including X and Y
            _viewModel.UpdateDrawableDimensions(dirtyRect.X, dirtyRect.Y, dirtyRect.Width, dirtyRect.Height);

            // SubtractFromClip - used once across all drawables to create frame effect
            canvas.SaveState();
            
            // Draw outer rectangle
            canvas.FillColor = Colors.Gold;
            canvas.FillRectangle(dirtyRect);
            
            // Subtract inner rectangle to create frame
            float frameWidth = 15;
            canvas.SubtractFromClip(dirtyRect.X + frameWidth, dirtyRect.Y + frameWidth, 
                dirtyRect.Width - (2 * frameWidth), dirtyRect.Height - (2 * frameWidth));
            
            canvas.RestoreState();

            // Draw inner content area
            canvas.FillColor = Colors.LightGray;
            canvas.FillRectangle(dirtyRect.X + frameWidth, dirtyRect.Y + frameWidth, 
                dirtyRect.Width - (2 * frameWidth), dirtyRect.Height - (2 * frameWidth));

            if (_image != null)
            {
                // Calculate image position and size to fit within the inner area
                float padding = 20 + frameWidth;
                float availableWidth = dirtyRect.Width - (2 * padding);
                float availableHeight = dirtyRect.Height - (2 * padding);

                // Calculate scale to fit image while maintaining aspect ratio
                float scaleX = availableWidth / _image.Width;
                float scaleY = availableHeight / _image.Height;
                float scale = Math.Min(scaleX, scaleY);

                float imageWidth = _image.Width * scale;
                float imageHeight = _image.Height * scale;

                // Center the image
                float imageX = dirtyRect.X + (dirtyRect.Width - imageWidth) / 2;
                float imageY = dirtyRect.Y + (dirtyRect.Height - imageHeight) / 2;

                // Draw the actual PNG image
                canvas.DrawImage(_image, imageX, imageY, imageWidth, imageHeight);

                Debug.WriteLine($"Drew image at ({imageX}, {imageY}) with size ({imageWidth}, {imageHeight})");
            }
            else
            {
                // Fallback: draw text if image couldn't be loaded
                canvas.FontColor = Colors.Red;
                canvas.FontSize = 14;
                canvas.Font = Microsoft.Maui.Graphics.Font.Default;
                
                string errorText = "Image not found";
                float textX = dirtyRect.X + 10 + frameWidth;
                float textY = dirtyRect.Y + 30 + frameWidth;
                float textWidth = dirtyRect.Width - 20 - (2 * frameWidth);
                float textHeight = 30;
                
                canvas.DrawString(errorText, textX, textY, textWidth, textHeight, HorizontalAlignment.Center, VerticalAlignment.Top);
                
                Debug.WriteLine("Image could not be loaded, showing error message");
            }
        }
    }
}

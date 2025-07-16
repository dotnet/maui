using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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

            // Use the color from ViewModel to show when redraw happens
            canvas.FillColor = _viewModel.CurrentDrawColor;
            canvas.FillRectangle(dirtyRect);

            // Add "HelloWorld" label with flow direction support
            DrawFlowDirectionLabel(canvas, dirtyRect);
        }

        private void DrawFlowDirectionLabel(ICanvas canvas, RectF rect)
        {
            canvas.FontColor = Colors.White;
            var fontSize = Math.Min(rect.Width, rect.Height) * 0.15f;
            canvas.FontSize = fontSize;
            
            string text = "HelloWorld";
            
            // Use the shape bounds and let FlowDirection handle RTL/LTR automatically
            // HorizontalAlignment.Left respects FlowDirection (becomes right-aligned in RTL)
            float x = rect.Left + 10; // Small padding from edge
            float y = rect.Center.Y;
            
            canvas.DrawString(text, x, y, HorizontalAlignment.Left);
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

            // Use the color from ViewModel to show when redraw happens
            canvas.FillColor = _viewModel.CurrentDrawColor;
            PathF path = new PathF();
            path.MoveTo(dirtyRect.Left, dirtyRect.Bottom);
            path.LineTo(dirtyRect.Right, dirtyRect.Bottom);
            path.LineTo(dirtyRect.Center.X, dirtyRect.Top);
            path.Close();
            canvas.FillPath(path);

            // Add "HelloWorld" label with flow direction support
            DrawFlowDirectionLabel(canvas, dirtyRect);
        }

        private void DrawFlowDirectionLabel(ICanvas canvas, RectF rect)
        {
            canvas.FontColor = Colors.White;
            var fontSize = Math.Min(rect.Width, rect.Height) * 0.12f;
            canvas.FontSize = fontSize;
            
            string text = "HelloWorld";
            
            // Use the shape bounds and let FlowDirection handle RTL/LTR automatically
            // HorizontalAlignment.Left respects FlowDirection (becomes right-aligned in RTL)
            float x = rect.Left + 10; // Small padding from edge
            float y = rect.Center.Y;
            
            canvas.DrawString(text, x, y, HorizontalAlignment.Left);
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

            // Use the color from ViewModel to show when redraw happens
            canvas.FillColor = _viewModel.CurrentDrawColor;
            canvas.FillEllipse(dirtyRect);

            // Add "HelloWorld" label with flow direction support
            DrawFlowDirectionLabel(canvas, dirtyRect);
        }

        private void DrawFlowDirectionLabel(ICanvas canvas, RectF rect)
        {
            canvas.FontColor = Colors.White;
            var fontSize = Math.Min(rect.Width, rect.Height) * 0.15f;
            canvas.FontSize = fontSize;
            
            string text = "HelloWorld";
            
            // Use the shape bounds and let FlowDirection handle RTL/LTR automatically
            // HorizontalAlignment.Left respects FlowDirection (becomes right-aligned in RTL)
            float x = rect.Left + 10; // Small padding from edge
            float y = rect.Center.Y;
            
            canvas.DrawString(text, x, y, HorizontalAlignment.Left);
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

            // Use the color from ViewModel to show when redraw happens
            canvas.StrokeColor = _viewModel.CurrentDrawColor;
            canvas.StrokeSize = 2;
            canvas.DrawLine(dirtyRect.Left, dirtyRect.Top, dirtyRect.Right, dirtyRect.Bottom);
            Debug.WriteLine(dirtyRect);

            // Add "HelloWorld" label with flow direction support
            DrawFlowDirectionLabel(canvas, dirtyRect);
        }

        private void DrawFlowDirectionLabel(ICanvas canvas, RectF rect)
        {
            canvas.FontColor = Colors.Black;
            var fontSize = Math.Min(rect.Width, rect.Height) * 0.15f;
            canvas.FontSize = fontSize;
            
            string text = "HelloWorld";
            
            // Use the shape bounds and let FlowDirection handle RTL/LTR automatically
            // HorizontalAlignment.Left respects FlowDirection (becomes right-aligned in RTL)
            float x = rect.Left + 5; // Small padding from edge
            float y = rect.Top + fontSize + 5; // Position above the line
            
            canvas.DrawString(text, x, y, HorizontalAlignment.Left);
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

            // Set font properties
            canvas.FontColor = Colors.DarkBlue;
            var fontSize = Math.Min(dirtyRect.Width, dirtyRect.Height) * 0.2f;
            canvas.FontSize = fontSize;
            Debug.WriteLine(dirtyRect);
            // Simple text to display
            string displayText = "Sample Text";

            // Calculate text positioning (center the text)
            var textSize = canvas.GetStringSize(displayText, Microsoft.Maui.Graphics.Font.Default, fontSize);
            float x = dirtyRect.Center.X - (textSize.Width / 2);
            float y = dirtyRect.Center.Y + (textSize.Height / 4);

            // Draw background rectangle for better visibility
            var backgroundRect = new RectF(x - 5, y - textSize.Height + 5, textSize.Width + 10, textSize.Height + 5);
            canvas.FillColor = Colors.LightYellow;
            canvas.FillRectangle(backgroundRect);

            // Draw border around text
            canvas.StrokeColor = Colors.DarkBlue;
            canvas.StrokeSize = 1;
            canvas.DrawRectangle(backgroundRect);

            // Draw the text
            canvas.FontColor = Colors.DarkBlue;
            canvas.DrawString(displayText, x, y, HorizontalAlignment.Left);

            // Add "HelloWorld" label with flow direction support
            DrawFlowDirectionLabel(canvas, dirtyRect);
        }

        private void DrawFlowDirectionLabel(ICanvas canvas, RectF rect)
        {
            canvas.FontColor = Colors.Green;
            var fontSize = Math.Min(rect.Width, rect.Height) * 0.12f;
            canvas.FontSize = fontSize;
            
            string text = "HelloWorld";
            
            // Use the shape bounds and let FlowDirection handle RTL/LTR automatically
            // HorizontalAlignment.Left respects FlowDirection (becomes right-aligned in RTL)
            float x = rect.Left + 10; // Small padding from edge
            float y = rect.Bottom - 15; // Position at bottom
            
            canvas.DrawString(text, x, y, HorizontalAlignment.Left);
        }
    }

    public class ImageDrawable : IDrawable
    {
        private readonly GraphicsViewViewModel _viewModel;

        public ImageDrawable(GraphicsViewViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            // Notify ViewModel about updated dimensions including X and Y
            _viewModel.UpdateDrawableDimensions(dirtyRect.X, dirtyRect.Y, dirtyRect.Width, dirtyRect.Height);

            // Draw a placeholder image using basic shapes
            // Create a mock "image" using geometric patterns
            DrawMockImage(canvas, dirtyRect);

            // Add "HelloWorld" label with flow direction support
            DrawFlowDirectionLabel(canvas, dirtyRect);
        }

        private void DrawMockImage(ICanvas canvas, RectF rect)
        {
            // Draw image background
            canvas.FillColor = Colors.LightBlue;
            var imageRect = new RectF(
                rect.Left + 10,
                rect.Top + 10,
                rect.Width - 20,
                rect.Height - 20
            ); // Use full rectangle since no label needed
            canvas.FillRectangle(imageRect);

            // Draw image border
            canvas.StrokeColor = Colors.Navy;
            canvas.StrokeSize = 2;
            canvas.DrawRectangle(imageRect);

            // Draw a simple geometric pattern - star icon
            var centerX = imageRect.Center.X;
            var centerY = imageRect.Center.Y;
            var starSize = Math.Min(imageRect.Width, imageRect.Height) * 0.3f;

            // Draw a 5-pointed star
            canvas.FillColor = Colors.Gold;
            PathF starPath = new PathF();

            // Calculate star points
            float outerRadius = starSize;
            float innerRadius = starSize * 0.4f;

            for (int i = 0; i < 10; i++)
            {
                float angle = (float)(i * Math.PI / 5);
                float radius = (i % 2 == 0) ? outerRadius : innerRadius;
                float x = centerX + (float)(Math.Cos(angle - Math.PI / 2) * radius);
                float y = centerY + (float)(Math.Sin(angle - Math.PI / 2) * radius);

                if (i == 0)
                    starPath.MoveTo(x, y);
                else
                    starPath.LineTo(x, y);
            }
            starPath.Close();
            canvas.FillPath(starPath);

            // Add some decorative circles around the star
            canvas.FillColor = Colors.White;
            float circleRadius = starSize * 0.1f;
            canvas.FillEllipse(centerX - starSize * 0.8f, centerY - circleRadius, circleRadius * 2, circleRadius * 2);
            canvas.FillEllipse(centerX + starSize * 0.6f, centerY - circleRadius, circleRadius * 2, circleRadius * 2);
            canvas.FillEllipse(centerX - circleRadius, centerY - starSize * 0.8f, circleRadius * 2, circleRadius * 2);
            canvas.FillEllipse(centerX - circleRadius, centerY + starSize * 0.6f, circleRadius * 2, circleRadius * 2);
            Debug.WriteLine(rect);
        }

        private void DrawFlowDirectionLabel(ICanvas canvas, RectF rect)
        {
            canvas.FontColor = Colors.Navy;
            var fontSize = Math.Min(rect.Width, rect.Height) * 0.12f;
            canvas.FontSize = fontSize;
            
            string text = "HelloWorld";
            
            // Use the shape bounds and let FlowDirection handle RTL/LTR automatically
            // HorizontalAlignment.Left respects FlowDirection (becomes right-aligned in RTL)
            float x = rect.Left + 15; // Small padding from edge
            float y = rect.Bottom - 10; // Position at bottom
            
            canvas.DrawString(text, x, y, HorizontalAlignment.Left);
        }

    }
}

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample;

public enum DrawableType
{
    Rectangle,
    Ellipse,
    Line,
    Triangle,
    Polygon
}

public class GraphicsViewFeatureMatrixViewModel : INotifyPropertyChanged
{
    public event Action RequestInvalidate;
    private bool _isVisible = true;
    private Color _backgroundColor = null;
    private IDrawable _drawable;
    private DrawableType _drawableType = DrawableType.Rectangle;

    public event PropertyChangedEventHandler PropertyChanged;

    public GraphicsViewFeatureMatrixViewModel()
    {
        SetBackgroundColorCommand = new Command<string>(OnSetBackgroundColor);
        SetDrawableTypeCommand = new Command<string>(OnSetDrawableType);
        UpdateDrawable();
    }

    public bool IsVisible
    {
        get => _isVisible;
        set { if (_isVisible != value) { _isVisible = value; OnPropertyChanged(); } }
    }

    public Color BackgroundColor
    {
        get => _backgroundColor;
        set {
            if (_backgroundColor != value) {
                _backgroundColor = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Drawable));
                RequestInvalidate?.Invoke();
            }
        }
    }

    public IDrawable Drawable
    {
        get => _drawable;
        set { if (_drawable != value) { _drawable = value; OnPropertyChanged(); } }
    }

    public DrawableType DrawableType
    {
        get => _drawableType;
        set { if (_drawableType != value) { _drawableType = value; OnPropertyChanged(); UpdateDrawable(); RequestInvalidate?.Invoke(); } }
    }

    public ICommand SetBackgroundColorCommand { get; set; }
    public ICommand SetDrawableTypeCommand { get; set; }

    private void OnSetDrawableType(string type)
    {
        if (Enum.TryParse(type, out DrawableType result))
        {
            DrawableType = result;
        }
    }

    private void OnSetBackgroundColor(string colorName)
    {
        switch (colorName)
        {
            case "Blue":
                BackgroundColor = Colors.Blue;
                break;
            case "Green":
                BackgroundColor = Colors.Green;
                break;
            case "Default":
            default:
                BackgroundColor = null;
                break;
        }
    }

    private void UpdateDrawable()
    {
        switch (DrawableType)
        {
            case DrawableType.Rectangle:
                Drawable = new RectangleDrawable();
                break;
            case DrawableType.Ellipse:
                Drawable = new EllipseDrawable();
                break;
            case DrawableType.Line:
                Drawable = new LineDrawable();
                break;
            case DrawableType.Triangle:
                Drawable = new TriangleDrawable();
                break;
            case DrawableType.Polygon:
                Drawable = new PolygonDrawable();
                break;
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

// Drawable implementations
public class RectangleDrawable : IDrawable
{
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = Colors.Red;
        canvas.FillRectangle(dirtyRect);
        canvas.StrokeColor = Colors.Black;
        canvas.StrokeSize = 4;
        canvas.DrawRectangle(dirtyRect);
    }
}

public class EllipseDrawable : IDrawable
{
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = Colors.Green;
        canvas.FillEllipse(dirtyRect);
        canvas.StrokeColor = Colors.Black;
        canvas.StrokeSize = 4;
        canvas.DrawEllipse(dirtyRect);
    }
}

public class LineDrawable : IDrawable
{
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.StrokeColor = Colors.Blue;
        canvas.StrokeSize = 6;
        canvas.DrawLine(dirtyRect.Left, dirtyRect.Top, dirtyRect.Right, dirtyRect.Bottom);
    }
}

public class TriangleDrawable : IDrawable
{
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        var points = new PointF[]
        {
            new PointF(dirtyRect.Center.X, dirtyRect.Top),
            new PointF(dirtyRect.Right, dirtyRect.Bottom),
            new PointF(dirtyRect.Left, dirtyRect.Bottom)
        };
        var path = new PathF();
        path.MoveTo(points[0]);
        path.LineTo(points[1]);
        path.LineTo(points[2]);
        path.Close();
        canvas.FillColor = Colors.Purple;
        canvas.FillPath(path);
        canvas.StrokeColor = Colors.Black;
        canvas.StrokeSize = 4;
        canvas.DrawPath(path);
    }
}

public class PolygonDrawable : IDrawable
{
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        var points = new PointF[]
        {
            new PointF(dirtyRect.Center.X, dirtyRect.Top),
            new PointF(dirtyRect.Right, dirtyRect.Center.Y),
            new PointF(dirtyRect.Center.X, dirtyRect.Bottom),
            new PointF(dirtyRect.Left, dirtyRect.Center.Y)
        };
        var path = new PathF();
        path.MoveTo(points[0]);
        for (int i = 1; i < points.Length; i++)
            path.LineTo(points[i]);
        path.Close();
        canvas.FillColor = Colors.Orange;
        canvas.FillPath(path);
        canvas.StrokeColor = Colors.Black;
        canvas.StrokeSize = 4;
        canvas.DrawPath(path);
    }
}

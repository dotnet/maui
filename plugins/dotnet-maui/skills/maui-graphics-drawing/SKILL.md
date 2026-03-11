---
name: maui-graphics-drawing
description: >
  Guidance for custom drawing with Microsoft.Maui.Graphics, GraphicsView, canvas drawing operations,
  shapes, paths, text rendering, image drawing, shadows, clipping, and canvas state management.
  USE FOR: "custom drawing", "GraphicsView", "canvas drawing", "draw shapes", "draw path",
  "draw text", "ICanvas", "IDrawable", "shadows", "clipping", "Microsoft.Maui.Graphics".
  DO NOT USE FOR: view animations (use maui-animations), gesture handling on drawn elements
  (use maui-gestures), or app icons (use maui-app-icons-splash).
---

# .NET MAUI Graphics Drawing

Use this skill when implementing custom drawing in .NET MAUI apps using `Microsoft.Maui.Graphics` and `GraphicsView`.

## GraphicsView and IDrawable

`GraphicsView` is the canvas host control. Assign an `IDrawable` implementation to its `Drawable` property.

```xml
<GraphicsView Drawable="{StaticResource myDrawable}"
              HeightRequest="300"
              WidthRequest="300" />
```

```csharp
public class MyDrawable : IDrawable
{
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        // All drawing happens here
    }
}
```

- `ICanvas` provides all drawing methods.
- `RectF dirtyRect` is the area that needs redrawing.
- Call `graphicsView.Invalidate()` to trigger a redraw from outside the drawable.

## Drawing Shapes

### Lines

```csharp
canvas.StrokeColor = Colors.Blue;
canvas.StrokeSize = 2;
canvas.DrawLine(10, 10, 200, 10);
```

### Rectangles

```csharp
canvas.StrokeColor = Colors.Black;
canvas.DrawRectangle(10, 10, 100, 50);
canvas.FillColor = Colors.LightBlue;
canvas.FillRectangle(10, 10, 100, 50);
canvas.DrawRoundedRectangle(10, 10, 100, 50, 12);
canvas.FillRoundedRectangle(10, 10, 100, 50, 12);
```

### Ellipses

```csharp
canvas.DrawEllipse(10, 10, 100, 80);
canvas.FillEllipse(10, 10, 100, 80);
```

### Arcs

```csharp
canvas.DrawArc(10, 10, 100, 100, 0, 180, clockwise: true, closed: false);
```

## Paths with PathF

Use `PathF` for complex shapes built from segments.

```csharp
var path = new PathF();
path.MoveTo(10, 10);
path.LineTo(100, 10);
path.LineTo(100, 100);
path.QuadTo(50, 150, 10, 100);   // control point, end point
path.CubicTo(0, 80, 0, 40, 10, 10); // cp1, cp2, end
path.Close();
canvas.StrokeColor = Colors.Red;
canvas.DrawPath(path);
canvas.FillColor = Colors.Orange;
canvas.FillPath(path);
```

- `MoveTo(x, y)` — move without drawing.
- `LineTo(x, y)` — straight line to point.
- `QuadTo(cx, cy, x, y)` — quadratic Bézier curve.
- `CubicTo(c1x, c1y, c2x, c2y, x, y)` — cubic Bézier curve.
- `Close()` — close the path back to the start.

## Drawing Text

```csharp
canvas.FontColor = Colors.Black;
canvas.FontSize = 18;
canvas.Font = Microsoft.Maui.Graphics.Font.Default;
// DrawString with bounding rect and alignment
canvas.DrawString("Hello", 10, 10, 200, 40,
    HorizontalAlignment.Center,
    VerticalAlignment.Center);

// DrawString at a point (no bounding box)
canvas.DrawString("World", 10, 60, HorizontalAlignment.Left);
```

- Set `FontColor`, `FontSize`, and `Font` before calling `DrawString`.
- Overloads accept either a point or a bounding rectangle with alignment.

## Canvas State Properties

Set these properties before draw calls to control appearance.

### Stroke

```csharp
canvas.StrokeColor = Colors.Navy;
canvas.StrokeSize = 4;
canvas.StrokeDashPattern = new float[] { 6, 3 };     // dash, gap
canvas.StrokeLineCap = LineCap.Round;                 // Butt, Round, Square
canvas.StrokeLineJoin = LineJoin.Round;               // Miter, Round, Bevel
canvas.FillColor = Colors.CornflowerBlue;
```

## Shadows

```csharp
canvas.SetShadow(
    offset: new SizeF(5, 5),
    blur: 4,
    color: Colors.Gray);

canvas.FillRectangle(20, 20, 100, 60); // drawn with shadow
canvas.SetShadow(SizeF.Zero, 0, null); // remove shadow
```

## Clipping

Restrict drawing to a region.

```csharp
// Clip to rectangle
canvas.ClipRectangle(20, 20, 100, 100);
// Clip to arbitrary path
var clipPath = new PathF();
clipPath.AppendCircle(80, 80, 50);
canvas.ClipPath(clipPath);
// Subtract a region from the current clip
canvas.SubtractFromClip(40, 40, 30, 30);
```

## Canvas State Management

Use `SaveState` / `RestoreState` to isolate drawing settings.

```csharp
canvas.SaveState();
canvas.StrokeColor = Colors.Red;
canvas.StrokeSize = 6;
canvas.DrawRectangle(10, 10, 80, 80);
canvas.RestoreState();
// StrokeColor, StrokeSize, clip, shadow, etc. are restored
```

- Always pair `SaveState` and `RestoreState`.
- Saves/restores: stroke, fill, font, shadow, clip, and transforms.
- Nest calls for layered state isolation.

## Triggering Redraws

```csharp
graphicsView.Invalidate();
```

- `Invalidate()` queues a redraw; the framework calls `IDrawable.Draw` on the next frame.
- Do not call `Draw` directly; always use `Invalidate()`.
- Avoid calling `Invalidate()` in a tight loop; batch state changes before invalidating.

## Quick Reference

| Method / Property | Purpose |
|---|---|
| `DrawLine` | Stroke a line between two points |
| `DrawRectangle` / `FillRectangle` | Stroke or fill a rectangle |
| `DrawRoundedRectangle` / `FillRoundedRectangle` | Rounded-corner rectangle |
| `DrawEllipse` / `FillEllipse` | Stroke or fill an ellipse |
| `DrawArc` | Stroke an arc segment |
| `DrawPath` / `FillPath` | Stroke or fill a `PathF` |
| `DrawString` | Render text with alignment |
| `SetShadow` | Apply drop shadow to subsequent draws |
| `ClipRectangle` / `ClipPath` | Restrict drawing region |
| `SubtractFromClip` | Remove area from clip |
| `SaveState` / `RestoreState` | Push/pop canvas state |
| `graphicsView.Invalidate()` | Request a redraw |

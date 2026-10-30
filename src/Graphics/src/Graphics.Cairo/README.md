# Microsoft.Maui.Graphics.Cairo

`ICanvas` implementation on Cairo + Pango + gdk-pixbuf (GtkSharp 4 bindings `CairoSharp4`, `PangoSharp4`, `GdkSharp4`; no `GtkSharp4`). Targets `netstandard2.0` and `net10.0`. Used by the Linux/GTK 4 backend (`src/Gtk`) for `GraphicsView`/shapes, and usable on its own (Windows too: any GTK 4 runtime on the library path).

## Types

| Type | Role |
|---|---|
| `CairoCanvas` | `AbstractCanvas<CairoCanvasState>` + `IBlurrableCanvas`; draws into any `Cairo.Context` |
| `CairoCanvasState` / `CairoCanvasStateService` | Colors, fonts, stroke style, alpha, blend, shadow, blur, fill pattern. Transform and clip live in the Cairo context |
| `CairoImage` | `IImage` over an `ARGB32` surface; `FromStream`/`FromBytes`/`FromPixbuf`, `ToPixbuf`, `Resize`, `Downsize`, `Save` (PNG, JPEG, BMP, TIFF) |
| `CairoImageLoadingService` | `IImageLoadingService` (gdk-pixbuf decoding, EXIF orientation applied; `null` on undecodable data) |
| `PangoStringSizeService` | `IStringSizeService` measured with Pango, same metrics as `DrawString` |
| `CairoBitmapExportContext` / `CairoBitmapExportService` | Offscreen rendering to an image surface |
| `CairoExtensions` | `Color`/`Matrix3x2`/`BlendMode` to Cairo, `PathF` to a Cairo path, arc/rounded-rect/ellipse helpers |

## Usage

```csharp
using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, 200, 100);
using var cr = new Cairo.Context(surface);
using var canvas = new CairoCanvas(cr);   // the canvas does not own cr
canvas.FillColor = Colors.CornflowerBlue;
canvas.FillRoundedRectangle(10, 10, 180, 80, 12);
canvas.DrawString("Hello", 20, 60, HorizontalAlignment.Left);
```

Inside `Microsoft.Maui.Graphics.*` the name `Cairo` resolves to this namespace (`Microsoft.Maui.Graphics.Cairo`); use `global::Cairo.X` or a `using CairoContext = global::Cairo.Context;` alias.

## Behaviour notes

| Topic | Behaviour |
|---|---|
| Transforms | Real Cairo transforms. Do **not** wrap in `ScalingCanvas` (that is for canvases whose `Scale` is a no-op, like Skia's) |
| Angles | MAUI convention: degrees, counter-clockwise from 3 o'clock |
| Dash | Pattern and offset are multiples of the stroke size |
| Fill paint | `SolidPaint`, `LinearGradientPaint`, `RadialGradientPaint`, `PatternPaint`, `ImagePaint`; applied with the global `Alpha` at paint time |
| Clip | `SubtractFromClip` = even-odd fill of (clip box + rect); `ClipPath` honours `WindingMode` |
| Shadow / blur | Offscreen layer + 3-pass box blur (sigma = radius / 2). Offset and radius are device pixels, unaffected by the transform. `SetShadow(SizeF.Zero, 0, ...)` clears the shadow. Shadow layers are limited to 64 Mpx |
| Blend modes | Cairo operators (needs Cairo 1.10+). `PlusDarker` has no equivalent and draws normally |
| Text | Pango, absolute pixel sizes, grayscale AA. `DrawString` y is the baseline; boxed overload wraps at word/char, aligns, justifies, clips on `TextFlow.ClipBounds`. `DrawText` supports font name/size, bold, italic, underline, strikethrough, sub/superscript, color, background |
| Fonts | `IFont.Name` null/empty = `Sans` (fontconfig alias) |
| Images | Foreign `IImage` types are re-encoded to PNG once per draw. `ImageFormat.Gif` cannot be saved (`PlatformNotSupportedException`) |

## Tests

`src/Graphics/tests/Graphics.Cairo.Tests` (xUnit, `net10.0`): renders to `ImageSurface` and asserts pixels; also renders every `GraphicsTester` scenario.

```
dotnet test src/Graphics/tests/Graphics.Cairo.Tests/Graphics.Cairo.Tests.csproj
```

Needs the GTK 4 runtime (gdk-pixbuf, Pango, Cairo) discoverable by GtkSharp: `%LOCALAPPDATA%\Gtk\<version>\bin` on Windows, system packages on Linux (`libpango-1.0`, `libpangocairo-1.0`, `libgdk_pixbuf-2.0`, `libcairo`). Headless is fine, no display or `Gtk.Init` is needed.

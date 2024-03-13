namespace Microsoft.Maui.Graphics.Platform.Gtk;

public static class CairoExtensions
{
	public static Cairo.LineJoin ToLineJoin(this LineJoin lineJoin) =>
		lineJoin switch
		{
			LineJoin.Bevel => Cairo.LineJoin.Bevel,
			LineJoin.Round => Cairo.LineJoin.Round,
			_ => Cairo.LineJoin.Miter
		};

	public static Cairo.FillRule ToFillRule(this WindingMode windingMode) =>
		windingMode switch
		{
			WindingMode.EvenOdd => Cairo.FillRule.EvenOdd,
			_ => Cairo.FillRule.Winding
		};

	public static Cairo.LineCap ToLineCap(this LineCap lineCap) =>
		lineCap switch
		{
			LineCap.Butt => Cairo.LineCap.Butt,
			LineCap.Round => Cairo.LineCap.Round,
			_ => Cairo.LineCap.Square
		};

	public static Cairo.Antialias ToAntialias(bool antialias) => antialias ? Cairo.Antialias.Default : Cairo.Antialias.None;

	public static Size? GetSize(this Cairo.Surface it)
	{
		if (it is Cairo.ImageSurface i)
			return new Size(i.Width, i.Height);

		if (it is Cairo.XlibSurface x)
			return new Size(x.Width, x.Height);

		if (it is Cairo.XcbSurface c)
			return null;

		if (it is Cairo.SvgSurface s)
			return null;

		return null;
	}

	public static Cairo.Surface CreateSurface(this Cairo.Context context, bool imageSurface = false)
	{
		var surface = context.GetTarget();

		var extents = context.PathExtents();
		var pathSize = new Size(extents.X + extents.Width, extents.Height + extents.Y);

		var s = surface.GetSize();

		var shadowSurface = s.HasValue && !imageSurface ? surface.CreateSimilar(surface.Content, (int)pathSize.Width, (int)pathSize.Height) : new Cairo.ImageSurface(Cairo.Format.ARGB32, (int)pathSize.Width, (int)pathSize.Height);

		return shadowSurface;
	}
}
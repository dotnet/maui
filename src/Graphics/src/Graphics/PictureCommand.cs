namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Specifies the type of commands that can be recorded and played back in a picture.
	/// </summary>
	public enum PictureCommand
	{
		/// <summary>
		/// Draws a line between two points.
		/// </summary>
		DrawLine = 0,

		/// <summary>
		/// Draws the outline of a rectangle.
		/// </summary>
		DrawRectangle = 1,

		/// <summary>
		/// Draws the outline of a rectangle with rounded corners.
		/// </summary>
		DrawRoundedRectangle = 2,

		/// <summary>
		/// Draws the outline of an ellipse.
		/// </summary>
		DrawEllipse = 3,

		/// <summary>
		/// Draws the outline of a path.
		/// </summary>
		DrawPath = 4,

		/// <summary>
		/// Draws an image.
		/// </summary>
		DrawImage = 5,

		/// <summary>
		/// Draws an arc.
		/// </summary>
		DrawArc = 6,

		/// <summary>
		/// Draws a PDF page.
		/// </summary>
		DrawPdfPage = 7,

		/// <summary>
		/// Fills a rectangle with the current fill color or paint.
		/// </summary>
		FillRectangle = 10,

		/// <summary>
		/// Fills a rectangle with rounded corners with the current fill color or paint.
		/// </summary>
		FillRoundedRectangle = 11,

		/// <summary>
		/// Fills an ellipse with the current fill color or paint.
		/// </summary>
		FillEllipse = 12,

		/// <summary>
		/// Fills a path with the current fill color or paint.
		/// </summary>
		FillPath = 13,

		/// <summary>
		/// Fills an arc with the current fill color or paint.
		/// </summary>
		FillArc = 14,

		/// <summary>
		/// Alternative command for filling a path.
		/// </summary>
		FillPath2 = 15,

		/// <summary>
		/// Draws text at a specific point.
		/// </summary>
		DrawStringAtPoint = 20,

		/// <summary>
		/// Draws text within a rectangle with alignment.
		/// </summary>
		DrawStringInRect = 21,

		/// <summary>
		/// Draws text along a path.
		/// </summary>
		DrawStringInPath = 22,

		/// <summary>
		/// Draws formatted text within a rectangle.
		/// </summary>
		DrawTextInRect = 25,

		/// <summary>
		/// Sets the stroke width.
		/// </summary>
		StrokeSize = 30,

		/// <summary>
		/// Sets the stroke color.
		/// </summary>
		StrokeColor = 31,

		/// <summary>
		/// Sets the stroke dash pattern.
		/// </summary>
		StrokeDashPattern = 32,

		/// <summary>
		/// Sets the stroke line cap style.
		/// </summary>
		StrokeLineCap = 33,

		/// <summary>
		/// Sets the stroke line join style.
		/// </summary>
		StrokeLineJoin = 34,

		/// <summary>
		/// Sets the stroke location.
		/// </summary>
		/// <summary>
		/// Sets the stroke location.
		/// </summary>
		StrokeLocation = 35,

		/// <summary>
		/// Sets the miter limit for stroke line joins.
		/// </summary>
		StrokeMiterLimit = 36,

		/// <summary>
		/// Controls whether stroke scaling is limited.
		/// </summary>
		LimitStrokeScaling = 37,

		/// <summary>
		/// Sets the stroke limit.
		/// </summary>
		StrokeLimit = 38,

		/// <summary>
		/// Sets the stroke brush.
		/// </summary>
		StrokeBrush = 39,

		/// <summary>
		/// Sets the fill color.
		/// </summary>
		FillColor = 40,

		/// <summary>
		/// Sets the fill paint (gradient, pattern, etc.).
		/// </summary>
		FillPaint = 41,

		/// <summary>
		/// Sets the font color.
		/// </summary>
		FontColor = 50,

		/// <summary>
		/// Sets the font name or family.
		/// </summary>
		FontName = 51,

		/// <summary>
		/// Sets the font size.
		/// </summary>
		FontSize = 52,

		/// <summary>
		/// Applies a scaling transformation.
		/// </summary>
		Scale = 60,

		/// <summary>
		/// Applies a translation transformation.
		/// </summary>
		Translate = 61,

		/// <summary>
		/// Applies a rotation transformation around the origin.
		/// </summary>
		Rotate = 62,

		/// <summary>
		/// Applies a rotation transformation around a specified point.
		/// </summary>
		RotateAtPoint = 63,

		/// <summary>
		/// Concatenates a transformation matrix with the current transformation.
		/// </summary>
		ConcatenateTransform = 64,

		/// <summary>
		/// Sets shadow properties.
		/// </summary>
		Shadow = 70,

		/// <summary>
		/// Sets the global alpha (transparency) value.
		/// </summary>
		Alpha = 71,

		/// <summary>
		/// Sets the blend mode for compositing.
		/// </summary>
		BlendMode = 72,

		/// <summary>
		/// Subtracts a region from the current clipping area.
		/// </summary>
		SubtractFromClip = 80,

		/// <summary>
		/// Sets the clipping area to a path.
		/// </summary>
		ClipPath = 81,

		/// <summary>
		/// Sets the clipping area to a rectangle.
		/// </summary>
		ClipRectangle = 82,

		/// <summary>
		/// Subtracts a path from the current clipping area.
		/// </summary>
		SubtractPathFromClip = 83,

		/// <summary>
		/// Saves the current graphics state on a stack.
		/// </summary>
		SaveState = 100,

		/// <summary>
		/// Restores the most recently saved graphics state.
		/// </summary>
		RestoreState = 101,

		/// <summary>
		/// Resets the graphics state to its default values.
		/// </summary>
		ResetState = 102,

		SystemFont = 110,
		BoldSystemFont = 111
	}
}

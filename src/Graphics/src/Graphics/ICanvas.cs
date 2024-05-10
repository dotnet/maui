using System.Numerics;
using Microsoft.Maui.Graphics.Text;

namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Represents a platform-agnostic canvas on which 2D graphics can be drawn using types from the <see cref="Microsoft.Maui.Graphics"/> namespace.
	/// </summary>
	public interface ICanvas
	{
		/// <summary>
		/// Gets or sets a value that represents the scaling factor to scale the UI by. 
		/// </summary>
		public float DisplayScale { get; set; }

		/// <summary>
		/// Sets the width of the stroke used to draw an object's outline.
		/// </summary>
		public float StrokeSize { set; }

		/// <summary>
		/// Sets the limit of the miter length of line joins in an object.
		/// </summary>
		public float MiterLimit { set; }

		/// <summary>
		/// Sets the <see cref="Color"/> used to paint an object's outline.
		/// </summary>
		public Color StrokeColor { set; }

		/// <summary>
		/// Sets the shape at the start and end of a line.
		/// </summary>
		public LineCap StrokeLineCap { set; }

		/// <summary>
		/// Sets the type of join used at the vertices of a shape.
		/// </summary>
		public LineJoin StrokeLineJoin { set; }

		/// <summary>
		/// Sets the pattern of dashes and gaps that are used to outline an object.
		/// </summary>
		public float[] StrokeDashPattern { set; }

		/// <summary>
		/// Sets the distance within the dash pattern where a dash begins.
		/// </summary>
		public float StrokeDashOffset { set; }

		/// <summary>
		/// Sets the color used to paint an object's interior.
		/// </summary>
		public Color FillColor { set; }

		/// <summary>
		/// Sets the font color when drawing text.
		/// </summary>
		public Color FontColor { set; }

		/// <summary>
		/// Sets the font used when drawing text.
		/// </summary>
		public IFont Font { set; }

		/// <summary>
		/// Sets the size of the font used when drawing text.
		/// </summary>
		public float FontSize { set; }

		/// <summary>
		/// Sets the opacity of am object.
		/// </summary>
		public float Alpha { set; }

		/// <summary>
		/// Sets a value that indicates whether to use anti-aliasing is enabled.
		/// </summary>
		public bool Antialias { set; }

		/// <summary>
		/// Sets the blend mode, which determines what happens when an object is rendered on top of an existing object.
		/// </summary>
		public BlendMode BlendMode { set; }

		/// <summary>
		/// Draws the specified <paramref name="path"/> onto the canvas.
		/// </summary>
		/// <param name="path">The path to be drawn.</param>
		public void DrawPath(PathF path);

		/// <summary>
		/// Draws and fills the specified <paramref name="path"/> onto the canvas.
		/// </summary>
		/// <param name="path">The path to be drawn.</param>
		/// <param name="windingMode">The fill algorithm to be used.</param>
		public void FillPath(PathF path, WindingMode windingMode);

		/// <summary>
		/// Clips an object so that only the area outside the of a rectangle will be visible.
		/// </summary>
		/// <param name="x">Starting <c>x</c> coordinate of the rectangle.</param>
		/// <param name="y">Starting <c>y</c> coordinate of the rectangle.</param>
		/// <param name="width">Width of the rectangle.</param>
		/// <param name="height">Height of the rectangle.</param>
		public void SubtractFromClip(float x, float y, float width, float height);

		/// <summary>
		/// Clips an object so that only the area outside of a <see cref="PathF"/> object will be visible.
		/// </summary>
		/// <param name="path">The path used to clip the object</param>
		/// <param name="windingMode">Fill algorithm used for the path. Default is <see cref="WindingMode.NonZero"/>.</param>
		public void ClipPath(PathF path, WindingMode windingMode = WindingMode.NonZero);

		/// <summary>
		/// Clips an object so that only the area that's within the region of the rectangle will be visible.
		/// </summary>
		/// <param name="x">Starting <c>x</c> coordinate of the rectangle.</param>
		/// <param name="y">Starting <c>y</c> coordinate of the rectangle.</param>
		/// <param name="width">Width of the rectangle.</param>
		/// <param name="height">Height of the rectangle.</param>
		public void ClipRectangle(float x, float y, float width, float height);

		/// <summary>
		/// Draws a line between two points onto the canvas.
		/// </summary>
		/// <param name="x1">Starting <c>x</c> coordinate.</param>
		/// <param name="y1">Starting <c>y</c> coordinate.</param>
		/// <param name="x2">Ending <c>x</c> coordinate.</param>
		/// <param name="y2">Ending <c>x</c> coordinate.</param>
		public void DrawLine(float x1, float y1, float x2, float y2);

		/// <summary>
		/// Draws an arc onto the canvas.
		/// </summary>
		/// <param name="x">Starting <c>x</c> coordinate.</param>
		/// <param name="y">Starting <c>y</c> coordinate.</param>
		/// <param name="width">Width of the arc.</param>
		/// <param name="height">Height of the arc.</param>
		/// <param name="startAngle">The angle from the x-axis to the start point of the arc.</param>
		/// <param name="endAngle">The angle from the x-axis to the end point of the arc.</param>
		/// <param name="clockwise"><see langword="true"/> to draw the arc in a clockwise direction; <see langword="false"/> to draw the arc counterclockwise.</param>
		/// <param name="closed"><see langword="true"/> to specify whether the end point of the arc will be connected to the start point; <see langword="false"/> otherwise.</param>
		public void DrawArc(float x, float y, float width, float height, float startAngle, float endAngle, bool clockwise, bool closed);

		/// <summary>
		/// Draws a filled arc onto the canvas.
		/// </summary>
		/// <param name="x">Starting <c>x</c> coordinate.</param>
		/// <param name="y">Starting <c>y</c> coordinate.</param>
		/// <param name="width">Width of the arc.</param>
		/// <param name="height">Height of the arc.</param>
		/// <param name="startAngle">The angle from the x-axis to the start point of the arc.</param>
		/// <param name="endAngle">The angle from the x-axis to the end point of the arc.</param>
		/// <param name="clockwise"><see langword="true"/> to draw the arc in a clockwise direction; <see langword="false"/> to draw the arc counterclockwise.</param>
		public void FillArc(float x, float y, float width, float height, float startAngle, float endAngle, bool clockwise);

		/// <summary>
		/// Draws a rectangle onto the canvas.
		/// </summary>
		/// <param name="x">Starting <c>x</c> coordinate.</param>
		/// <param name="y">Starting <c>y</c> coordinate.</param>
		/// <param name="width">Width of the rectangle.</param>
		/// <param name="height">Height of the rectangle.</param>
		public void DrawRectangle(float x, float y, float width, float height);

		/// <summary>
		/// Draws a filled rectangle onto the canvas.
		/// </summary>
		/// <param name="x">Starting <c>x</c> coordinate.</param>
		/// <param name="y">Starting <c>y</c> coordinate.</param>
		/// <param name="width">Width of the rectangle.</param>
		/// <param name="height">Height of the rectangle.</param>
		public void FillRectangle(float x, float y, float width, float height);

		/// <summary>
		/// Draws a rectangle with rounded corners onto the canvas.
		/// </summary>
		/// <param name="x">Starting <c>x</c> coordinate.</param>
		/// <param name="y">Starting <c>y</c> coordinate.</param>
		/// <param name="width">Width of the rectangle.</param>
		/// <param name="height">Height of the rectangle.</param>
		/// <param name="cornerRadius">The radius used to round the corners of the rectangle.</param>
		public void DrawRoundedRectangle(float x, float y, float width, float height, float cornerRadius);

		/// <summary>
		/// Draws a filled rectangle with rounded corners onto the canvas.
		/// </summary>
		/// <param name="x">Starting <c>x</c> coordinate.</param>
		/// <param name="y">Starting <c>y</c> coordinate.</param>
		/// <param name="width">Width of the rectangle.</param>
		/// <param name="height">Height of the rectangle.</param>
		/// <param name="cornerRadius">The radius used to round the corners of the rectangle.</param>
		public void FillRoundedRectangle(float x, float y, float width, float height, float cornerRadius);

		/// <summary>
		/// Draws an ellipse onto the canvas.
		/// </summary>
		/// <param name="x">Starting <c>x</c> coordinate.</param>
		/// <param name="y">Starting <c>y</c> coordinate.</param>
		/// <param name="width">Width of the ellipse.</param>
		/// <param name="height">Height of the ellipse.</param>
		public void DrawEllipse(float x, float y, float width, float height);

		/// <summary>
		/// Draws a filled ellipse onto the canvas.
		/// </summary>
		/// <param name="x">Starting <c>x</c> coordinate.</param>
		/// <param name="y">Starting <c>y</c> coordinate.</param>
		/// <param name="width">Width of the ellipse.</param>
		/// <param name="height">Height of the ellipse.</param>
		public void FillEllipse(float x, float y, float width, float height);

		/// <summary>
		/// Draws a text string onto the canvas.
		/// </summary>
		/// <remarks>To draw attributed text, use <see cref="DrawText(IAttributedText, float, float, float, float)"/> instead.</remarks>
		/// <param name="value">Text to be displayed.</param>
		/// <param name="x">Starting <c>x</c> coordinate.</param>
		/// <param name="y">Starting <c>y</c> coordinate.</param>
		/// <param name="horizontalAlignment">Horizontal alignment options to align the string.</param>
		public void DrawString(string value, float x, float y, HorizontalAlignment horizontalAlignment);

		/// <summary>
		/// Draws a text string within a bounding box onto the canvas.
		/// </summary>
		/// <param name="value">Text to be displayed.</param>
		/// <param name="x">Starting <c>x</c> coordinate of the bounding box.</param>
		/// <param name="y">Starting <c>y</c> coordinate of the bounding box.</param>
		/// <param name="width">Width of the bounding box.</param>
		/// <param name="height">Height of the bounding box.</param>
		/// <param name="horizontalAlignment">Horizontal alignment options to align the string within the bounding box.</param>
		/// <param name="verticalAlignment">Vertical alignment options to align the string within the bounding box.</param>
		/// <param name="textFlow">Specifies whether text will be clipped in case it overflows the bounding box. Default is <see cref="TextFlow.ClipBounds"/>.</param>
		/// <param name="lineSpacingAdjustment">Spacing adjustment between lines. Default is 0.</param>
		public void DrawString(
			string value,
			float x,
			float y,
			float width,
			float height,
			HorizontalAlignment horizontalAlignment,
			VerticalAlignment verticalAlignment,
			TextFlow textFlow = TextFlow.ClipBounds,
			float lineSpacingAdjustment = 0);

		/// <summary>
		/// Draws attributed text within a bounding box onto the canvas.
		/// </summary>
		/// <param name="value">Attributed text to be displayed.</param>
		/// <param name="x">Starting <c>x</c> coordinate of the bounding box.</param>
		/// <param name="y">Starting <c>y</c> coordinate of the bounding box.</param>
		/// <param name="width">Width of the bounding box.</param>
		/// <param name="height">Height of the bounding box.</param>
		public void DrawText(
			IAttributedText value,
			float x,
			float y,
			float width,
			float height);

		/// <summary>
		/// Rotates a graphical object around a point.
		/// </summary>
		/// <remarks>Rotation is clockwise for increasing angles. Negative angles and angles greater than 360 are allowed.</remarks>
		/// <param name="degrees">Rotation angle.</param>
		/// <param name="x"><c>x</c> coordinate of the rotation point.</param>
		/// <param name="y"><c>y</c> coordinate of the rotation point.</param>
		public void Rotate(float degrees, float x, float y);

		/// <summary>
		/// Rotates a graphical object around the upper-left corner of the canvas (0,0).
		/// </summary>
		/// <remarks>Rotation is clockwise for increasing angles. Negative angles and angles greater than 360 are allowed.</remarks>
		public void Rotate(float degrees);


		/// <summary>
		/// Changes the size of a graphical object by scaling it.
		/// </summary>
		/// <remarks>Can cause starting coordinates to move when an object is made larger.</remarks>
		/// <param name="sx">Value for horizontal scaling.</param>
		/// <param name="sy">Value for vertical scaling.</param>
		public void Scale(float sx, float sy);

		/// <summary>
		/// Shifts a graphical object in horizontal and vertical directions.
		/// </summary>
		/// <param name="tx">Horizontal shift. Negative values move the object to the left, while positive values move it to the right.</param>
		/// <param name="ty">Vertical shift. Negative values move the object down, while positive values move it up.</param>
		public void Translate(float tx, float ty);

		/// <summary>
		/// Applies transformation specified by <paramref name="transform"/> to a graphical object.
		/// </summary>
		/// <param name="transform">Affine transformation matrix.</param>
		public void ConcatenateTransform(Matrix3x2 transform);

		/// <summary>
		/// Saves the current graphics state.
		/// </summary>
		public void SaveState();

		/// <summary>
		/// Restores the graphics state to the most recently saved state.
		/// </summary>
		/// <returns><see langword="true"/> if the restore was succesful, <see langword="false"/> otherwise.</returns>
		public bool RestoreState();

		/// <summary>
		/// Resets the graphics state to its default values.
		/// </summary>
		public void ResetState();

		/// <summary>
		/// Adds a shadow to a graphical object.
		/// </summary>
		/// <param name="offset">Represents the position of a light source that creates the shadow.</param>
		/// <param name="blur">Amount of blur to apply to the shadow.</param>
		/// <param name="color">Color of the shadow.</param>
		public void SetShadow(SizeF offset, float blur, Color color);

		/// <summary>
		/// Sets <paramref name="paint"/> as the fill of a graphical object.
		/// </summary>
		/// <param name="paint">Paint to set as fill</param>
		/// <param name="rectangle">Rectangle to apply a gradient on if the <paramref name="paint"/> supports it.</param>
		public void SetFillPaint(Paint paint, RectF rectangle);

		/// <summary>
		/// Draws an image onto the canvas.
		/// </summary>
		/// <param name="image">Image to display</param>
		/// <param name="x">Top left corner <c>x</c> coordinate.</param>
		/// <param name="y">Top left corner <c>y</c> coordinate.</param>
		/// <param name="width">Width of the image.</param>
		/// <param name="height">Height of the image.</param>
		public void DrawImage(IImage image, float x, float y, float width, float height);

		/// <summary>
		/// Calculates the area a string would occupy if drawn on the canvas.
		/// </summary>
		/// <param name="value">String to calculate the size on.</param>
		/// <param name="font">The string's font type.</param>
		/// <param name="fontSize">The string's font size.</param>
		/// <returns>The area the string would occupy on the canvas.</returns>
		public SizeF GetStringSize(string value, IFont font, float fontSize);

		/// <summary>
		/// Calculates the area a string would occupy if drawn on the canvas.
		/// </summary>
		/// <param name="value">String to calculate the size on.</param>
		/// <param name="font">The string's font type.</param>
		/// <param name="fontSize">The string's font size.</param>
		/// <param name="horizontalAlignment">Horizontal alignment options for the string.</param>
		/// <param name="verticalAlignment">Vertical alignment options for the string.</param>
		/// <returns>The area the string would occupy on the canvas.</returns>
		public SizeF GetStringSize(string value, IFont font, float fontSize, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment);
	}
}

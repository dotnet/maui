using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public interface IAdornerBorder : IDrawable
	{
		float DPI { get; }

		IView VisualView { get; }

		Rectangle Offset { get; }

		Color FillColor { get; }

		Color StrokeColor { get; }
	}
}

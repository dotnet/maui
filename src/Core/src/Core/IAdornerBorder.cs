using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public interface IAdornerBorder : IDrawable
	{
		/// <summary>
		/// Gets the DPI for the Adorner Border.
		/// Used to override the default DPI behavior for the containing border. 
		/// </summary>
		float DPI { get; }

		/// <summary>
		/// Gets the underlying <see cref="IView"/> that makes up the border.
		/// </summary>
		IView VisualView { get; }

		/// <summary>
		/// Gets the offset rectangle value for where to draw the border.
		/// Used to override the default Frame/Bounds controls of the underlying native element.
		/// Ex. Android's Status Bar height that does not reflect in the underlying APIs.
		/// </summary>
		Rectangle Offset { get; }

		/// <summary>
		/// Gets the fill color for the Adorner Border.
		/// </summary>
		Color FillColor { get; }

		/// <summary>
		/// Gets the stroke color for the Adorner Border.
		/// </summary>
		Color StrokeColor { get; }
	}
}

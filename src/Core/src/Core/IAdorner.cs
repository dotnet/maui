using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public interface IAdorner : IWindowOverlayElement
	{
		/// <summary>
		/// Gets the Density for the Adorner.
		/// Used to override the default density behavior for the containing border. 
		/// </summary>
		float Density { get; }

		/// <summary>
		/// Gets the underlying <see cref="IView"/> that makes up the border.
		/// </summary>
		IView VisualView { get; }

		/// <summary>
		/// Gets the offset point value for where to draw the border.
		/// Used to override the default Frame/Bounds controls of the underlying native element.
		/// Ex. Android's Status Bar height that does not reflect in the underlying APIs.
		/// </summary>
		Point Offset { get; }

		/// <summary>
		/// Gets the fill color for the Adorner.
		/// </summary>
		Color FillColor { get; }

		/// <summary>
		/// Gets the stroke color for the Adorner.
		/// </summary>
		Color StrokeColor { get; }
	}
}

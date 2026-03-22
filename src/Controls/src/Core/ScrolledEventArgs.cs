#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>Event arguments for the <see cref="ScrollView.Scrolled"/> event.</summary>
	public class ScrolledEventArgs : EventArgs
	{
		/// <summary>Creates a new <see cref="ScrolledEventArgs"/> with the specified scroll positions.</summary>
		/// <param name="x">The horizontal scroll position.</param>
		/// <param name="y">The vertical scroll position.</param>
		public ScrolledEventArgs(double x, double y)
		{
			ScrollX = x;
			ScrollY = y;
		}

		/// <summary>Gets the current horizontal scroll position.</summary>
		public double ScrollX { get; private set; }

		/// <summary>Gets the current vertical scroll position.</summary>
		public double ScrollY { get; private set; }
	}
}
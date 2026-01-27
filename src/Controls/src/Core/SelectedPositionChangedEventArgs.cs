#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>Event arguments for position changes in a CarouselPage.</summary>
	public class SelectedPositionChangedEventArgs : EventArgs
	{
		/// <summary>Creates a new <see cref="SelectedPositionChangedEventArgs"/> with the specified position.</summary>
		/// <param name="selectedPosition">The index of the newly selected position.</param>
		public SelectedPositionChangedEventArgs(int selectedPosition)
		{
			SelectedPosition = selectedPosition;
		}

		/// <summary>Gets the index of the newly selected position.</summary>
		public object SelectedPosition { get; private set; }
	}
}
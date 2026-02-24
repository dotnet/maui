#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Provides data for the PositionChanged event in carousel and collection views.
	/// </summary>
	/// <remarks>
	/// This event args class is used when the current position changes in a <see cref="CarouselView"/> or <see cref="IndicatorView"/>.
	/// It provides both the previous and current positions as zero-based indices.
	/// </remarks>
	public class PositionChangedEventArgs : EventArgs
	{
		/// <summary>
		/// Gets the position index that was current before the change.
		/// </summary>
		/// <value>The zero-based index of the previously current position.</value>
		public int PreviousPosition { get; }
		
		/// <summary>
		/// Gets the position index that is now current after the change.
		/// </summary>
		/// <value>The zero-based index of the currently selected position.</value>
		public int CurrentPosition { get; }

		internal PositionChangedEventArgs(int previousPosition, int currentPosition)
		{
			PreviousPosition = previousPosition;
			CurrentPosition = currentPosition;
		}
	}
}

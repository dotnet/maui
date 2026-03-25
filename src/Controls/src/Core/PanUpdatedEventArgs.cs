#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>Event arguments for the <see cref="PanGestureRecognizer.PanUpdated"/> event.</summary>
	public class PanUpdatedEventArgs : EventArgs
	{
		/// <summary>Initializes a new instance of the <see cref="PanUpdatedEventArgs"/> class with translation data.</summary>
		/// <param name="type">The gesture status.</param>
		/// <param name="gestureId">The unique identifier for this gesture.</param>
		/// <param name="totalx">The total X translation since the gesture started.</param>
		/// <param name="totaly">The total Y translation since the gesture started.</param>
		public PanUpdatedEventArgs(GestureStatus type, int gestureId, double totalx, double totaly) : this(type, gestureId)
		{
			TotalX = totalx;
			TotalY = totaly;
		}

		/// <summary>Initializes a new instance of the <see cref="PanUpdatedEventArgs"/> class.</summary>
		/// <param name="type">The gesture status.</param>
		/// <param name="gestureId">The unique identifier for this gesture.</param>
		public PanUpdatedEventArgs(GestureStatus type, int gestureId)
		{
			StatusType = type;
			GestureId = gestureId;
		}

		/// <summary>Gets the identifier for the gesture that raised the event.</summary>
		public int GestureId { get; }

		/// <summary>Gets a value that tells if this event is for a newly started gesture, a running gesture, a completed gesture, or a canceled gesture.</summary>
		public GestureStatus StatusType { get; }

		/// <summary>Gets the total change in the X direction since the beginning of the gesture.</summary>
		public double TotalX { get; }

		/// <summary>Gets the total change in the Y direction since the beginning of the gesture.</summary>
		public double TotalY { get; }
	}
}
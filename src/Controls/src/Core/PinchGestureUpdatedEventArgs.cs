#nullable disable
using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <summary>Event arguments for the <see cref="PinchGestureRecognizer.PinchUpdated"/> event.</summary>
	public class PinchGestureUpdatedEventArgs : EventArgs
	{
		/// <summary>Initializes a new instance of the <see cref="PinchGestureUpdatedEventArgs"/> class with scale and origin data.</summary>
		/// <param name="status">The gesture status.</param>
		/// <param name="scale">The relative scale of the pinch.</param>
		/// <param name="origin">The origin point of the pinch gesture.</param>
		public PinchGestureUpdatedEventArgs(GestureStatus status, double scale, Point origin) : this(status)
		{
			ScaleOrigin = origin;
			Scale = scale;
		}

		/// <summary>Initializes a new instance of the <see cref="PinchGestureUpdatedEventArgs"/> class.</summary>
		/// <param name="status">The gesture status.</param>
		public PinchGestureUpdatedEventArgs(GestureStatus status)
		{
			Status = status;
		}

		/// <summary>Gets the relative scale of the pinch gesture since the last update. Default is 1.</summary>
		public double Scale { get; } = 1;

		/// <summary>Gets the origin point of the pinch gesture, which is the center between the two touch points.</summary>
		public Point ScaleOrigin { get; }

		/// <summary>Gets a value indicating whether the gesture started, is running, or has finished.</summary>
		public GestureStatus Status { get; }
	}
}
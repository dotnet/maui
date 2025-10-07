#nullable disable
using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <summary>Event arguments for the <see cref="Microsoft.Maui.Controls.PinchGestureRecognizer.PinchUpdated"/> event.</summary>
	public class PinchGestureUpdatedEventArgs : EventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/PinchGestureUpdatedEventArgs.xml" path="//Member[@MemberName='.ctor'][2]/Docs/*" />
		public PinchGestureUpdatedEventArgs(GestureStatus status, double scale, Point origin) : this(status)
		{
			ScaleOrigin = origin;
			Scale = scale;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/PinchGestureUpdatedEventArgs.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public PinchGestureUpdatedEventArgs(GestureStatus status)
		{
			Status = status;
		}

		/// <summary>The relative size of the user's pinch gesture since the last update was received.</summary>
		/// <remarks>The initial value of the</remarks>
		public double Scale { get; } = 1;

		/// <summary>The updated origin of the pinch gesture.</summary>
		/// <remarks>The origin of the pinch is the center of the pinch gesture, and changes if the user translates their pinch while they scale. Application developers may want to store the pinch origin when the gesture begins and use it for all scaling operations for that gesture.</remarks>
		public Point ScaleOrigin { get; }

		/// <summary>Whether the gesture started, is running, or has finished.</summary>
		/// <remarks>The origin of the pinch,
		/// The initial value of the</remarks>
		public GestureStatus Status { get; }
	}
}
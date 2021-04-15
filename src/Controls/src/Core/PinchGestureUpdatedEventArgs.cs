using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public class PinchGestureUpdatedEventArgs : EventArgs
	{
		public PinchGestureUpdatedEventArgs(GestureStatus status, double scale, Point origin) : this(status)
		{
			ScaleOrigin = origin;
			Scale = scale;
		}

		public PinchGestureUpdatedEventArgs(GestureStatus status)
		{
			Status = status;
		}

		public double Scale { get; } = 1;

		public Point ScaleOrigin { get; }

		public GestureStatus Status { get; }
	}
}
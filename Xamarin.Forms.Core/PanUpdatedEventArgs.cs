using System;

namespace Xamarin.Forms
{
	public class PanUpdatedEventArgs : EventArgs
	{
		public PanUpdatedEventArgs(GestureStatus type, int gestureId, double totalx, double totaly) : this(type, gestureId)
		{
			TotalX = totalx;
			TotalY = totaly;
		}

		public PanUpdatedEventArgs(GestureStatus type, int gestureId)
		{
			StatusType = type;
			GestureId = gestureId;
		}

		public int GestureId { get; }

		public GestureStatus StatusType { get; }

		public double TotalX { get; }

		public double TotalY { get; }
	}
}
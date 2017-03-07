using System;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	public sealed class PinchGestureRecognizer : GestureRecognizer, IPinchGestureController
	{
		bool IPinchGestureController.IsPinching { get; set; }

		void IPinchGestureController.SendPinch(Element sender, double delta, Point currentScalePoint)
		{
			EventHandler<PinchGestureUpdatedEventArgs> handler = PinchUpdated;
			if (handler != null)
			{
				handler(sender, new PinchGestureUpdatedEventArgs(GestureStatus.Running, delta, currentScalePoint));
			}
			((IPinchGestureController)this).IsPinching = true;
		}

		void IPinchGestureController.SendPinchCanceled(Element sender)
		{
			EventHandler<PinchGestureUpdatedEventArgs> handler = PinchUpdated;
			if (handler != null)
			{
				handler(sender, new PinchGestureUpdatedEventArgs(GestureStatus.Canceled));
			}
			((IPinchGestureController)this).IsPinching = false;
		}

		void IPinchGestureController.SendPinchEnded(Element sender)
		{
			EventHandler<PinchGestureUpdatedEventArgs> handler = PinchUpdated;
			if (handler != null)
			{
				handler(sender, new PinchGestureUpdatedEventArgs(GestureStatus.Completed));
			}
			((IPinchGestureController)this).IsPinching = false;
		}

		void IPinchGestureController.SendPinchStarted(Element sender, Point initialScalePoint)
		{
			EventHandler<PinchGestureUpdatedEventArgs> handler = PinchUpdated;
			if (handler != null)
			{
				handler(sender, new PinchGestureUpdatedEventArgs(GestureStatus.Started, 1, initialScalePoint));
			}
			((IPinchGestureController)this).IsPinching = true;
		}

		public event EventHandler<PinchGestureUpdatedEventArgs> PinchUpdated;
	}
}
#nullable disable
using System;
using System.ComponentModel;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/PinchGestureRecognizer.xml" path="Type[@FullName='Microsoft.Maui.Controls.PinchGestureRecognizer']/Docs/*" />
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

			(this as IPinchGestureController).IsPinching = true;
		}

		void IPinchGestureController.SendPinchCanceled(Element sender)
		{
			EventHandler<PinchGestureUpdatedEventArgs> handler = PinchUpdated;
			if (handler != null)
			{
				handler(sender, new PinchGestureUpdatedEventArgs(GestureStatus.Canceled));
			}

			(this as IPinchGestureController).IsPinching = false;
		}

		void IPinchGestureController.SendPinchEnded(Element sender)
		{
			EventHandler<PinchGestureUpdatedEventArgs> handler = PinchUpdated;
			if (handler != null)
			{
				handler(sender, new PinchGestureUpdatedEventArgs(GestureStatus.Completed));
			}

			(this as IPinchGestureController).IsPinching = false;
		}

		void IPinchGestureController.SendPinchStarted(Element sender, Point initialScalePoint)
		{
			EventHandler<PinchGestureUpdatedEventArgs> handler = PinchUpdated;
			if (handler != null)
			{
				handler(sender, new PinchGestureUpdatedEventArgs(GestureStatus.Started, 1, initialScalePoint));
			}

			(this as IPinchGestureController).IsPinching = true;
		}

		public event EventHandler<PinchGestureUpdatedEventArgs> PinchUpdated;
	}
}
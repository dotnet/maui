using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public interface IPinchGestureController
	{
		bool IsPinching { get; set; }

		void SendPinch(Element sender, double scale, Point currentScalePoint);

		void SendPinchCanceled(Element sender);

		void SendPinchEnded(Element sender);

		void SendPinchStarted(Element sender, Point intialScalePoint);
	}
}
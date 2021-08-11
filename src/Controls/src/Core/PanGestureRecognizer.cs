using System;
using System.ComponentModel;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	public class PanGestureRecognizer : GestureRecognizer, IPanGestureController
	{
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static AutoId CurrentId { get; } = new();

		public static readonly BindableProperty TouchPointsProperty = BindableProperty.Create("TouchPoints", typeof(int), typeof(PanGestureRecognizer), 1);

		public int TouchPoints
		{
			get { return (int)GetValue(TouchPointsProperty); }
			set { SetValue(TouchPointsProperty, value); }
		}

		void IPanGestureController.SendPan(Element sender, double totalX, double totalY, int gestureId)
		{
			PanUpdated?.Invoke(sender, new PanUpdatedEventArgs(GestureStatus.Running, gestureId, totalX, totalY));
		}

		void IPanGestureController.SendPanCanceled(Element sender, int gestureId)
		{
			PanUpdated?.Invoke(sender, new PanUpdatedEventArgs(GestureStatus.Canceled, gestureId));
		}

		void IPanGestureController.SendPanCompleted(Element sender, int gestureId)
		{
			PanUpdated?.Invoke(sender, new PanUpdatedEventArgs(GestureStatus.Completed, gestureId));
		}

		void IPanGestureController.SendPanStarted(Element sender, int gestureId)
		{
			PanUpdated?.Invoke(sender, new PanUpdatedEventArgs(GestureStatus.Started, gestureId));
		}

		public event EventHandler<PanUpdatedEventArgs> PanUpdated;
	}
}
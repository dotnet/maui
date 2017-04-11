using System;
using System.ComponentModel;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	public class PanGestureRecognizer : GestureRecognizer, IPanGestureController
	{
		public static readonly BindableProperty TouchPointsProperty = BindableProperty.Create("TouchPoints", typeof(int), typeof(PanGestureRecognizer), 1);

		public int TouchPoints
		{
			get { return (int)GetValue(TouchPointsProperty); }
			set { SetValue(TouchPointsProperty, value); }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendPan(Element sender, double totalX, double totalY, int gestureId)
		{
			PanUpdated?.Invoke(sender, new PanUpdatedEventArgs(GestureStatus.Running, gestureId, totalX, totalY));
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendPanCanceled(Element sender, int gestureId)
		{
			PanUpdated?.Invoke(sender, new PanUpdatedEventArgs(GestureStatus.Canceled, gestureId));
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendPanCompleted(Element sender, int gestureId)
		{
			PanUpdated?.Invoke(sender, new PanUpdatedEventArgs(GestureStatus.Completed, gestureId));
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendPanStarted(Element sender, int gestureId)
		{
			PanUpdated?.Invoke(sender, new PanUpdatedEventArgs(GestureStatus.Started, gestureId));
		}

		public event EventHandler<PanUpdatedEventArgs> PanUpdated;
	}
}
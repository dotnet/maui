#nullable disable
using System;
using System.ComponentModel;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	/// <summary>A gesture recognizer for panning content that is larger than its parent view.</summary>
	public class PanGestureRecognizer : GestureRecognizer, IPanGestureController
	{
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static AutoId CurrentId { get; } = new();

		/// <summary>Bindable property for <see cref="TouchPoints"/>.</summary>
		public static readonly BindableProperty TouchPointsProperty = BindableProperty.Create(nameof(TouchPoints), typeof(int), typeof(PanGestureRecognizer), 1);

		/// <summary>Gets or sets the number of touch points in the gesture. This is a bindable property.</summary>
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

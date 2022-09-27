using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using Microsoft.Maui.Controls.Platform;
using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	/// <summary>
	/// Base class for view renderers.
	/// </summary>
	[Obsolete("Use Microsoft.Maui.Controls.Handlers.Compatibility.ViewRenderer instead")]
	public abstract class ViewRenderer<TView, TNativeView> : VisualElementRenderer<TView>
		where TView : View
		where TNativeView : NView
	{
		ObservableCollection<IGestureRecognizer> GestureRecognizers => Element.GestureRecognizers as ObservableCollection<IGestureRecognizer>;

		internal GestureDetector GestureDetector { get; private set; }

		/// <summary>
		/// Native control associated with this renderer.
		/// </summary>
		public TNativeView Control
		{
			get
			{
				return (TNativeView)NativeView;
			}
		}

		protected override void OnElementChanged(ElementChangedEventArgs<TView> e)
		{
			base.OnElementChanged(e);
			GestureRecognizers.CollectionChanged += OnGestureRecognizerCollectionChanged;
			if (Element.GestureRecognizers.Count > 0)
			{
				GestureDetector = new GestureDetector(this);
				GestureDetector.AddGestures(Element.GestureRecognizers);
			}
		}

		protected void SetNativeControl(TNativeView control)
		{
			Debug.Assert(control != null);
			SetNativeView(control);
		}

		protected override void UpdateIsEnabled(bool initialize)
		{
			base.UpdateIsEnabled(initialize);
			if (initialize && Element.IsEnabled)
				return;

			if (GestureDetector != null)
			{
				GestureDetector.IsEnabled = Element.IsEnabled;
			}
		}

		void OnGestureRecognizerCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (GestureDetector == null)
			{
				GestureDetector = new GestureDetector(this);
			}

			// Gestures will be registered/unregistered according to changes in the GestureRecognizers list
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					GestureDetector.AddGestures(e.NewItems.OfType<IGestureRecognizer>());
					break;

				case NotifyCollectionChangedAction.Replace:
					GestureDetector.RemoveGestures(e.OldItems.OfType<IGestureRecognizer>());
					GestureDetector.AddGestures(e.NewItems.OfType<IGestureRecognizer>());
					break;

				case NotifyCollectionChangedAction.Remove:
					GestureDetector.RemoveGestures(e.OldItems.OfType<IGestureRecognizer>());
					break;

				case NotifyCollectionChangedAction.Reset:
					GestureDetector.Clear();
					break;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				GestureDetector?.Clear();
			}
			base.Dispose(disposing);
		}

	}
}

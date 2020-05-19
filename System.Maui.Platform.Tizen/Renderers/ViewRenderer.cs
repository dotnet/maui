using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using ElmSharp;
using ElmSharp.Wearable;
using Specific = System.Maui.PlatformConfiguration.TizenSpecific.Application;

namespace System.Maui.Platform.Tizen
{
	/// <summary>
	/// Base class for view renderers.
	/// </summary>
	public abstract class ViewRenderer<TView, TNativeView> : VisualElementRenderer<TView>
		where TView : View
		where TNativeView : EvasObject
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
			if (GestureDetector != null)
			{
				GestureRecognizers.CollectionChanged -= OnGestureRecognizerCollectionChanged;
				GestureDetector.Clear();
				GestureDetector = null;
			}

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

		protected override void OnFocused(object sender, EventArgs e)
		{
			base.OnFocused(sender, e);
			UpdateRotaryInteraction(true);
		}

		protected override void OnUnfocused(object sender, EventArgs e)
		{
			base.OnUnfocused(sender, e);
			UpdateRotaryInteraction(false);
		}

		protected virtual void UpdateRotaryInteraction(bool enable)
		{
			if (NativeView is IRotaryInteraction ri)
			{
				if (Specific.GetUseBezelInteraction(Application.Current))
				{
					if (enable)
					{
						ri.RotaryWidget?.Activate();
						System.Maui.Maui.RotaryFocusObject = Element;
					}
					else
					{
						ri.RotaryWidget?.Deactivate();
						if (System.Maui.Maui.RotaryFocusObject == Element)
							System.Maui.Maui.RotaryFocusObject = null;
					}
				}
			}
		}
	}
}

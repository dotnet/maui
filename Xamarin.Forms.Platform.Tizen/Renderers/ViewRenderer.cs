using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using ElmSharp;

namespace Xamarin.Forms.Platform.Tizen
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
	}
}
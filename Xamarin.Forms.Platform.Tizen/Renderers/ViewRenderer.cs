using System.ComponentModel;
using ElmSharp;
using System.Diagnostics;

namespace Xamarin.Forms.Platform.Tizen
{
	/// <summary>
	/// Base class for view renderers.
	/// </summary>
	public abstract class ViewRenderer<TView, TNativeView> : VisualElementRenderer<TView>
		where TView : View
		where TNativeView : EvasObject
	{
		GestureDetector _gestureDetector;

		/// <summary>
		/// Default constructor.
		/// </summary>
		protected ViewRenderer()
		{
		}

		protected override void OnElementChanged(ElementChangedEventArgs<TView> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement != null)
			{
				_gestureDetector.Clear();
				_gestureDetector = null;
			}

			if (e.NewElement != null)
			{
				_gestureDetector = new GestureDetector(this);
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
			{
				UpdateIsEnabled();
			}
		}

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

		protected void SetNativeControl(TNativeView control)
		{
			Debug.Assert(control != null);
			SetNativeView(control);
		}

		void UpdateIsEnabled()
		{
			_gestureDetector.IsEnabled = Element.IsEnabled;
		}
	}
}
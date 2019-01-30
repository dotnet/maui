#if __ANDROID_28__
using System;
using System.ComponentModel;
using Android.Content;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android.FastRenderers;
using Xamarin.Forms.Platform.Android.Material;
using AColor = Android.Graphics.Color;
using AProgressBar = Android.Widget.ProgressBar;
using AView = Android.Views.View;

[assembly: ExportRenderer(typeof(ActivityIndicator), typeof(MaterialActivityIndicatorRenderer), new[] { typeof(VisualRendererMarker.Material) })]

namespace Xamarin.Forms.Platform.Android.Material
{
	public class MaterialActivityIndicatorRenderer : FrameLayout,
		IVisualElementRenderer, IViewRenderer, ITabStop
	{
		int? _defaultLabelFor;

		bool _disposed;

		ActivityIndicator _element;
		AProgressBar _control;

		VisualElementTracker _visualElementTracker;
		VisualElementRenderer _visualElementRenderer;
		MotionEventHelper _motionEventHelper;

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;
		public event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;

		public MaterialActivityIndicatorRenderer(Context context)
			: base(context)
		{
			VisualElement.VerifyVisualFlagEnabled();

			_control = new AProgressBar(new ContextThemeWrapper(context, Resource.Style.XamarinFormsMaterialProgressBarCircular), null, Resource.Style.XamarinFormsMaterialProgressBarCircular);
			_control.Indeterminate = true;
			AddView(_control);

			_visualElementRenderer = new VisualElementRenderer(this);
			_motionEventHelper = new MotionEventHelper();
		}

		protected AProgressBar Control => _control;

		protected ActivityIndicator Element
		{
			get { return _element; }
			set
			{
				if (_element == value)
					return;

				var oldElement = _element;
				_element = value;

				OnElementChanged(new ElementChangedEventArgs<ActivityIndicator>(oldElement, _element));

				_element?.SendViewInitialized(this);

				_motionEventHelper.UpdateElement(_element);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;
			_disposed = true;

			if (disposing)
			{
				_visualElementTracker?.Dispose();
				_visualElementTracker = null;

				_visualElementRenderer?.Dispose();
				_visualElementRenderer = null;

				if (Element != null)
				{
					Element.PropertyChanged -= OnElementPropertyChanged;

					if (Platform.GetRenderer(Element) == this)
						Element.ClearValue(Platform.RendererProperty);
				}
			}

			base.Dispose(disposing);
		}

		protected virtual void OnElementChanged(ElementChangedEventArgs<ActivityIndicator> e)
		{
			ElementChanged?.Invoke(this, new VisualElementChangedEventArgs(e.OldElement, e.NewElement));

			if (e.OldElement != null)
			{
				e.OldElement.PropertyChanged -= OnElementPropertyChanged;
			}

			if (e.NewElement != null)
			{
				this.EnsureId();

				if (_visualElementTracker == null)
					_visualElementTracker = new VisualElementTracker(this);

				e.NewElement.PropertyChanged += OnElementPropertyChanged;

				UpdateColorsAndRuning();

				ElevationHelper.SetElevation(this, e.NewElement);
			}
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			ElementPropertyChanged?.Invoke(this, e);

			if (e.IsOneOf(ActivityIndicator.IsRunningProperty, ActivityIndicator.ColorProperty, VisualElement.BackgroundColorProperty))
				UpdateColorsAndRuning();
		}

		public override bool OnTouchEvent(MotionEvent e)
		{
			if (_visualElementRenderer.OnTouchEvent(e) || base.OnTouchEvent(e))
				return true;

			return _motionEventHelper.HandleMotionEvent(Parent, e);
		}

		void UpdateColorsAndRuning()
		{
			if (Element == null || Control == null)
				return;

			var background = Element.BackgroundColor.IsDefault 
				? AColor.Transparent 
				: Element.BackgroundColor.ToAndroid();
			(_control.Background as GradientDrawable)?.SetColor(background);

			if (Element.IsRunning)
			{
				var progress = Element.Color.IsDefault 
					? MaterialColors.Light.PrimaryColor 
					: Element.Color.ToAndroid();
				_control.IndeterminateTintList = ColorStateList.ValueOf(progress);
			}
			else
			{
				_control.Visibility = Element.BackgroundColor.IsDefault 
					? ViewStates.Gone 
					: ViewStates.Visible;
				_control.IndeterminateTintList = ColorStateList.ValueOf(AColor.Transparent);
			}
		}

		// IVisualElementRenderer

		VisualElement IVisualElementRenderer.Element => Element;

		VisualElementTracker IVisualElementRenderer.Tracker => _visualElementTracker;

		ViewGroup IVisualElementRenderer.ViewGroup => null;

		AView IVisualElementRenderer.View => this;

		SizeRequest IVisualElementRenderer.GetDesiredSize(int widthConstraint, int heightConstraint)
		{
			_control.Measure(widthConstraint, heightConstraint);
			return new SizeRequest(new Size(Control.MeasuredWidth, Control.MeasuredHeight), new Size());
		}

		void IVisualElementRenderer.SetElement(VisualElement element) =>
			Element = (element as ActivityIndicator) ??
				throw new ArgumentException($"{element?.GetType().FullName} is not compatible. {nameof(element)} must be of type {nameof(ActivityIndicator)}.");

		void IVisualElementRenderer.SetLabelFor(int? id)
		{
			if (_defaultLabelFor == null)
				_defaultLabelFor = ViewCompat.GetLabelFor(this);

			ViewCompat.SetLabelFor(this, (int)(id ?? _defaultLabelFor));
		}

		void IVisualElementRenderer.UpdateLayout() =>
			_visualElementTracker?.UpdateLayout();

		// IViewRenderer

		void IViewRenderer.MeasureExactly() =>
			ViewRenderer.MeasureExactly(_control, Element, Context);

		// ITabStop

		AView ITabStop.TabStop => _control;
	}
}
#endif
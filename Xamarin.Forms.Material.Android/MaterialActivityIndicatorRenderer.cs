
using System;
using System.ComponentModel;
using Android.Content;
#if __ANDROID_29__
using AndroidX.Core.View;
#else
using Android.Support.V4.View;
#endif
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android.FastRenderers;
using Xamarin.Forms.Material.Android;
using AProgressBar = Android.Widget.ProgressBar;
using AView = Android.Views.View;
using Xamarin.Forms.Platform.Android;


namespace Xamarin.Forms.Material.Android
{
	public class MaterialActivityIndicatorRenderer : FrameLayout,
		IVisualElementRenderer, IViewRenderer, ITabStop
	{
		int? _defaultLabelFor;
		bool _disposed;
		ActivityIndicator _element;
		CircularProgress _control;
		VisualElementTracker _visualElementTracker;
		VisualElementRenderer _visualElementRenderer;
		MotionEventHelper _motionEventHelper;

		public MaterialActivityIndicatorRenderer(Context context)
			: base(context)
		{
			_control = new CircularProgress(MaterialContextThemeWrapper.Create(context), null, Resource.Attribute.materialProgressBarCircularStyle)
			{
				// limiting size to compare iOS realization
				// https://github.com/material-components/material-components-ios/blob/develop/components/ActivityIndicator/src/MDCActivityIndicator.m#L425
				MinSize = (int)Context.ToPixels(10),
				MaxSize = (int)Context.ToPixels(144),
				DefaultColor = MaterialColors.Light.PrimaryColor
			};
			AddView(Control);

			_visualElementRenderer = new VisualElementRenderer(this);
			_motionEventHelper = new MotionEventHelper();
		}

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

		public event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;

		public override bool OnTouchEvent(MotionEvent e)
		{
			if (_visualElementRenderer.OnTouchEvent(e) || base.OnTouchEvent(e))
				return true;

			return _motionEventHelper.HandleMotionEvent(Parent, e);
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

					if (Platform.Android.Platform.GetRenderer(Element) == this)
						Element.ClearValue(Platform.Android.Platform.RendererProperty);
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

				UpdateColor();
				UpdateBackgroundColor();
				UpdateIsRunning();

				ElevationHelper.SetElevation(this, e.NewElement);
			}
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			ElementPropertyChanged?.Invoke(this, e);

			if (e.Is(ActivityIndicator.IsRunningProperty))
				UpdateIsRunning();
			else if (e.Is(ActivityIndicator.ColorProperty))
				UpdateColor();
			else if (e.Is(VisualElement.BackgroundColorProperty))
				UpdateBackgroundColor();
		}

		void UpdateIsRunning()
		{
			if (Element != null && _control != null)
				_control.IsRunning = Element.IsRunning;
		}

		void UpdateColor()
		{
			if (Element != null && _control != null)
				_control.SetColor(Element.Color);
		}

		void UpdateBackgroundColor()
		{
			if (Element != null && _control != null)
				_control.SetBackgroundColor(Element.BackgroundColor);
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
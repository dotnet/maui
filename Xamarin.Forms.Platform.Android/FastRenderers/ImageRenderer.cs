using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using AImageView = Android.Widget.ImageView;
using AView = Android.Views.View;
using Android.Views;
using Xamarin.Forms.Internals;
using Android.Support.V4.View;

namespace Xamarin.Forms.Platform.Android.FastRenderers
{
	internal sealed class ImageRenderer : AImageView, IVisualElementRenderer, IImageRendererController, IViewRenderer, ITabStop
	{
		bool _disposed;
		Image _element;
		bool _skipInvalidate;
		int? _defaultLabelFor;
		VisualElementTracker _visualElementTracker;
		VisualElementRenderer _visualElementRenderer;
		readonly MotionEventHelper _motionEventHelper = new MotionEventHelper();

		bool IImageRendererController.IsDisposed => _disposed;
		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				ImageElementManager.Dispose(this);
				BackgroundManager.Dispose(this);

				if (_visualElementTracker != null)
				{
					_visualElementTracker.Dispose();
					_visualElementTracker = null;
				}

				if (_visualElementRenderer != null)
				{
					_visualElementRenderer.Dispose();
					_visualElementRenderer = null;
				}

				if (_element != null)
				{
					_element.PropertyChanged -= OnElementPropertyChanged;

					if (Platform.GetRenderer(_element) == this)
						_element.ClearValue(Platform.RendererProperty);
				}
			}

			base.Dispose(disposing);
		}

		public override void Invalidate()
		{
			if (_skipInvalidate)
			{
				_skipInvalidate = false;
				return;
			}

			base.Invalidate();
		}

		void OnElementChanged(ElementChangedEventArgs<Image> e)
		{
			this.EnsureId();
			ElementChanged?.Invoke(this, new VisualElementChangedEventArgs(e.OldElement, e.NewElement));
		}

		public override bool OnTouchEvent(MotionEvent e)
		{
			if (_visualElementRenderer.OnTouchEvent(e) || base.OnTouchEvent(e))
			{
				return true;
			}

			return _motionEventHelper.HandleMotionEvent(Parent, e);
		}

		Size MinimumSize()
		{
			return new Size();
		}

		SizeRequest IVisualElementRenderer.GetDesiredSize(int widthConstraint, int heightConstraint)
		{
			if (_disposed)
			{
				return new SizeRequest();
			}

			Measure(widthConstraint, heightConstraint);
			return new SizeRequest(new Size(MeasuredWidth, MeasuredHeight), MinimumSize());
		}

		void IVisualElementRenderer.SetElement(VisualElement element)
		{
			if (element == null)
				throw new ArgumentNullException(nameof(element));

			var image = element as Image;
			if (image == null)
				throw new ArgumentException("Element is not of type " + typeof(Image), nameof(element));

			Image oldElement = _element;
			_element = image;

			Performance.Start(out string reference);

			if (oldElement != null)
				oldElement.PropertyChanged -= OnElementPropertyChanged;

			element.PropertyChanged += OnElementPropertyChanged;

			if (_visualElementTracker == null)
			{
				_visualElementTracker = new VisualElementTracker(this);
			}

			if (_visualElementRenderer == null)
			{
				_visualElementRenderer = new VisualElementRenderer(this);
				BackgroundManager.Init(this);
				ImageElementManager.Init(this);
			}

			Performance.Stop(reference);
			_motionEventHelper.UpdateElement(element);
			OnElementChanged(new ElementChangedEventArgs<Image>(oldElement, _element));

			_element?.SendViewInitialized(Control);
		}

		void IVisualElementRenderer.SetLabelFor(int? id)
		{
			if (_defaultLabelFor == null)
				_defaultLabelFor = ViewCompat.GetLabelFor(this);

			ViewCompat.SetLabelFor(this, (int)(id ?? _defaultLabelFor));
		}

		void IVisualElementRenderer.UpdateLayout() => _visualElementTracker?.UpdateLayout();

		void IViewRenderer.MeasureExactly()
		{
			ViewRenderer.MeasureExactly(this, ((IVisualElementRenderer)this).Element, Context);
		}

		VisualElement IVisualElementRenderer.Element => _element;

		VisualElementTracker IVisualElementRenderer.Tracker => _visualElementTracker;

		AView IVisualElementRenderer.View => this;

		AView ITabStop.TabStop => this;

		ViewGroup IVisualElementRenderer.ViewGroup => null;

		void IImageRendererController.SkipInvalidate() => _skipInvalidate = true;

		AImageView Control => this;

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;
		public event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;

		public ImageRenderer(Context context) : base(context)
		{
		}

		[Obsolete("This constructor is obsolete as of version 2.5. Please use ImageRenderer(Context) instead.")]
		public ImageRenderer() : base(Forms.Context)
		{
		}

		void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			ElementPropertyChanged?.Invoke(this, e);
		}
	}
}

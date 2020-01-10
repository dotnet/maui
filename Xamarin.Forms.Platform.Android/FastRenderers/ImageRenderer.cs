using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using AImageView = Android.Widget.ImageView;
using AView = Android.Views.View;
using Android.Views;
using Xamarin.Forms.Internals;
#if __ANDROID_29__
using AndroidX.Core.View;
#else
using Android.Support.V4.View;
#endif

namespace Xamarin.Forms.Platform.Android.FastRenderers
{
	public class ImageRenderer : AImageView, IVisualElementRenderer, IImageRendererController, IViewRenderer, ITabStop,
		ILayoutChanges
	{
		bool _disposed;
		Image _element;
		bool _skipInvalidate;
		int? _defaultLabelFor;
		VisualElementTracker _visualElementTracker;
		VisualElementRenderer _visualElementRenderer;
		readonly MotionEventHelper _motionEventHelper = new MotionEventHelper();
		IFormsAnimationDrawable _formsAnimationDrawable;

		bool IImageRendererController.IsDisposed => _disposed || !Control.IsAlive();
		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				if (_element != null)
				{
					_element.PropertyChanged -= OnElementPropertyChanged;
				}

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

		protected virtual void OnElementChanged(ElementChangedEventArgs<Image> e)
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
		void IImageRendererController.SetFormsAnimationDrawable(IFormsAnimationDrawable value)
		{
			if(_formsAnimationDrawable != null)
				_formsAnimationDrawable.AnimationStopped -= OnAnimationStopped;

			_formsAnimationDrawable = value;
			if (_formsAnimationDrawable != null)
				_formsAnimationDrawable.AnimationStopped += OnAnimationStopped;
		}

		void OnAnimationStopped(object sender, FormsAnimationDrawableStateEventArgs e) =>
			ImageElementManager.OnAnimationStopped(Element, e);

		protected AImageView Control => this;
		protected Image Element => _element;

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;
		public event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;

		public ImageRenderer(Context context) : base(context)
		{
		}

		[Obsolete("This constructor is obsolete as of version 2.5. Please use ImageRenderer(Context) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public ImageRenderer() : base(Forms.Context)
		{
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			ElementPropertyChanged?.Invoke(this, e);
		}
	}
}

using System;
using System.ComponentModel;
using Android.Content;
using Android.Graphics;
using Android.Views;
using AndroidX.Core.View;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using AImageView = Android.Widget.ImageView;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android.FastRenderers
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public class ImageRenderer : AImageView, IVisualElementRenderer, IImageRendererController, IViewRenderer, ITabStop,
		ILayoutChanges
	{
		bool _hasLayoutOccurred;
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

		protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
		{
			base.OnLayout(changed, left, top, right, bottom);
			_hasLayoutOccurred = true;
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

		public override void Draw(Canvas canvas)
		{
			canvas.ClipShape(Context, Element);

			base.Draw(canvas);
		}

		protected virtual void OnElementChanged(ElementChangedEventArgs<Image> e)
		{
			this.EnsureId();
			ElementChanged?.Invoke(this, new VisualElementChangedEventArgs(e.OldElement, e.NewElement));
		}

		public override bool OnTouchEvent(MotionEvent e)
		{
			if (base.OnTouchEvent(e))
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

		void IImageRendererController.SkipInvalidate() => _skipInvalidate = true;
		void IImageRendererController.SetFormsAnimationDrawable(IFormsAnimationDrawable value)
		{
			if (_formsAnimationDrawable != null)
				_formsAnimationDrawable.AnimationStopped -= OnAnimationStopped;

			_formsAnimationDrawable = value;
			if (_formsAnimationDrawable != null)
				_formsAnimationDrawable.AnimationStopped += OnAnimationStopped;
		}

		bool ILayoutChanges.HasLayoutOccurred => _hasLayoutOccurred;

		void OnAnimationStopped(object sender, FormsAnimationDrawableStateEventArgs e) =>
			ImageElementManager.OnAnimationStopped(Element, e);

		protected AImageView Control => this;
		protected Image Element => _element;

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;
		public event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;

		public ImageRenderer(Context context) : base(context)
		{
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (this.IsDisposed())
			{
				return;
			}

			ElementPropertyChanged?.Invoke(this, e);
		}
	}
}

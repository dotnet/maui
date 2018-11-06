using System;
using System.ComponentModel;
using Android.Content;
using Android.Support.V7.Widget;
using AView = Android.Views.View;
using Android.Views;
using Xamarin.Forms.Internals;
using AColor = Android.Graphics.Color;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using Android.Graphics.Drawables;
using Android.Graphics;
using Xamarin.Forms.Platform.Android.FastRenderers;
using Android.OS;

namespace Xamarin.Forms.Platform.Android
{
	public class ImageButtonRenderer :
		AppCompatImageButton,
		IVisualElementRenderer,
		IBorderVisualElementRenderer,
		IImageRendererController,
		AView.IOnFocusChangeListener,
		AView.IOnClickListener,
		AView.IOnTouchListener
	{
		bool _inputTransparent;
		bool _disposed;
		bool _skipInvalidate;
		int? _defaultLabelFor;
		VisualElementTracker _tracker;
		VisualElementRenderer _visualElementRenderer;
		BorderBackgroundManager _backgroundTracker;
		IPlatformElementConfiguration<PlatformConfiguration.Android, ImageButton> _platformElementConfiguration;
		private ImageButton _imageButton;

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;
		public event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;

		void IVisualElementRenderer.UpdateLayout() => _tracker?.UpdateLayout();
		VisualElement IVisualElementRenderer.Element => ImageButton;
		AView IVisualElementRenderer.View => this;
		ViewGroup IVisualElementRenderer.ViewGroup => null;
		VisualElementTracker IVisualElementRenderer.Tracker => _tracker;

		ImageButton ImageButton
		{
			get => _imageButton;
			set
			{
				_imageButton = value;
				_platformElementConfiguration = null;
			}
		}

		void IImageRendererController.SkipInvalidate() => _skipInvalidate = true;
		bool IImageRendererController.IsDisposed => _disposed;

		AppCompatImageButton Control => this;
		public ImageButtonRenderer(Context context) : base(context)
		{
			// These set the defaults so visually it matches up with other platforms
			SetPadding(0, 0, 0, 0);
			SoundEffectsEnabled = false;
			SetOnClickListener(this);
			SetOnTouchListener(this);
			OnFocusChangeListener = this;

			Tag = this;
			_backgroundTracker = new BorderBackgroundManager(this, false);
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{

				ImageElementManager.Dispose(this);

				_tracker?.Dispose();
				_tracker = null;

				_backgroundTracker?.Dispose();
				_backgroundTracker = null;

				if (ImageButton != null)
				{
					ImageButton.PropertyChanged -= OnElementPropertyChanged;

					if (Android.Platform.GetRenderer(ImageButton) == this)
					{
						ImageButton.ClearValue(Android.Platform.RendererProperty);
					}

					ImageButton = null;
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
			{
				throw new ArgumentNullException(nameof(element));
			}

			if (!(element is ImageButton image))
			{
				throw new ArgumentException("Element is not of type " + typeof(ImageButton), nameof(element));
			}

			ImageButton oldElement = ImageButton;
			ImageButton = image;

			Performance.Start(out string reference);

			if (oldElement != null)
			{
				oldElement.PropertyChanged -= OnElementPropertyChanged;
			}

			element.PropertyChanged += OnElementPropertyChanged;

			if (_tracker == null)
			{
				_tracker = new VisualElementTracker(this);
				ImageElementManager.Init(this);

			}

			if (_visualElementRenderer == null)
			{
				_visualElementRenderer = new VisualElementRenderer(this);
			}

			Performance.Stop(reference);
			this.EnsureId();

			UpdateInputTransparent();
			UpdatePadding();

			ElementChanged?.Invoke(this, new VisualElementChangedEventArgs(oldElement, ImageButton));
			ImageButton?.SendViewInitialized(Control);
		}

		public override void Draw(Canvas canvas)
		{
			if (ImageButton == null)
				return;

			var backgroundDrawable = _backgroundTracker?.BackgroundDrawable;

			RectF drawableBounds = null;

			if ((int)Build.VERSION.SdkInt >= 18 && backgroundDrawable != null)
			{
				var outlineBounds = backgroundDrawable.GetPaddingBounds(canvas.Width, canvas.Height);
				var width = (float)MeasuredWidth;
				var height = (float)MeasuredHeight;

				var widthRatio = 1f;
				var heightRatio = 1f;

				if (ImageButton.Aspect == Aspect.AspectFill && OnThisPlatform().GetIsShadowEnabled())
					Internals.Log.Warning(nameof(ImageButtonRenderer), "AspectFill isn't fully supported when using shadows. Image may be clipped incorrectly to Border");

				switch (ImageButton.Aspect)
				{
					case Aspect.Fill:
						break;
					case Aspect.AspectFill:
					case Aspect.AspectFit:
						heightRatio = (float)Drawable.IntrinsicHeight / height;
						widthRatio = (float)Drawable.IntrinsicWidth / width;
						break;
				}

				drawableBounds = new RectF(outlineBounds.Left * widthRatio, outlineBounds.Top * heightRatio, outlineBounds.Right * widthRatio, outlineBounds.Bottom * heightRatio);
			}

			if (drawableBounds != null)
				Drawable.SetBounds((int)drawableBounds.Left, (int)drawableBounds.Top, (int)drawableBounds.Right, (int)drawableBounds.Bottom);



			base.Draw(canvas);
			if (_backgroundTracker.BackgroundDrawable != null)
				_backgroundTracker.BackgroundDrawable.DrawOutline(canvas, canvas.Width, canvas.Height);
		}

		void IVisualElementRenderer.SetLabelFor(int? id)
		{
			if (_defaultLabelFor == null)
				_defaultLabelFor = LabelFor;

			LabelFor = (int)(id ?? _defaultLabelFor);
		}

		public override bool OnTouchEvent(MotionEvent e)
		{
			if (!Enabled || (_inputTransparent && Enabled))
				return false;

			return base.OnTouchEvent(e);
		}


		void UpdatePadding()
		{
			SetPadding(
				(int)(Context.ToPixels(ImageButton.Padding.Left)),
				(int)(Context.ToPixels(ImageButton.Padding.Top)),
				(int)(Context.ToPixels(ImageButton.Padding.Right)),
				(int)(Context.ToPixels(ImageButton.Padding.Bottom))
			);
		}

		void UpdateInputTransparent()
		{
			if (ImageButton == null || _disposed)
			{
				return;
			}

			_inputTransparent = ImageButton.InputTransparent;
		}

		// Image related
		void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == VisualElement.InputTransparentProperty.PropertyName)
				UpdateInputTransparent();
			else if (e.PropertyName == ImageButton.PaddingProperty.PropertyName)
				UpdatePadding();

			ElementPropertyChanged?.Invoke(this, e);
		}


		// general state related
		void IOnFocusChangeListener.OnFocusChange(AView v, bool hasFocus)
		{
			((IElementController)ImageButton).SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, hasFocus);
		}
		// general state related


		// Button related
		void IOnClickListener.OnClick(AView v) =>
			ButtonElementManager.OnClick(ImageButton, ImageButton, v);

		bool IOnTouchListener.OnTouch(AView v, MotionEvent e) =>
			ButtonElementManager.OnTouch(ImageButton, ImageButton, v, e);
		// Button related


		float IBorderVisualElementRenderer.ShadowRadius => Context.ToPixels(OnThisPlatform().GetShadowRadius());
		float IBorderVisualElementRenderer.ShadowDx => Context.ToPixels(OnThisPlatform().GetShadowOffset().Width);
		float IBorderVisualElementRenderer.ShadowDy => Context.ToPixels(OnThisPlatform().GetShadowOffset().Height);
		AColor IBorderVisualElementRenderer.ShadowColor => OnThisPlatform().GetShadowColor().ToAndroid();
		bool IBorderVisualElementRenderer.IsShadowEnabled() => OnThisPlatform().GetIsShadowEnabled();
		bool IBorderVisualElementRenderer.UseDefaultPadding() => false;
		bool IBorderVisualElementRenderer.UseDefaultShadow() => false;
		VisualElement IBorderVisualElementRenderer.Element => ImageButton;
		AView IBorderVisualElementRenderer.View => this;

		IPlatformElementConfiguration<PlatformConfiguration.Android, ImageButton> OnThisPlatform()
		{
			if (_platformElementConfiguration == null)
				_platformElementConfiguration = ImageButton.OnThisPlatform();

			return _platformElementConfiguration;
		}
	}
}

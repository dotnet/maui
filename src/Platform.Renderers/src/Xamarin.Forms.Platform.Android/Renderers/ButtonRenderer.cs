using System;
using System.ComponentModel;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using static System.String;
using AButton = Android.Widget.Button;
using AColor = Android.Graphics.Color;
using AMotionEvent = Android.Views.MotionEvent;
using AMotionEventActions = Android.Views.MotionEventActions;
using AView = Android.Views.View;
using Object = Java.Lang.Object;

namespace Xamarin.Forms.Platform.Android
{
	public class ButtonRenderer : ViewRenderer<Button, AButton>, AView.IOnAttachStateChangeListener, IBorderVisualElementRenderer
	{
		BorderBackgroundManager _backgroundTracker;
		TextColorSwitcher _textColorSwitcher;
		float _defaultFontSize;
		Typeface _defaultTypeface;
		bool _isDisposed;
		int _imageHeight = -1;
		Thickness _paddingDeltaPix = new Thickness();
		IVisualElementRenderer _visualElementRenderer;

		public ButtonRenderer(Context context) : base(context)
		{
			AutoPackage = false;
			_visualElementRenderer = this;
			_backgroundTracker = new BorderBackgroundManager(this);
		}

		[Obsolete("This constructor is obsolete as of version 2.5. Please use ButtonRenderer(Context) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public ButtonRenderer()
		{
			AutoPackage = false;
			_visualElementRenderer = this;
			_backgroundTracker = new BorderBackgroundManager(this);

		}

		AButton NativeButton
		{
			get { return Control; }
		}

		public void OnViewAttachedToWindow(AView attachedView)
		{
			UpdateText();
		}

		public void OnViewDetachedFromWindow(AView detachedView)
		{
		}

		public override SizeRequest GetDesiredSize(int widthConstraint, int heightConstraint)
		{
			UpdateText();
			return base.GetDesiredSize(widthConstraint, heightConstraint);
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			if (_imageHeight > -1)
			{
				// We've got an image (and no text); it's already centered horizontally,
				// we just need to adjust the padding so it centers vertically
				var diff = ((b - Context.ToPixels(Element.Padding.Bottom + Element.Padding.Top)) - t - _imageHeight) / 2;
				diff = Math.Max(diff, 0);
				UpdateContentEdge(new Thickness(0, diff, 0, -diff));
			}
			else
			{
				UpdateContentEdge();
			}

			base.OnLayout(changed, l, t, r, b);
		}

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			_isDisposed = true;

			if (disposing)
			{
				_backgroundTracker?.Dispose();
				_backgroundTracker = null;
				_visualElementRenderer = null;
			}

			base.Dispose(disposing);
		}

		protected override AButton CreateNativeControl()
		{
			return new AButton(Context);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement == null)
			{
				AButton button = Control;
				if (button == null)
				{
					button = CreateNativeControl();
					button.SetOnClickListener(ButtonClickListener.Instance.Value);
					button.SetOnTouchListener(ButtonTouchListener.Instance.Value);
					button.Tag = this;
					SetNativeControl(button);

					var useLegacyColorManagement = e.NewElement.UseLegacyColorManagement();
					_textColorSwitcher = new TextColorSwitcher(button.TextColors, useLegacyColorManagement);

					button.AddOnAttachStateChangeListener(this);
				}
			}

			UpdateAll();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Button.TextProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == Button.TextColorProperty.PropertyName)
				UpdateTextColor();
			else if (e.PropertyName == Button.CharacterSpacingProperty.PropertyName)
				UpdateCharacterSpacing();
			else if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
				UpdateEnabled();
			else if (e.PropertyName == Button.FontProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Button.ImageSourceProperty.PropertyName)
				UpdateBitmap();
			else if (e.PropertyName == VisualElement.IsVisibleProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == Button.PaddingProperty.PropertyName)
				UpdatePadding();

			base.OnElementPropertyChanged(sender, e);
		}

		protected override void UpdateBackgroundColor()
		{
			if (Element == null || Control == null)
				return;

			_backgroundTracker?.UpdateDrawable();
		}

		protected override void UpdateBackground()
		{
			if (Element == null || Control == null)
				return;

			_backgroundTracker?.UpdateDrawable();
		}

		void UpdateAll()
		{
			UpdateFont();
			UpdateText();
			UpdateBitmap();
			UpdateTextColor();
			UpdateCharacterSpacing();
			UpdateEnabled();
			UpdateBackgroundColor();
			UpdatePadding();
		}

		void UpdateBitmap()
		{
			var elementImage = Element.ImageSource;
			_imageHeight = -1;

			if (elementImage == null || elementImage.IsEmpty)
			{
				Control.SetCompoundDrawablesWithIntrinsicBounds(null, null, null, null);
				return;
			}

			if (IsNullOrEmpty(Element.Text))
			{
				// No text, so no need for relative position; just center the image
				// There's no option for just plain-old centering, so we'll use Top 
				// (which handles the horizontal centering) and some tricksy padding (in OnLayout)
				// to handle the vertical centering 

				this.ApplyDrawableAsync(Button.ImageSourceProperty, Context, image =>
				{
					// Clear any previous padding and set the image as top/center
					UpdateContentEdge();
					Control.SetCompoundDrawablesWithIntrinsicBounds(null, image, null, null);

					// Keep track of the image height so we can use it in OnLayout
					_imageHeight = image?.IntrinsicHeight ?? -1;

					Element?.InvalidateMeasureNonVirtual(InvalidationTrigger.MeasureChanged);
				});
				return;
			}

			this.ApplyDrawableAsync(Button.ImageSourceProperty, Context, image =>
			{
				var layout = Element.ContentLayout;

				Control.CompoundDrawablePadding = (int)layout.Spacing;

				switch (layout.Position)
				{
					case Button.ButtonContentLayout.ImagePosition.Top:
						Control.SetCompoundDrawablesWithIntrinsicBounds(null, image, null, null);
						break;
					case Button.ButtonContentLayout.ImagePosition.Bottom:
						Control.SetCompoundDrawablesWithIntrinsicBounds(null, null, null, image);
						break;
					case Button.ButtonContentLayout.ImagePosition.Right:
						Control.SetCompoundDrawablesWithIntrinsicBounds(null, null, image, null);
						break;
					default:
						// Defaults to image on the left
						Control.SetCompoundDrawablesWithIntrinsicBounds(image, null, null, null);
						break;
				}

				Element?.InvalidateMeasureNonVirtual(InvalidationTrigger.MeasureChanged);
			});
		}

		void UpdateEnabled()
		{
			Control.Enabled = Element.IsEnabled;
		}

		void UpdateFont()
		{
			Button button = Element;
			if (button.Font == Font.Default && _defaultFontSize == 0f)
				return;

			if (_defaultFontSize == 0f)
			{
				_defaultTypeface = NativeButton.Typeface;
				_defaultFontSize = NativeButton.TextSize;
			}

			if (button.Font == Font.Default)
			{
				NativeButton.Typeface = _defaultTypeface;
				NativeButton.SetTextSize(ComplexUnitType.Px, _defaultFontSize);
			}
			else
			{
				NativeButton.Typeface = button.Font.ToTypeface();
				NativeButton.SetTextSize(ComplexUnitType.Sp, button.Font.ToScaledPixel());
			}
		}

		void UpdateText()
		{
			var oldText = NativeButton.Text;
			NativeButton.Text = Element.Text;

			// If we went from or to having no text, we need to update the image position
			if (IsNullOrEmpty(oldText) != IsNullOrEmpty(NativeButton.Text))
			{
				UpdateBitmap();
			}
		}

		void UpdateTextColor()
		{
			_textColorSwitcher?.UpdateTextColor(Control, Element.TextColor);
		}

		void UpdateCharacterSpacing()
		{
			if (Forms.IsLollipopOrNewer)
			{
				Control.LetterSpacing = Element.CharacterSpacing.ToEm();
			}
		}

		float IBorderVisualElementRenderer.ShadowRadius => Control.ShadowRadius;
		float IBorderVisualElementRenderer.ShadowDx => Control.ShadowDx;
		float IBorderVisualElementRenderer.ShadowDy => Control.ShadowDy;
		AColor IBorderVisualElementRenderer.ShadowColor => Control.ShadowColor;
		bool IBorderVisualElementRenderer.UseDefaultPadding() => Element.OnThisPlatform().UseDefaultPadding();
		bool IBorderVisualElementRenderer.UseDefaultShadow() => Element.OnThisPlatform().UseDefaultShadow();
		bool IBorderVisualElementRenderer.IsShadowEnabled() => true;
		VisualElement IBorderVisualElementRenderer.Element => Element;
		AView IBorderVisualElementRenderer.View => Control;
		event EventHandler<VisualElementChangedEventArgs> IBorderVisualElementRenderer.ElementChanged
		{
			add => _visualElementRenderer.ElementChanged += value;
			remove => _visualElementRenderer.ElementChanged -= value;
		}

		void UpdatePadding()
		{
			Control?.SetPadding(
				(int)(Context.ToPixels(Element.Padding.Left) + _paddingDeltaPix.Left),
				(int)(Context.ToPixels(Element.Padding.Top) + _paddingDeltaPix.Top),
				(int)(Context.ToPixels(Element.Padding.Right) + _paddingDeltaPix.Right),
				(int)(Context.ToPixels(Element.Padding.Bottom) + _paddingDeltaPix.Bottom)
			);
		}

		void UpdateContentEdge(Thickness? delta = null)
		{
			_paddingDeltaPix = delta ?? new Thickness();
			UpdatePadding();
		}

		class ButtonClickListener : Object, IOnClickListener
		{
			public static readonly Lazy<ButtonClickListener> Instance = new Lazy<ButtonClickListener>(() => new ButtonClickListener());

			public void OnClick(AView v)
			{
				var renderer = v.Tag as ButtonRenderer;
				if (renderer != null)
					((IButtonController)renderer.Element).SendClicked();
			}
		}

		class ButtonTouchListener : Object, IOnTouchListener
		{
			public static readonly Lazy<ButtonTouchListener> Instance = new Lazy<ButtonTouchListener>(() => new ButtonTouchListener());

			public bool OnTouch(AView v, AMotionEvent e)
			{
				var renderer = v.Tag as ButtonRenderer;
				if (renderer != null)
				{
					var buttonController = renderer.Element as IButtonController;
					if (e.Action == AMotionEventActions.Down)
					{
						buttonController?.SendPressed();
					}
					else if (e.Action == AMotionEventActions.Up)
					{
						buttonController?.SendReleased();
					}
				}
				return false;
			}
		}
	}
}
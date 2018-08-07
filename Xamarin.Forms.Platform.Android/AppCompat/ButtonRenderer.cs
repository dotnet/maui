using System;
using System.ComponentModel;
using Android.Content;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Util;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using GlobalResource = Android.Resource;
using Object = Java.Lang.Object;
using AView = Android.Views.View;
using AMotionEvent = Android.Views.MotionEvent;
using AMotionEventActions = Android.Views.MotionEventActions;
using static System.String;

namespace Xamarin.Forms.Platform.Android.AppCompat
{
	public class ButtonRenderer : ViewRenderer<Button, AppCompatButton>, AView.IOnAttachStateChangeListener
	{
		ButtonBackgroundTracker _backgroundTracker;
		TextColorSwitcher _textColorSwitcher;
		float _defaultFontSize;
		Thickness? _defaultPadding;
		Typeface _defaultTypeface;
		bool _isDisposed;
		int _imageHeight = -1;
		Thickness _paddingDeltaPix = new Thickness();

		public ButtonRenderer(Context context) : base(context)
		{
			AutoPackage = false;
		}

		[Obsolete("This constructor is obsolete as of version 2.5. Please use ButtonRenderer(Context) instead.")]
		public ButtonRenderer()
		{
			AutoPackage = false;
		}

		global::Android.Widget.Button NativeButton => Control;

		void AView.IOnAttachStateChangeListener.OnViewAttachedToWindow(AView attachedView)
		{
			UpdateText();
		}

		void AView.IOnAttachStateChangeListener.OnViewDetachedFromWindow(AView detachedView)
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

		protected override AppCompatButton CreateNativeControl()
		{
			return new AppCompatButton(Context);
		}

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			_isDisposed = true;

			if (disposing)
			{
				if (Control != null)
				{
					Control.SetOnClickListener(null);
					Control.SetOnTouchListener(null);
					Control.RemoveOnAttachStateChangeListener(this);
					Control.Tag = null;
					_textColorSwitcher = null;
				}
				_backgroundTracker?.Dispose();
				_backgroundTracker = null;
			}

			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement != null)
			{
			}

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					AppCompatButton button = CreateNativeControl();

					button.SetOnClickListener(ButtonClickListener.Instance.Value);
					button.SetOnTouchListener(ButtonTouchListener.Instance.Value);
					button.Tag = this;

					var useLegacyColorManagement = e.NewElement.UseLegacyColorManagement();
					_textColorSwitcher = new TextColorSwitcher(button.TextColors, useLegacyColorManagement);

					SetNativeControl(button);
					button.AddOnAttachStateChangeListener(this);
				}

				if (_backgroundTracker == null)
					_backgroundTracker = new ButtonBackgroundTracker(Element, Control);
				else
					_backgroundTracker.Button = e.NewElement;

				_defaultFontSize = 0f;
				_defaultPadding = null;

				UpdateAll();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Button.TextProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == Button.TextColorProperty.PropertyName)
				UpdateTextColor();
			else if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
				UpdateEnabled();
			else if (e.PropertyName == Button.FontProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Button.ImageProperty.PropertyName)
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

		void UpdateAll()
		{
			UpdateFont();
			UpdateText();
			UpdateBitmap();
			UpdateTextColor();
			UpdateEnabled();
			UpdateBackgroundColor();
			UpdatePadding();
		}

		void UpdateBitmap()
		{
			var elementImage = Element.Image;
			var imageFile = elementImage?.File;
			_imageHeight = -1;

			if (elementImage == null || string.IsNullOrEmpty(imageFile))
			{
				Control.SetCompoundDrawablesWithIntrinsicBounds(null, null, null, null);
				return;
			}

			var image = Context.GetDrawable(imageFile);

			if (IsNullOrEmpty(Element.Text))
			{
				// No text, so no need for relative position; just center the image
				// There's no option for just plain-old centering, so we'll use Top 
				// (which handles the horizontal centering) and some tricksy padding (in OnLayout)
				// to handle the vertical centering 

				// Clear any previous padding and set the image as top/center
				UpdateContentEdge();
				Control.SetCompoundDrawablesWithIntrinsicBounds(null, image, null, null);

				// Keep track of the image height so we can use it in OnLayout
				_imageHeight = image.IntrinsicHeight;

				image?.Dispose();
				return;
			}

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

			image?.Dispose();
		}

		void UpdateEnabled()
		{
			Control.Enabled = Element.IsEnabled;
		}

		void UpdateFont()
		{
			Button button = Element;
			Font font = button.Font;

			if (font == Font.Default && _defaultFontSize == 0f)
				return;

			if (_defaultFontSize == 0f)
			{
				_defaultTypeface = NativeButton.Typeface;
				_defaultFontSize = NativeButton.TextSize;
			}

			if (font == Font.Default)
			{
				NativeButton.Typeface = _defaultTypeface;
				NativeButton.SetTextSize(ComplexUnitType.Px, _defaultFontSize);
			}
			else
			{
				NativeButton.Typeface = font.ToTypeface();
				NativeButton.SetTextSize(ComplexUnitType.Sp, font.ToScaledPixel());
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

		void UpdatePadding()
		{
			if (Control == null)
				return;

			if (!_defaultPadding.HasValue)
				_defaultPadding = new Thickness(Control.PaddingLeft, Control.PaddingTop, Control.PaddingRight, Control.PaddingBottom);

			if (Element.IsSet(Button.PaddingProperty))
			{
				Control.SetPadding(
					(int)(Context.ToPixels(Element.Padding.Left) + _paddingDeltaPix.Left),
					(int)(Context.ToPixels(Element.Padding.Top) + _paddingDeltaPix.Top),
					(int)(Context.ToPixels(Element.Padding.Right) + _paddingDeltaPix.Right),
					(int)(Context.ToPixels(Element.Padding.Bottom) + _paddingDeltaPix.Bottom)
				);
			}
			else
			{
				Control.SetPadding(
						(int)_defaultPadding.Value.Left,
						(int)_defaultPadding.Value.Top,
						(int)_defaultPadding.Value.Right,
						(int)_defaultPadding.Value.Bottom
					);
			}
		}

		void UpdateContentEdge(Thickness? delta = null)
		{
			_paddingDeltaPix = delta ?? new Thickness();
			UpdatePadding();
		}

		class ButtonClickListener : Object, AView.IOnClickListener
		{
			#region Statics

			public static readonly Lazy<ButtonClickListener> Instance = new Lazy<ButtonClickListener>(() => new ButtonClickListener());

			#endregion

			public void OnClick(AView v)
			{
				var renderer = v.Tag as ButtonRenderer;
				((IButtonController)renderer?.Element)?.SendClicked();
			}
		}

		class ButtonTouchListener : Object, AView.IOnTouchListener
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

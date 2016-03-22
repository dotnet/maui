using System;
using System.ComponentModel;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using AButton = Android.Widget.Button;
using AView = Android.Views.View;
using Object = Java.Lang.Object;

namespace Xamarin.Forms.Platform.Android
{
	public class ButtonRenderer : ViewRenderer<Button, AButton>, AView.IOnAttachStateChangeListener
	{
		ButtonDrawable _backgroundDrawable;
		ColorStateList _buttonDefaulTextColors;
		Drawable _defaultDrawable;
		float _defaultFontSize;
		Typeface _defaultTypeface;
		bool _drawableEnabled;

		bool _isDisposed;

		public ButtonRenderer()
		{
			AutoPackage = false;
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

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			_isDisposed = true;

			if (disposing)
			{
				if (_backgroundDrawable != null)
				{
					_backgroundDrawable.Dispose();
					_backgroundDrawable = null;
				}
			}

			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement == null)
			{
				AButton button = Control;
				if (button == null)
				{
					button = new AButton(Context);
					button.SetOnClickListener(ButtonClickListener.Instance.Value);
					button.Tag = this;
					SetNativeControl(button);

					button.AddOnAttachStateChangeListener(this);
				}
			}
			else
			{
				if (_drawableEnabled)
				{
					_drawableEnabled = false;
					_backgroundDrawable.Reset();
					_backgroundDrawable = null;
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
			else if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
				UpdateEnabled();
			else if (e.PropertyName == Button.FontProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdateDrawable();
			else if (e.PropertyName == Button.ImageProperty.PropertyName)
				UpdateBitmap();
			else if (e.PropertyName == VisualElement.IsVisibleProperty.PropertyName)
				UpdateText();

			if (_drawableEnabled &&
				(e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName || e.PropertyName == Button.BorderColorProperty.PropertyName || e.PropertyName == Button.BorderRadiusProperty.PropertyName ||
				 e.PropertyName == Button.BorderWidthProperty.PropertyName))
			{
				_backgroundDrawable.Reset();
				Control.Invalidate();
			}

			base.OnElementPropertyChanged(sender, e);
		}

		protected override void UpdateBackgroundColor()
		{
			// Do nothing, the drawable handles this now
		}

		void UpdateAll()
		{
			UpdateFont();
			UpdateText();
			UpdateBitmap();
			UpdateTextColor();
			UpdateEnabled();
			UpdateDrawable();
		}

		async void UpdateBitmap()
		{
			if (Element.Image != null && !string.IsNullOrEmpty(Element.Image.File))
			{
				Drawable image = Context.Resources.GetDrawable(Element.Image.File);
				Control.SetCompoundDrawablesWithIntrinsicBounds(image, null, null, null);
				if (image != null)
					image.Dispose();
			}
			else
				Control.SetCompoundDrawablesWithIntrinsicBounds(null, null, null, null);
		}

		void UpdateDrawable()
		{
			if (Element.BackgroundColor == Color.Default)
			{
				if (!_drawableEnabled)
					return;

				if (_defaultDrawable != null)
					Control.SetBackgroundDrawable(_defaultDrawable);

				_drawableEnabled = false;
			}
			else
			{
				if (_backgroundDrawable == null)
					_backgroundDrawable = new ButtonDrawable();

				_backgroundDrawable.Button = Element;

				if (_drawableEnabled)
					return;

				if (_defaultDrawable == null)
					_defaultDrawable = Control.Background;

				Control.SetBackgroundDrawable(_backgroundDrawable);
				_drawableEnabled = true;
			}

			Control.Invalidate();
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
			NativeButton.Text = Element.Text;
		}

		void UpdateTextColor()
		{
			Color color = Element.TextColor;

			if (color.IsDefault)
			{
				if (_buttonDefaulTextColors == null)
					return;

				NativeButton.SetTextColor(_buttonDefaulTextColors);
			}
			else
			{
				_buttonDefaulTextColors = _buttonDefaulTextColors ?? Control.TextColors;

				// Set the new enabled state color, preserving the default disabled state color
				NativeButton.SetTextColor(color.ToAndroidPreserveDisabled(_buttonDefaulTextColors));
			}
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
	}
}
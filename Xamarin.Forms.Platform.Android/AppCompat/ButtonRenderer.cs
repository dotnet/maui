using System;
using System.ComponentModel;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.V7.Widget;
using Android.Util;
using GlobalResource = Android.Resource;
using Object = Java.Lang.Object;

namespace Xamarin.Forms.Platform.Android.AppCompat
{
	public class ButtonRenderer : ViewRenderer<Button, AppCompatButton>, global::Android.Views.View.IOnAttachStateChangeListener
	{
		static readonly int[][] States = { new[] { GlobalResource.Attribute.StateEnabled }, new[] { -GlobalResource.Attribute.StateEnabled } };

		ColorStateList _buttonDefaulTextColors;
		Color _currentTextColor;
		float _defaultFontSize;
		Typeface _defaultTypeface;
		bool _isDisposed;

		public ButtonRenderer()
		{
			AutoPackage = false;
		}

		global::Android.Widget.Button NativeButton => Control;

		void IOnAttachStateChangeListener.OnViewAttachedToWindow(global::Android.Views.View attachedView)
		{
			UpdateText();
		}

		void IOnAttachStateChangeListener.OnViewDetachedFromWindow(global::Android.Views.View detachedView)
		{
		}

		public override SizeRequest GetDesiredSize(int widthConstraint, int heightConstraint)
		{
			UpdateText();
			return base.GetDesiredSize(widthConstraint, heightConstraint);
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
					button.Tag = this;
					_buttonDefaulTextColors = button.TextColors;
					SetNativeControl(button);

					button.AddOnAttachStateChangeListener(this);
				}

				UpdateAll();
				UpdateBackgroundColor();
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

			base.OnElementPropertyChanged(sender, e);
		}

		protected override void UpdateBackgroundColor()
		{
			if (Element == null || Control == null)
				return;

			Color backgroundColor = Element.BackgroundColor;
			if (backgroundColor.IsDefault)
			{
				if (Control.SupportBackgroundTintList != null)
				{
					Context context = Context;
					int id = GlobalResource.Attribute.ButtonTint;
					unchecked
					{
						using(var value = new TypedValue())
						{
							try
							{
								Resources.Theme theme = context.Theme;
								if (theme != null && theme.ResolveAttribute(id, value, true))
									Control.SupportBackgroundTintList = Resources.GetColorStateList(value.Data);
								else
									Control.SupportBackgroundTintList = new ColorStateList(States, new[] { (int)0xffd7d6d6, 0x7fd7d6d6 });
							}
							catch (Exception ex)
							{
								Control.SupportBackgroundTintList = new ColorStateList(States, new[] { (int)0xffd7d6d6, 0x7fd7d6d6 });
							}
						}
					}
				}
			}
			else
			{
				int intColor = backgroundColor.ToAndroid().ToArgb();
				int disableColor = backgroundColor.MultiplyAlpha(0.5).ToAndroid().ToArgb();
				Control.SupportBackgroundTintList = new ColorStateList(States, new[] { intColor, disableColor });
			}
		}

		void UpdateAll()
		{
			UpdateFont();
			UpdateText();
			UpdateBitmap();
			UpdateTextColor();
			UpdateEnabled();
		}

		void UpdateBitmap()
		{
			FileImageSource elementImage = Element.Image;
			string imageFile = elementImage?.File;
			if (elementImage != null && !string.IsNullOrEmpty(imageFile))
			{
				Drawable image = Context.Resources.GetDrawable(imageFile);
				Control.SetCompoundDrawablesWithIntrinsicBounds(image, null, null, null);
				image?.Dispose();
			}
			else
				Control.SetCompoundDrawablesWithIntrinsicBounds(null, null, null, null);
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
			NativeButton.Text = Element.Text;
		}

		void UpdateTextColor()
		{
			Color color = Element.TextColor;
			if (color == _currentTextColor)
				return;

			_currentTextColor = color;

			if (color.IsDefault)
				NativeButton.SetTextColor(_buttonDefaulTextColors);
			else
			{
				// Set the new enabled state color, preserving the default disabled state color
				int defaultDisabledColor = _buttonDefaulTextColors.GetColorForState(States[1], color.ToAndroid());

				NativeButton.SetTextColor(new ColorStateList(States, new[] { color.ToAndroid().ToArgb(), defaultDisabledColor }));
			}
		}

		class ButtonClickListener : Object, IOnClickListener
		{
			#region Statics

			public static readonly Lazy<ButtonClickListener> Instance = new Lazy<ButtonClickListener>(() => new ButtonClickListener());

			#endregion

			public void OnClick(global::Android.Views.View v)
			{
				var renderer = v.Tag as ButtonRenderer;
				((IButtonController)renderer?.Element)?.SendClicked();
			}
		}
	}
}
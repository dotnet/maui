using System;
using System.ComponentModel;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.OS;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using Specifics = Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using AButton = Android.Widget.Button;
using AColor = Android.Graphics.Color;

namespace Xamarin.Forms.Platform.Android
{
	internal class ButtonBackgroundTracker : IDisposable
	{
		Drawable _defaultDrawable;
		ButtonDrawable _backgroundDrawable;
		RippleDrawable _rippleDrawable;
		Button _button;
		AButton _nativeButton;
		bool _drawableEnabled;
		bool _disposed;

		public ButtonBackgroundTracker(Button button, AButton nativeButton)
		{
			Button = button;
			_nativeButton = nativeButton;
		}

		public Button Button
		{
			get { return _button; }
			set
			{
				if (_button == value)
					return;
				if (_button != null)
					_button.PropertyChanged -= ButtonPropertyChanged;
				_button = value;
				_button.PropertyChanged += ButtonPropertyChanged;
			}
		}

		public void UpdateDrawable()
		{
			if (_button == null || _nativeButton == null)
				return;

			bool cornerRadiusIsDefault = !_button.IsSet(Button.CornerRadiusProperty) || (_button.CornerRadius == (int)Button.CornerRadiusProperty.DefaultValue || _button.CornerRadius == ButtonDrawable.DefaultCornerRadius);
			bool backgroundColorIsDefault = !_button.IsSet(VisualElement.BackgroundColorProperty) || _button.BackgroundColor == (Color)VisualElement.BackgroundColorProperty.DefaultValue;
			bool borderColorIsDefault = !_button.IsSet(Button.BorderColorProperty) || _button.BorderColor == (Color)Button.BorderColorProperty.DefaultValue;
			bool borderWidthIsDefault = !_button.IsSet(Button.BorderWidthProperty) || _button.BorderWidth == (double)Button.BorderWidthProperty.DefaultValue;

			if (backgroundColorIsDefault
				&& cornerRadiusIsDefault
				&& borderColorIsDefault
				&& borderWidthIsDefault)
			{
				if (!_drawableEnabled)
					return;

				if (_defaultDrawable != null)
					_nativeButton.SetBackground(_defaultDrawable);

				_drawableEnabled = false;
			}
			else
			{
				if (_backgroundDrawable == null)
					_backgroundDrawable = new ButtonDrawable(_nativeButton.Context.ToPixels, Forms.GetColorButtonNormal(_nativeButton.Context));

				_backgroundDrawable.Button = _button;

				var useDefaultPadding = _button.OnThisPlatform().UseDefaultPadding();

				int paddingTop = useDefaultPadding ? _nativeButton.PaddingTop : 0;
				int paddingLeft = useDefaultPadding ? _nativeButton.PaddingLeft : 0;

				var useDefaultShadow = _button.OnThisPlatform().UseDefaultShadow();

				// Use no shadow by default for API < 16
				float shadowRadius = 0;
				float shadowDy = 0;
				float shadowDx = 0;
				AColor shadowColor = Color.Transparent.ToAndroid();
				// Add Android's default material shadow if we want it
				if (useDefaultShadow)
				{
					shadowRadius = 2;
					shadowDy = 4;
					shadowDx = 0;
					shadowColor = _backgroundDrawable.PressedBackgroundColor.ToAndroid();
				}
				// Otherwise get values from the control (but only for supported APIs)
				else if ((int)Build.VERSION.SdkInt >= 16)
				{
					shadowRadius = _nativeButton.ShadowRadius;
					shadowDy = _nativeButton.ShadowDy;
					shadowDx = _nativeButton.ShadowDx;
					shadowColor = _nativeButton.ShadowColor;
				}

				_backgroundDrawable.SetPadding(paddingTop, paddingLeft)
								   .SetShadow(shadowDy, shadowDx, shadowColor, shadowRadius);

				if (_drawableEnabled)
					return;

				if (_defaultDrawable == null)
					_defaultDrawable = _nativeButton.Background;

				if (Forms.IsLollipopOrNewer)
				{
					var rippleColor = _backgroundDrawable.PressedBackgroundColor.ToAndroid();

					_rippleDrawable = new RippleDrawable(ColorStateList.ValueOf(rippleColor), _backgroundDrawable, null);
					_nativeButton.SetBackground(_rippleDrawable);
				}
				else
				{
					_nativeButton.SetBackground(_backgroundDrawable);
				}

				_drawableEnabled = true;
			}

			_nativeButton.Invalidate();
		}

		public void Reset()
		{
			if (_drawableEnabled)
			{
				_drawableEnabled = false;
				_backgroundDrawable?.Reset();
				_backgroundDrawable = null;
				_rippleDrawable = null;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					_backgroundDrawable?.Dispose();
					_backgroundDrawable = null;
					_defaultDrawable?.Dispose();
					_defaultDrawable = null;
					_rippleDrawable?.Dispose();
					_rippleDrawable = null;
					if (_button != null)
					{
						_button.PropertyChanged -= ButtonPropertyChanged;
						_button = null;
					}
					_nativeButton = null;
				}
				_disposed = true;
			}
		}

		void ButtonPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName.Equals(Button.BorderColorProperty.PropertyName) ||
				e.PropertyName.Equals(Button.BorderWidthProperty.PropertyName) ||
				e.PropertyName.Equals(Button.CornerRadiusProperty.PropertyName) ||
				e.PropertyName.Equals(VisualElement.BackgroundColorProperty.PropertyName) ||
				e.PropertyName.Equals(Specifics.Button.UseDefaultPaddingProperty.PropertyName) ||
				e.PropertyName.Equals(Specifics.Button.UseDefaultShadowProperty.PropertyName))
			{
				Reset();
				UpdateDrawable();
			}
		}

	}
}
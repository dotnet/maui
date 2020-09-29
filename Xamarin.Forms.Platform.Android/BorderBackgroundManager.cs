using System;
using System.ComponentModel;
using Android.Content.Res;
using Android.Graphics.Drawables;
using AColor = Android.Graphics.Color;
using AView = Android.Views.View;
using Specifics = Xamarin.Forms.PlatformConfiguration.AndroidSpecific;

namespace Xamarin.Forms.Platform.Android
{
	internal class BorderBackgroundManager : IDisposable
	{
		Drawable _defaultDrawable;
		BorderDrawable _backgroundDrawable;
		RippleDrawable _rippleDrawable;
		bool _drawableEnabled;
		bool _disposed;
		IBorderVisualElementRenderer _renderer;
		IBorderElement _borderElement;

		VisualElement Element => _renderer?.Element;
		AView Control => _renderer?.View;
		readonly bool _drawOutlineWithBackground;

		public bool DrawOutlineWithBackground { get; set; } = true;
		public BorderDrawable BackgroundDrawable => _backgroundDrawable;

		public BorderBackgroundManager(IBorderVisualElementRenderer renderer) : this(renderer, true)
		{
		}

		public BorderBackgroundManager(IBorderVisualElementRenderer renderer, bool drawOutlineWithBackground)
		{
			_renderer = renderer;
			_renderer.ElementChanged += OnElementChanged;
			_drawOutlineWithBackground = drawOutlineWithBackground;
		}

		void OnElementChanged(object sender, VisualElementChangedEventArgs e)
		{
			if (e.OldElement != null)
			{
				if (e.OldElement is INotifyPropertyChanged oldElement)
					oldElement.PropertyChanged -= BorderElementPropertyChanged;
			}

			if (e.NewElement != null)
			{
				if (BorderPropertyChanged != null)
				{
					BorderPropertyChanged.PropertyChanged -= BorderElementPropertyChanged;
				}
				BorderElement = (IBorderElement)e.NewElement;

				if (BorderPropertyChanged != null)
					BorderPropertyChanged.PropertyChanged += BorderElementPropertyChanged;
			}

			Reset();
			UpdateDrawable();
		}

		public IBorderElement BorderElement
		{
			get => _borderElement;
			private set
			{
				_borderElement = value;
				BorderPropertyChanged = value as INotifyPropertyChanged;
			}
		}

		INotifyPropertyChanged BorderPropertyChanged { get; set; }

		public void UpdateDrawable()
		{
			if (BorderElement == null || Control == null)
				return;

			bool cornerRadiusIsDefault = !BorderElement.IsCornerRadiusSet() || (BorderElement.CornerRadius == (int)BorderElement.CornerRadiusDefaultValue || BorderElement.CornerRadius == BorderDrawable.DefaultCornerRadius);
			bool backgroundColorIsDefault = !BorderElement.IsBackgroundColorSet() || BorderElement.BackgroundColor == (Color)VisualElement.BackgroundColorProperty.DefaultValue;
			bool backgroundIsDefault = !BorderElement.IsBackgroundSet() || BorderElement.Background == (Brush)VisualElement.BackgroundProperty.DefaultValue;
			bool borderColorIsDefault = !BorderElement.IsBorderColorSet() || BorderElement.BorderColor == (Color)BorderElement.BorderColorDefaultValue;
			bool borderWidthIsDefault = !BorderElement.IsBorderWidthSet() || BorderElement.BorderWidth == (double)BorderElement.BorderWidthDefaultValue;

			if (backgroundColorIsDefault
				&& backgroundIsDefault
				&& cornerRadiusIsDefault
				&& borderColorIsDefault
				&& borderWidthIsDefault)
			{
				if (!_drawableEnabled)
					return;

				if (_defaultDrawable != null)
					Control.SetBackground(_defaultDrawable);

				_drawableEnabled = false;
				Reset();
			}
			else
			{
				if (_backgroundDrawable == null)
					_backgroundDrawable = new BorderDrawable(Control.Context.ToPixels, Forms.GetColorButtonNormal(Control.Context), _drawOutlineWithBackground);

				_backgroundDrawable.BorderElement = BorderElement;

				var useDefaultPadding = _renderer.UseDefaultPadding();

				int paddingTop = useDefaultPadding ? Control.PaddingTop : 0;
				int paddingLeft = useDefaultPadding ? Control.PaddingLeft : 0;

				var useDefaultShadow = _renderer.UseDefaultShadow();

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
				else if ((int)Forms.SdkInt >= 16)
				{
					shadowRadius = _renderer.ShadowRadius;
					shadowDy = _renderer.ShadowDy;
					shadowDx = _renderer.ShadowDx;
					shadowColor = _renderer.ShadowColor;
				}

				_backgroundDrawable.SetPadding(paddingTop, paddingLeft);
				if (_renderer.IsShadowEnabled())
				{
					_backgroundDrawable
						.SetShadow(shadowDy, shadowDx, shadowColor, shadowRadius);
				}

				if (_drawableEnabled)
					return;

				if (_defaultDrawable == null)
					_defaultDrawable = Control.Background;

				if (!backgroundColorIsDefault || _drawOutlineWithBackground)
				{
					if (Forms.IsLollipopOrNewer)
					{
						var rippleColor = _backgroundDrawable.PressedBackgroundColor.ToAndroid();
						_rippleDrawable = new RippleDrawable(ColorStateList.ValueOf(rippleColor), _backgroundDrawable, null);
						Control.SetBackground(_rippleDrawable);
					}
					else
					{
						Control.SetBackground(_backgroundDrawable);
					}
				}

				_drawableEnabled = true;
			}

			Control.Invalidate();
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

		public void UpdateBackgroundColor()
		{
			UpdateDrawable();
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
					if (BorderPropertyChanged != null)
					{
						BorderPropertyChanged.PropertyChanged -= BorderElementPropertyChanged;
					}

					BorderElement = null;

					if (_renderer != null)
					{
						_renderer.ElementChanged -= OnElementChanged;
						_renderer = null;
					}
				}
				_disposed = true;
			}
		}

		void BorderElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (_renderer.View.IsDisposed())
			{
				return;
			}

			if (e.PropertyName.Equals(Button.BorderColorProperty.PropertyName) ||
				e.PropertyName.Equals(Button.BorderWidthProperty.PropertyName) ||
				e.PropertyName.Equals(Button.CornerRadiusProperty.PropertyName) ||
				e.PropertyName.Equals(VisualElement.BackgroundColorProperty.PropertyName) ||
				e.PropertyName.Equals(VisualElement.BackgroundProperty.PropertyName) ||
				e.PropertyName.Equals(Specifics.Button.UseDefaultPaddingProperty.PropertyName) ||
				e.PropertyName.Equals(Specifics.Button.UseDefaultShadowProperty.PropertyName) ||
				e.PropertyName.Equals(Specifics.ImageButton.IsShadowEnabledProperty.PropertyName) ||
				e.PropertyName.Equals(Specifics.ImageButton.ShadowColorProperty.PropertyName) ||
				e.PropertyName.Equals(Specifics.ImageButton.ShadowOffsetProperty.PropertyName) ||
				e.PropertyName.Equals(Specifics.ImageButton.ShadowRadiusProperty.PropertyName))
			{
				Reset();
				UpdateDrawable();
			}
		}
	}
}
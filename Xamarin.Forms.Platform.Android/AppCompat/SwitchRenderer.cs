using System;
using System.ComponentModel;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using Xamarin.Forms.Platform.Android.FastRenderers;

namespace Xamarin.Forms.Platform.Android.AppCompat
{
	public class SwitchRenderer : ViewRenderer<Switch, SwitchCompat>, CompoundButton.IOnCheckedChangeListener
	{
		bool _disposed;
		Drawable _defaultTrackDrawable;
		string _defaultContentDescription;
		bool _changedThumbColor;

		public SwitchRenderer(Context context) : base(context)
		{
			AutoPackage = false;
		}

		[Obsolete("This constructor is obsolete as of version 2.5. Please use SwitchRenderer(Context) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public SwitchRenderer()
		{
			AutoPackage = false;
		}

		protected override void SetContentDescription()
			=> AutomationPropertiesProvider.SetBasicContentDescription(this, Element, ref _defaultContentDescription);

		void CompoundButton.IOnCheckedChangeListener.OnCheckedChanged(CompoundButton buttonView, bool isChecked)
		{
			((IViewController)Element).SetValueFromRenderer(Switch.IsToggledProperty, isChecked);
			UpdateOnColor();
		}

		public override SizeRequest GetDesiredSize(int widthConstraint, int heightConstraint)
		{
			SizeRequest sizeConstraint = base.GetDesiredSize(widthConstraint, heightConstraint);

			if (sizeConstraint.Request.Width == 0)
			{
				int width = widthConstraint;
				if (widthConstraint <= 0)
					width = (int)Context.GetThemeAttributeDp(global::Android.Resource.Attribute.SwitchMinWidth);

				sizeConstraint = new SizeRequest(new Size(width, sizeConstraint.Request.Height), new Size(width, sizeConstraint.Minimum.Height));
			}

			return sizeConstraint;
		}

		protected override SwitchCompat CreateNativeControl()
		{
			return new SwitchCompat(Context);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				_disposed = true;

				if (Element != null)
					Element.Toggled -= HandleToggled;

				Control?.SetOnCheckedChangeListener(null);

				_defaultTrackDrawable?.Dispose();
				_defaultTrackDrawable = null;
			}

			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Switch> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement != null)
				e.OldElement.Toggled -= HandleToggled;

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					SwitchCompat aswitch = CreateNativeControl();
					aswitch.SetOnCheckedChangeListener(this);
					SetNativeControl(aswitch);
					_defaultTrackDrawable = aswitch.TrackDrawable;
				}
				else
					UpdateEnabled(); // Normally set by SetNativeControl, but not when the Control is reused.

				e.NewElement.Toggled += HandleToggled;
				Control.Checked = e.NewElement.IsToggled;
				UpdateOnColor();
				UpdateThumbColor();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (this.IsDisposed())
			{
				return;
			}

			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Switch.OnColorProperty.PropertyName)
				UpdateOnColor();
			else if (e.PropertyName == Slider.ThumbColorProperty.PropertyName)
				UpdateThumbColor();
		}

		void UpdateOnColor()
		{
			if (Element == null || Control == null)
				return;

			if (Control.Checked)
			{
				if (Element.OnColor == Color.Default)
				{
					Control.TrackDrawable = _defaultTrackDrawable;
				}
				else
				{
					Control.TrackDrawable?.SetColorFilter(Element.OnColor, FilterMode.SrcAtop);
				}
			}
			else
			{
				Control.TrackDrawable?.ClearColorFilter();
			}
		}

		void UpdateThumbColor()
		{
			if (Element == null)
				return;

			if (Element.ThumbColor != Color.Default)
			{
				Control.ThumbDrawable?.SetColorFilter(Element.ThumbColor, FilterMode.SrcAtop);
				_changedThumbColor = true;
			}
			else
			{
				if (_changedThumbColor)
				{
					Control.ThumbDrawable?.ClearColorFilter();
					_changedThumbColor = false;
				}
			}
		}

		void HandleToggled(object sender, EventArgs e)
		{
			Control.Checked = Element.IsToggled;
		}

		void UpdateEnabled()
		{
			Control.Enabled = Element.IsEnabled;
		}
	}
}
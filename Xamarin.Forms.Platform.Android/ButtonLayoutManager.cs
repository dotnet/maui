using System;
using System.ComponentModel;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Text;
using AndroidX.Core.View;
using AndroidX.Core.Widget;
using Xamarin.Forms.Internals;
using AButton = Android.Widget.Button;
using ARect = Android.Graphics.Rect;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	// TODO: Currently the drawable is reloaded if the text or the layout changes.
	//       This is obviously not great, but it works. An optimization should
	//       be made to find the drawable in the view and just re-position.
	//       If we do this, we must remember to undo the offset in OnLayout.

	public class ButtonLayoutManager : IDisposable
	{
		// we use left/right as this does not resize the button when there is no text
		Button.ButtonContentLayout _imageOnlyLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Left, 0);

		// reuse this instance to save on allocations
		ARect _drawableBounds = new ARect();

		bool _disposed;
		IButtonLayoutRenderer _renderer;
		Thickness? _defaultPaddingPix;
		Button _element;
		bool _alignIconWithText;
		bool _preserveInitialPadding;
		bool _borderAdjustsPadding;
		bool _maintainLegacyMeasurements;
		bool _hasLayoutOccurred;

		public ButtonLayoutManager(IButtonLayoutRenderer renderer)
			: this(renderer, false, false, false, true)
		{
		}

		public ButtonLayoutManager(IButtonLayoutRenderer renderer,
			bool alignIconWithText,
			bool preserveInitialPadding,
			bool borderAdjustsPadding,
			bool maintainLegacyMeasurements)
		{
			_renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
			_renderer.ElementChanged += OnElementChanged;
			_alignIconWithText = alignIconWithText;
			_preserveInitialPadding = preserveInitialPadding;
			_borderAdjustsPadding = borderAdjustsPadding;
			_maintainLegacyMeasurements = maintainLegacyMeasurements;
		}

		AButton View => _renderer?.View ?? _renderer as AButton;

		Context Context => _renderer?.View?.Context;

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
					if (_renderer != null)
					{
						if (_element != null)
						{
							_element.PropertyChanged -= OnElementPropertyChanged;
						}

						_renderer.ElementChanged -= OnElementChanged;
						_renderer = null;
					}
				}
				_disposed = true;
			}
		}

		internal SizeRequest GetDesiredSize(int widthConstraint, int heightConstraint)
		{
			var previousHeight = View.MeasuredHeight;
			var previousWidth = View.MeasuredWidth;

			View.Measure(widthConstraint, heightConstraint);

			// if the measure of the view has changed then trigger a request for layout
			// if the measure hasn't changed then force a layout of the button
			if (previousHeight != View.MeasuredHeight || previousWidth != View.MeasuredWidth)
				View.MaybeRequestLayout();
			else
				View.ForceLayout();

			return new SizeRequest(new Size(View.MeasuredWidth, View.MeasuredHeight), Size.Zero);
		}

		public void OnLayout(bool changed, int left, int top, int right, int bottom)
		{
			if (_disposed || _renderer == null || _element == null)
				return;

			AButton view = View;
			if (view == null)
				return;

			Drawable drawable = null;
			Drawable[] drawables = TextViewCompat.GetCompoundDrawablesRelative(view);
			if (drawables != null)
			{
				foreach (var compoundDrawable in drawables)
				{
					if (compoundDrawable != null)
					{
						drawable = compoundDrawable;
						break;
					}
				}
			}

			if (drawable != null)
			{
				int iconWidth = drawable.IntrinsicWidth;
				drawable.CopyBounds(_drawableBounds);

				// Center the drawable in the button if there is no text.
				// We do not need to undo this as when we get some text, the drawable recreated
				if (string.IsNullOrEmpty(_element.Text))
				{
					var newLeft = (right - left - iconWidth) / 2 - view.PaddingLeft;

					_drawableBounds.Set(newLeft, _drawableBounds.Top, newLeft + iconWidth, _drawableBounds.Bottom);
					drawable.Bounds = _drawableBounds;
				}
				else
				{
					if (_alignIconWithText && _element.ContentLayout.IsHorizontal())
					{
						var buttonText = view.TextFormatted;

						// if text is transformed, add that transformation to to ensure correct calculation of icon padding
						if (view.TransformationMethod != null)
							buttonText = view.TransformationMethod.GetTransformationFormatted(buttonText, view);

						var measuredTextWidth = view.Paint.MeasureText(buttonText, 0, buttonText.Length());
						var textWidth = Math.Min((int)measuredTextWidth, view.Layout.Width);
						var contentsWidth = ViewCompat.GetPaddingStart(view) + iconWidth + view.CompoundDrawablePadding + textWidth + ViewCompat.GetPaddingEnd(view);

						var newLeft = (view.MeasuredWidth - contentsWidth) / 2;
						if (_element.ContentLayout.Position == Button.ButtonContentLayout.ImagePosition.Right)
							newLeft = -newLeft;
						if (ViewCompat.GetLayoutDirection(view) == ViewCompat.LayoutDirectionRtl)
							newLeft = -newLeft;

						_drawableBounds.Set(newLeft, _drawableBounds.Top, newLeft + iconWidth, _drawableBounds.Bottom);
						drawable.Bounds = _drawableBounds;
					}
				}
			}

			_hasLayoutOccurred = true;
		}

		public void OnViewAttachedToWindow(AView attachedView)
		{
			Update();
		}

		public void OnViewDetachedFromWindow(AView detachedView)
		{
		}

		public void Update()
		{
			if (View?.LayoutParameters == null && _hasLayoutOccurred)
				return;

			if (!UpdateTextAndImage())
				UpdateImage();
			UpdatePadding();
			UpdateLineBreakMode();
		}

		void OnElementChanged(object sender, VisualElementChangedEventArgs e)
		{
			if (_element != null)
			{
				_element.PropertyChanged -= OnElementPropertyChanged;
				_element = null;
			}

			if (e.NewElement is Button button)
			{
				_element = button;
				_element.PropertyChanged += OnElementPropertyChanged;
			}

			Update();
		}

		void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (_disposed || _renderer == null || _element == null)
				return;

			if (e.PropertyName == Button.PaddingProperty.PropertyName)
				UpdatePadding();
			else if (e.PropertyName == Button.ImageSourceProperty.PropertyName || e.PropertyName == Button.ContentLayoutProperty.PropertyName)
				UpdateImage();
			else if (e.IsOneOf(Button.TextProperty, VisualElement.IsVisibleProperty, Button.TextTransformProperty))
				UpdateTextAndImage();
			else if (e.PropertyName == Button.BorderWidthProperty.PropertyName && _borderAdjustsPadding)
				_element.InvalidateMeasureNonVirtual(InvalidationTrigger.MeasureChanged);
			else if (e.PropertyName == Button.LineBreakModeProperty.PropertyName)
				UpdateLineBreakMode();
		}

		void UpdatePadding()
		{
			AButton view = View;
			if (view == null)
				return;

			if (_disposed || _renderer == null || _element == null)
				return;

			if (!_defaultPaddingPix.HasValue)
				_defaultPaddingPix = new Thickness(view.PaddingLeft, view.PaddingTop, view.PaddingRight, view.PaddingBottom);

			// Currently the Padding Bindable property uses a creator factory so once it is set it can't become unset
			// I would say this is currently a bug but it's a bug that exists already in the code base.
			// Having this comment and this code more accurately demonstrates behavior then
			// having an else clause for when the PaddingProperty isn't set
			if (!_element.IsSet(Button.PaddingProperty))
				return;

			var padding = _element.Padding;
			var adjustment = 0.0;
			if (_borderAdjustsPadding && _element is IBorderElement borderElement && borderElement.IsBorderWidthSet() && borderElement.BorderWidth != borderElement.BorderWidthDefaultValue)
				adjustment = borderElement.BorderWidth;

			var defaultPadding = _preserveInitialPadding && _defaultPaddingPix.HasValue
				? _defaultPaddingPix.Value
				: new Thickness();

			view.SetPadding(
				(int)(Context.ToPixels(padding.Left + adjustment) + defaultPadding.Left),
				(int)(Context.ToPixels(padding.Top + adjustment) + defaultPadding.Top),
				(int)(Context.ToPixels(padding.Right + adjustment) + defaultPadding.Right),
				(int)(Context.ToPixels(padding.Bottom + adjustment) + defaultPadding.Bottom));
		}

		bool UpdateTextAndImage()
		{
			if (_disposed || _renderer?.View == null || _element == null)
				return false;

			if (View?.LayoutParameters == null && _hasLayoutOccurred)
				return false;

			AButton view = View;
			if (view == null)
				return false;

			var textTransform = _element.TextTransform;

			_renderer.View.SetAllCaps(textTransform == TextTransform.Default);

			string oldText = view.Text;
			view.Text = _element.UpdateFormsText(_element.Text, textTransform);

			// If we went from or to having no text, we need to update the image position
			if (string.IsNullOrEmpty(oldText) != string.IsNullOrEmpty(view.Text))
			{
				UpdateImage();
				return true;
			}

			return false;
		}

		void UpdateImage()
		{
			if (_disposed || _renderer == null || _element == null)
				return;

			AButton view = View;
			if (view == null)
				return;

			ImageSource elementImage = _element.ImageSource;

			if (elementImage == null || elementImage.IsEmpty)
			{
				view.SetCompoundDrawablesWithIntrinsicBounds(null, null, null, null);
				return;
			}

			// No text, so no need for relative position; just center the image
			// There's no option for just plain-old centering, so we'll use Top 
			// (which handles the horizontal centering) and some tricksy padding (in OnLayout)
			// to handle the vertical centering 
			var layout = string.IsNullOrEmpty(_element.Text) ? _imageOnlyLayout : _element.ContentLayout;

			if (_maintainLegacyMeasurements)
				view.CompoundDrawablePadding = (int)layout.Spacing;
			else
				view.CompoundDrawablePadding = (int)Context.ToPixels(layout.Spacing);

			Drawable existingImage = null;
			var images = TextViewCompat.GetCompoundDrawablesRelative(view);
			for (int i = 0; i < images.Length; i++)
				if (images[i] != null)
				{
					existingImage = images[i];
					break;
				}

			if (_renderer is IVisualElementRenderer visualElementRenderer)
			{
				visualElementRenderer.ApplyDrawableAsync(Button.ImageSourceProperty, Context, image =>
				{
					if (image == existingImage)
						return;

					switch (layout.Position)
					{
						case Button.ButtonContentLayout.ImagePosition.Top:
							TextViewCompat.SetCompoundDrawablesRelativeWithIntrinsicBounds(view, null, image, null, null);
							break;
						case Button.ButtonContentLayout.ImagePosition.Right:
							TextViewCompat.SetCompoundDrawablesRelativeWithIntrinsicBounds(view, null, null, image, null);
							break;
						case Button.ButtonContentLayout.ImagePosition.Bottom:
							TextViewCompat.SetCompoundDrawablesRelativeWithIntrinsicBounds(view, null, null, null, image);
							break;
						default:
							// Defaults to image on the left
							TextViewCompat.SetCompoundDrawablesRelativeWithIntrinsicBounds(view, image, null, null, null);
							break;
					}

					if (_hasLayoutOccurred)
						_element?.InvalidateMeasureNonVirtual(InvalidationTrigger.MeasureChanged);
				});
			}
		}

		void UpdateLineBreakMode()
		{
			AButton view = View;

			if (view == null || _element == null || _renderer?.View == null)
				return;

			view.SetLineBreakMode(_element);
			_renderer.View.SetAllCaps(_element.TextTransform == TextTransform.Default);
		}
	}
}
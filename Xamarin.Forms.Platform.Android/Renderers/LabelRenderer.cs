using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;
using System.ComponentModel;

namespace Xamarin.Forms.Platform.Android
{
	public class LabelRenderer : ViewRenderer<Label, TextView>
	{
		ColorStateList _labelTextColorDefault;
		float _lineSpacingExtraDefault;
		float _lineSpacingMultiplierDefault;
		int _lastConstraintHeight;
		int _lastConstraintWidth;

		SizeRequest? _lastSizeRequest;
		float _lastTextSize = -1f;
		Typeface _lastTypeface;

		Color _lastUpdateColor = Color.Default;
		FormsTextView _view;
		bool _wasFormatted;

		readonly MotionEventHelper _motionEventHelper = new MotionEventHelper();

		SpannableString _spannableString;

		public LabelRenderer(Context context) : base(context)
		{
			AutoPackage = false;
		}

		[Obsolete("This constructor is obsolete as of version 2.5. Please use LabelRenderer(Context) instead.")]
		public LabelRenderer()
		{
			AutoPackage = false;
		}

		public override SizeRequest GetDesiredSize(int widthConstraint, int heightConstraint)
		{
			if (_lastSizeRequest.HasValue)
			{
				// if we are measuring the same thing, no need to waste the time
				bool canRecycleLast = widthConstraint == _lastConstraintWidth && heightConstraint == _lastConstraintHeight;

				if (!canRecycleLast)
				{
					// if the last time we measured the returned size was all around smaller than the passed constraint
					// and the constraint is bigger than the last size request, we can assume the newly measured size request
					// will not change either.
					int lastConstraintWidthSize = MeasureSpecFactory.GetSize(_lastConstraintWidth);
					int lastConstraintHeightSize = MeasureSpecFactory.GetSize(_lastConstraintHeight);

					int currentConstraintWidthSize = MeasureSpecFactory.GetSize(widthConstraint);
					int currentConstraintHeightSize = MeasureSpecFactory.GetSize(heightConstraint);

					bool lastWasSmallerThanConstraints = _lastSizeRequest.Value.Request.Width < lastConstraintWidthSize && _lastSizeRequest.Value.Request.Height < lastConstraintHeightSize;

					bool currentConstraintsBiggerThanLastRequest = currentConstraintWidthSize >= _lastSizeRequest.Value.Request.Width && currentConstraintHeightSize >= _lastSizeRequest.Value.Request.Height;

					canRecycleLast = lastWasSmallerThanConstraints && currentConstraintsBiggerThanLastRequest;
				}

				if (canRecycleLast)
					return _lastSizeRequest.Value;
			}

			SizeRequest result = base.GetDesiredSize(widthConstraint, heightConstraint);
			result.Minimum = new Size(Math.Min(Context.ToPixels(10), result.Request.Width), result.Request.Height);

			_lastConstraintWidth = widthConstraint;
			_lastConstraintHeight = heightConstraint;
			_lastSizeRequest = result;

			return result;
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			base.OnLayout(changed, l, t, r, b);
			Control.RecalculateSpanPositions(Element, _spannableString, new SizeRequest(new Size(r - l, b - t)));
		}

		protected override TextView CreateNativeControl()
		{
			return new FormsTextView(Context);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
		{
			base.OnElementChanged(e);
			if (_view == null)
			{
				_view = (FormsTextView)CreateNativeControl();
				_labelTextColorDefault = _view.TextColors;
				_lineSpacingMultiplierDefault = _view.LineSpacingMultiplier;
				_lineSpacingExtraDefault = _view.LineSpacingExtra;
				SetNativeControl(_view);
			}

			if (e.OldElement == null)
			{
				UpdateText();
				UpdateLineBreakMode();
				UpdateGravity();
			}
			else
			{
				_view.SkipNextInvalidate();
				UpdateText();
				if (e.OldElement.LineBreakMode != e.NewElement.LineBreakMode)
					UpdateLineBreakMode();
				if (e.OldElement.HorizontalTextAlignment != e.NewElement.HorizontalTextAlignment || e.OldElement.VerticalTextAlignment != e.NewElement.VerticalTextAlignment)
					UpdateGravity();
			}

			_motionEventHelper.UpdateElement(e.NewElement);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Label.HorizontalTextAlignmentProperty.PropertyName || e.PropertyName == Label.VerticalTextAlignmentProperty.PropertyName)
				UpdateGravity();
			else if (e.PropertyName == Label.TextColorProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == Label.FontProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == Label.LineBreakModeProperty.PropertyName)
				UpdateLineBreakMode();
			else if (e.PropertyName == Label.TextProperty.PropertyName || e.PropertyName == Label.FormattedTextProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == Label.LineHeightProperty.PropertyName)
				UpdateLineHeight();
		}

		void UpdateColor()
		{
			Color c = Element.TextColor;
			if (c == _lastUpdateColor)
				return;
			_lastUpdateColor = c;

			if (c.IsDefault)
				_view.SetTextColor(_labelTextColorDefault);
			else
				_view.SetTextColor(c.ToAndroid());
		}

		void UpdateFont()
		{
#pragma warning disable 618 // We will need to update this when .Font goes away
			Font f = Element.Font;
#pragma warning restore 618

			Typeface newTypeface = f.ToTypeface();
			if (newTypeface != _lastTypeface)
			{
				_view.Typeface = newTypeface;
				_lastTypeface = newTypeface;
			}

			float newTextSize = f.ToScaledPixel();
			if (newTextSize != _lastTextSize)
			{
				_view.SetTextSize(ComplexUnitType.Sp, newTextSize);
				_lastTextSize = newTextSize;
			}
		}

		void UpdateGravity()
		{
			Label label = Element;

			_view.Gravity = label.HorizontalTextAlignment.ToHorizontalGravityFlags() | label.VerticalTextAlignment.ToVerticalGravityFlags();

			_lastSizeRequest = null;
		}

		void UpdateLineBreakMode()
		{
			_view.SetLineBreakMode(Element.LineBreakMode);
			_lastSizeRequest = null;
		}

		void UpdateLineHeight()
		{
			_lastSizeRequest = null;
			if (Element.LineHeight == -1)
				_view.SetLineSpacing(_lineSpacingExtraDefault, _lineSpacingMultiplierDefault);
			else if (Element.LineHeight >= 0)
				_view.SetLineSpacing(0, (float)Element.LineHeight);
		}

		void UpdateText()
		{
			if (Element.FormattedText != null)
			{
				FormattedString formattedText = Element.FormattedText ?? Element.Text;
#pragma warning disable 618 // We will need to update this when .Font goes away
				_view.TextFormatted = _spannableString = formattedText.ToAttributed(Element.Font, Element.TextColor, _view);
#pragma warning restore 618
				_wasFormatted = true;
			}
			else
			{
				if (_wasFormatted)
				{
					_view.SetTextColor(_labelTextColorDefault);
					_lastUpdateColor = Color.Default;
				}
				_view.Text = Element.Text;
				UpdateColor();
				UpdateFont();

				_wasFormatted = false;
			}

			_lastSizeRequest = null;
		}

		public override bool OnTouchEvent(MotionEvent e)
		{
			if (base.OnTouchEvent(e))
				return true;

			return _motionEventHelper.HandleMotionEvent(Parent, e);
		}
	}
}
using System;
using System.ComponentModel;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using Xamarin.Forms.PlatformConfiguration.TizenSpecific;

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
		[EditorBrowsable(EditorBrowsableState.Never)]
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

			//We need to clear the Hint or else it will interfere with the sizing of the Label
			var hint = Control.Hint;
			if (!string.IsNullOrEmpty(hint))
				Control.Hint = string.Empty;

			SizeRequest result = base.GetDesiredSize(widthConstraint, heightConstraint);

			//Set Hint back after sizing
			Control.Hint = hint;

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
				UpdateCharacterSpacing();
				UpdateLineHeight();
				UpdateGravity();
				UpdateMaxLines();
				UpdateFlowDirection();
			}
			else
			{
				UpdateText();
				if (e.OldElement.LineBreakMode != e.NewElement.LineBreakMode)
					UpdateLineBreakMode();
				if (e.OldElement.HorizontalTextAlignment != e.NewElement.HorizontalTextAlignment || e.OldElement.VerticalTextAlignment != e.NewElement.VerticalTextAlignment)
					UpdateGravity();
				if (e.OldElement.MaxLines != e.NewElement.MaxLines)
					UpdateMaxLines();
				if (e.OldElement.CharacterSpacing != e.NewElement.CharacterSpacing)
					UpdateCharacterSpacing();
				if (e.OldElement.FlowDirection != e.NewElement.FlowDirection)
					UpdateFlowDirection();
			}
			UpdateTextDecorations();
			UpdatePadding();
			_motionEventHelper.UpdateElement(e.NewElement);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (this.IsDisposed())
			{
				return;
			}

			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Label.HorizontalTextAlignmentProperty.PropertyName || e.PropertyName == Label.VerticalTextAlignmentProperty.PropertyName)
				UpdateGravity();
			else if (e.IsOneOf(Label.TextColorProperty, Label.TextTransformProperty))
				UpdateText();
			else if (e.PropertyName == Label.FontProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == Label.CharacterSpacingProperty.PropertyName)
				UpdateCharacterSpacing();
			else if (e.PropertyName == Label.LineBreakModeProperty.PropertyName)
				UpdateLineBreakMode();
			else if (e.PropertyName == Label.TextDecorationsProperty.PropertyName)
				UpdateTextDecorations();
			else if (e.IsOneOf(Label.TextProperty, Label.FormattedTextProperty, Label.TextTypeProperty))
				UpdateText();
			else if (e.PropertyName == Label.LineHeightProperty.PropertyName)
				UpdateLineHeight();
			else if (e.PropertyName == Label.MaxLinesProperty.PropertyName)
				UpdateMaxLines();
			else if (e.PropertyName == Label.PaddingProperty.PropertyName)
				UpdatePadding();
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
				UpdateFlowDirection();
		}

		void UpdateFlowDirection()
		{
			Control.UpdateFlowDirection(Element);
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

		void UpdateTextDecorations()
		{
			if (!Element.IsSet(Label.TextDecorationsProperty))
				return;

			var textDecorations = Element.TextDecorations;

			if ((textDecorations & TextDecorations.Strikethrough) == 0)
				_view.PaintFlags &= ~PaintFlags.StrikeThruText;
			else
				_view.PaintFlags |= PaintFlags.StrikeThruText;

			if ((textDecorations & TextDecorations.Underline) == 0)
				_view.PaintFlags &= ~PaintFlags.UnderlineText;
			else
				_view.PaintFlags |= PaintFlags.UnderlineText;
		}

		void UpdateGravity()
		{
			Label label = Element;

			_view.Gravity = label.HorizontalTextAlignment.ToHorizontalGravityFlags() | label.VerticalTextAlignment.ToVerticalGravityFlags();

			_lastSizeRequest = null;
		}

		void UpdateLineBreakMode()
		{
			_view.SetLineBreakMode(Element);
			_lastSizeRequest = null;
		}
		void UpdateCharacterSpacing()
		{
			if (Forms.IsLollipopOrNewer && Control is TextView textControl)
			{
				textControl.LetterSpacing = Element.CharacterSpacing.ToEm();
			}
		}

		void UpdateLineHeight()
		{
			_lastSizeRequest = null;
			if (Element.LineHeight == -1)
				_view.SetLineSpacing(_lineSpacingExtraDefault, _lineSpacingMultiplierDefault);
			else if (Element.LineHeight >= 0)
				_view.SetLineSpacing(0, (float)Element.LineHeight);
		}

		void UpdateMaxLines()
		{
			Control.SetMaxLines(Element);
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

				switch (Element.TextType)
				{

					case TextType.Html:
						if (Forms.IsNougatOrNewer)
							Control.SetText(Html.FromHtml(Element.Text ?? string.Empty, FromHtmlOptions.ModeCompact), TextView.BufferType.Spannable);
						else
#pragma warning disable CS0618 // Type or member is obsolete
							Control.SetText(Html.FromHtml(Element.Text ?? string.Empty), TextView.BufferType.Spannable);
#pragma warning restore CS0618 // Type or member is obsolete
						break;

					default:
						_view.Text = Element.UpdateFormsText(Element.Text, Element.TextTransform);

						break;
				}

				UpdateColor();
				UpdateFont();

				_wasFormatted = false;
			}

			_lastSizeRequest = null;
		}

		void UpdatePadding()
		{
			Control.SetPadding(
				(int)Context.ToPixels(Element.Padding.Left),
				(int)Context.ToPixels(Element.Padding.Top),
				(int)Context.ToPixels(Element.Padding.Right),
				(int)Context.ToPixels(Element.Padding.Bottom));

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
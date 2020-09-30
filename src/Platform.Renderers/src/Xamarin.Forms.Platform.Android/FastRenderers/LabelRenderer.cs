using System;
using System.ComponentModel;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Text;
using Android.Util;
using Android.Views;
using AndroidX.Core.View;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android.FastRenderers
{
	public class LabelRenderer : FormsTextView, IVisualElementRenderer, IViewRenderer, ITabStop
	{
		int? _defaultLabelFor;
		bool _disposed;
		Label _element;
		// Do not dispose _labelTextColorDefault
		readonly ColorStateList _labelTextColorDefault;
		int _lastConstraintHeight;
		int _lastConstraintWidth;
		SizeRequest? _lastSizeRequest;
		float _lastTextSize = -1f;
		Typeface _lastTypeface;
		Color _lastUpdateColor = Color.Default;
		float _lineSpacingExtraDefault = -1.0f;
		float _lineSpacingMultiplierDefault = -1.0f;
		VisualElementTracker _visualElementTracker;
		VisualElementRenderer _visualElementRenderer;
		readonly MotionEventHelper _motionEventHelper = new MotionEventHelper();
		SpannableString _spannableString;
		bool _hasLayoutOccurred;
		bool _wasFormatted;

		public LabelRenderer(Context context) : base(context)
		{
			_labelTextColorDefault = TextColors;
			_visualElementRenderer = new VisualElementRenderer(this);
			BackgroundManager.Init(this);
		}

		[Obsolete("This constructor is obsolete as of version 2.5. Please use LabelRenderer(Context) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public LabelRenderer() : base(Forms.Context)
		{
			_labelTextColorDefault = TextColors;
			_visualElementRenderer = new VisualElementRenderer(this);
			BackgroundManager.Init(this);
		}

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;
		public event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;

		VisualElement IVisualElementRenderer.Element => Element;

		VisualElementTracker IVisualElementRenderer.Tracker => _visualElementTracker;

		AView IVisualElementRenderer.View => this;

		AView ITabStop.TabStop => this;

		ViewGroup IVisualElementRenderer.ViewGroup => null;

		protected Label Element
		{
			get { return _element; }
			set
			{
				if (_element == value)
					return;

				Label oldElement = _element;
				_element = value;

				OnElementChanged(new ElementChangedEventArgs<Label>(oldElement, _element));

				_element?.SendViewInitialized(this);
			}
		}
		protected global::Android.Widget.TextView Control => this;

		SizeRequest IVisualElementRenderer.GetDesiredSize(int widthConstraint, int heightConstraint)
		{
			if (_disposed)
			{
				return new SizeRequest();
			}

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
			bool setHint = Control.LayoutParameters != null;
			if (!string.IsNullOrEmpty(hint) && setHint)
				Control.Hint = string.Empty;

			var hc = MeasureSpec.GetSize(heightConstraint);

			Measure(widthConstraint, heightConstraint);
			var result = new SizeRequest(new Size(MeasuredWidth, MeasuredHeight), new Size());

			//Set Hint back after sizing
			if (setHint)
				Control.Hint = hint;

			result.Minimum = new Size(Math.Min(Context.ToPixels(10), result.Request.Width), result.Request.Height);

			// if the measure of the view has changed then trigger a request for layout
			// if the measure hasn't changed then force a layout of the label
			var measureIsChanged = !_lastSizeRequest.HasValue ||
				_lastSizeRequest.HasValue && (_lastSizeRequest.Value.Request.Height != MeasuredHeight || _lastSizeRequest.Value.Request.Width != MeasuredWidth);
			if (measureIsChanged)
				this.MaybeRequestLayout();
			else
				ForceLayout();

			_lastConstraintWidth = widthConstraint;
			_lastConstraintHeight = heightConstraint;
			_lastSizeRequest = result;

			return result;
		}

		protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
		{
			base.OnLayout(changed, left, top, right, bottom);
			this.RecalculateSpanPositions(Element, _spannableString, new SizeRequest(new Size(right - left, bottom - top)));
			_hasLayoutOccurred = true;
		}

		void IVisualElementRenderer.SetElement(VisualElement element)
		{
			var label = element as Label;
			if (label == null)
				throw new ArgumentException("Element must be of type Label");

			Element = label;
			_motionEventHelper.UpdateElement(element);
		}

		void IVisualElementRenderer.SetLabelFor(int? id)
		{
			if (_defaultLabelFor == null)
				_defaultLabelFor = ViewCompat.GetLabelFor(this);

			ViewCompat.SetLabelFor(this, (int)(id ?? _defaultLabelFor));
		}

		void IVisualElementRenderer.UpdateLayout()
		{
			VisualElementTracker tracker = _visualElementTracker;
			tracker?.UpdateLayout();
		}

		void IViewRenderer.MeasureExactly()
		{
			ViewRenderer.MeasureExactly(this, Element, Context);
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				if (Element != null)
				{
					Element.PropertyChanged -= OnElementPropertyChanged;
				}

				BackgroundManager.Dispose(this);
				if (_visualElementTracker != null)
				{
					_visualElementTracker.Dispose();
					_visualElementTracker = null;
				}

				if (_visualElementRenderer != null)
				{
					_visualElementRenderer.Dispose();
					_visualElementRenderer = null;
				}

				_spannableString?.Dispose();

				if (Element != null)
				{
					if (Platform.GetRenderer(Element) == this)
						Element.ClearValue(Platform.RendererProperty);
				}
			}

			base.Dispose(disposing);
		}

		public override bool OnTouchEvent(MotionEvent e)
		{
			if (_visualElementRenderer.OnTouchEvent(e) || base.OnTouchEvent(e))
			{
				return true;
			}

			return _motionEventHelper.HandleMotionEvent(Parent, e);
		}

		protected virtual void OnElementChanged(ElementChangedEventArgs<Label> e)
		{
			ElementChanged?.Invoke(this, new VisualElementChangedEventArgs(e.OldElement, e.NewElement));

			if (e.OldElement != null)
			{
				e.OldElement.PropertyChanged -= OnElementPropertyChanged;
				this.MaybeRequestLayout();
			}

			if (e.NewElement != null)
			{
				this.EnsureId();

				if (_visualElementTracker == null)
				{
					_visualElementTracker = new VisualElementTracker(this);
				}

				e.NewElement.PropertyChanged += OnElementPropertyChanged;

				UpdateText();
				UpdateLineHeight();
				UpdateCharacterSpacing();
				UpdateTextDecorations();
				if (e.OldElement?.LineBreakMode != e.NewElement.LineBreakMode)
					UpdateLineBreakMode();
				if (e.OldElement?.HorizontalTextAlignment != e.NewElement.HorizontalTextAlignment || e.OldElement?.VerticalTextAlignment != e.NewElement.VerticalTextAlignment)
					UpdateGravity();
				if (e.OldElement?.MaxLines != e.NewElement.MaxLines)
					UpdateMaxLines();

				UpdatePadding();

				ElevationHelper.SetElevation(this, e.NewElement);
			}
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (this.IsDisposed())
			{
				return;
			}

			ElementPropertyChanged?.Invoke(this, e);

			if (Control?.LayoutParameters == null && _hasLayoutOccurred)
				return;

			if (e.PropertyName == Label.HorizontalTextAlignmentProperty.PropertyName || e.PropertyName == Label.VerticalTextAlignmentProperty.PropertyName)
				UpdateGravity();
			else if (e.PropertyName == Label.TextColorProperty.PropertyName ||
				e.PropertyName == Label.TextTypeProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == Label.FontProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == Label.LineBreakModeProperty.PropertyName)
				UpdateLineBreakMode();
			else if (e.PropertyName == Label.CharacterSpacingProperty.PropertyName)
				UpdateCharacterSpacing();
			else if (e.PropertyName == Label.TextDecorationsProperty.PropertyName)
				UpdateTextDecorations();
			else if (e.IsOneOf(Label.TextProperty, Label.FormattedTextProperty, Label.TextTransformProperty))
				UpdateText();
			else if (e.PropertyName == Label.LineHeightProperty.PropertyName)
				UpdateLineHeight();
			else if (e.PropertyName == Label.MaxLinesProperty.PropertyName)
				UpdateMaxLines();
			else if (e.PropertyName == Label.PaddingProperty.PropertyName)
				UpdatePadding();
		}

		void UpdateColor()
		{
			Color c = Element.TextColor;
			if (c == _lastUpdateColor)
				return;
			_lastUpdateColor = c;

			if (c.IsDefault)
				SetTextColor(_labelTextColorDefault);
			else
				SetTextColor(c.ToAndroid());
		}

		void UpdateFont()
		{
#pragma warning disable 618 // We will need to update this when .Font goes away
			Font f = Element.Font;
#pragma warning restore 618

			Typeface newTypeface = f.ToTypeface();
			if (newTypeface != _lastTypeface)
			{
				Typeface = newTypeface;
				_lastTypeface = newTypeface;
			}

			float newTextSize = f.ToScaledPixel();
			if (newTextSize != _lastTextSize)
			{
				SetTextSize(ComplexUnitType.Sp, newTextSize);
				_lastTextSize = newTextSize;
			}
		}

		void UpdateTextDecorations()
		{
			if (!Element.IsSet(Label.TextDecorationsProperty))
				return;

			var textDecorations = Element.TextDecorations;

			if ((textDecorations & TextDecorations.Strikethrough) == 0)
				PaintFlags &= ~PaintFlags.StrikeThruText;
			else
				PaintFlags |= PaintFlags.StrikeThruText;

			if ((textDecorations & TextDecorations.Underline) == 0)
				PaintFlags &= ~PaintFlags.UnderlineText;
			else
				PaintFlags |= PaintFlags.UnderlineText;
		}

		void UpdateGravity()
		{
			Label label = Element;

			Gravity = label.HorizontalTextAlignment.ToHorizontalGravityFlags() | label.VerticalTextAlignment.ToVerticalGravityFlags();

			_lastSizeRequest = null;
		}

		void UpdateCharacterSpacing()
		{
			if (Forms.IsLollipopOrNewer)
			{
				LetterSpacing = Element.CharacterSpacing.ToEm();
			}
		}

		void UpdateLineBreakMode()
		{
			this.SetLineBreakMode(Element);
			_lastSizeRequest = null;
		}

		void UpdateMaxLines()
		{
			this.SetMaxLines(Element);
			_lastSizeRequest = null;
		}

		void UpdateText()
		{
			if (Element.FormattedText != null)
			{
				FormattedString formattedText = Element.FormattedText ?? Element.Text;
#pragma warning disable 618 // We will need to update this when .Font goes away
				TextFormatted = _spannableString = formattedText.ToAttributed(Element.Font, Element.TextColor, this);
#pragma warning restore 618
				_wasFormatted = true;
			}
			else
			{
				if (_wasFormatted)
				{
					SetTextColor(_labelTextColorDefault);
					_lastUpdateColor = Color.Default;
				}

				switch (Element.TextType)
				{
					case TextType.Html:
						if (Forms.IsNougatOrNewer)
							Control.SetText(Html.FromHtml(Element.Text ?? string.Empty, FromHtmlOptions.ModeCompact), BufferType.Spannable);
						else
#pragma warning disable CS0618 // Type or member is obsolete
							Control.SetText(Html.FromHtml(Element.Text ?? string.Empty), BufferType.Spannable);
#pragma warning restore CS0618 // Type or member is obsolete
						break;

					default:
						Text = Element.UpdateFormsText(Element.Text, Element.TextTransform);
						break;
				}

				UpdateColor();
				UpdateFont();

				_wasFormatted = false;
			}

			_lastSizeRequest = null;
		}

		void UpdateLineHeight()
		{
			if (_lineSpacingExtraDefault < 0)
				_lineSpacingExtraDefault = LineSpacingExtra;
			if (_lineSpacingMultiplierDefault < 0)
				_lineSpacingMultiplierDefault = LineSpacingMultiplier;

			if (Element.LineHeight == -1)
				SetLineSpacing(_lineSpacingExtraDefault, _lineSpacingMultiplierDefault);
			else if (Element.LineHeight >= 0)
				SetLineSpacing(0, (float)Element.LineHeight);

			_lastSizeRequest = null;
		}

		void UpdatePadding()
		{
			SetPadding(
				(int)Context.ToPixels(Element.Padding.Left),
				(int)Context.ToPixels(Element.Padding.Top),
				(int)Context.ToPixels(Element.Padding.Right),
				(int)Context.ToPixels(Element.Padding.Bottom));

			_lastSizeRequest = null;
		}
	}
}
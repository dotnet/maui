#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Microsoft.Maui.Controls.Internals;

using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <summary>A <see cref="Microsoft.Maui.Controls.View"/> that displays text.</summary>
	[ContentProperty(nameof(Text))]
	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
	[ElementHandler(typeof(LabelHandler))]
	public partial class Label : View, IFontElement, ITextElement, ITextAlignmentElement, ILineHeightElement, IElementConfiguration<Label>, IDecorableTextElement, IPaddingElement, ILabel
	{
		/// <summary>Bindable property for <see cref="HorizontalTextAlignment"/>.</summary>
		public static readonly BindableProperty HorizontalTextAlignmentProperty = TextAlignmentElement.HorizontalTextAlignmentProperty;

		/// <summary>Bindable property for <see cref="VerticalTextAlignment"/>.</summary>
		public static readonly BindableProperty VerticalTextAlignmentProperty = BindableProperty.Create(nameof(VerticalTextAlignment), typeof(TextAlignment), typeof(Label), TextAlignment.Start);

		/// <summary>Bindable property for <see cref="TextColor"/>.</summary>
		public static readonly BindableProperty TextColorProperty = TextElement.TextColorProperty;

		/// <summary>Bindable property for <see cref="CharacterSpacing"/>.</summary>
		public static readonly BindableProperty CharacterSpacingProperty = TextElement.CharacterSpacingProperty;

		/// <summary>Bindable property for <see cref="Text"/>.</summary>
		public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(Label), default(string), propertyChanged: OnTextPropertyChanged);

		/// <summary>Bindable property for <see cref="FontFamily"/>.</summary>
		public static readonly BindableProperty FontFamilyProperty = FontElement.FontFamilyProperty;

		/// <summary>Bindable property for <see cref="FontSize"/>.</summary>
		public static readonly BindableProperty FontSizeProperty = FontElement.FontSizeProperty;

		/// <summary>Bindable property for <see cref="FontAttributes"/>.</summary>
		public static readonly BindableProperty FontAttributesProperty = FontElement.FontAttributesProperty;

		/// <summary>Bindable property for <see cref="FontAutoScalingEnabled"/>.</summary>
		public static readonly BindableProperty FontAutoScalingEnabledProperty = FontElement.FontAutoScalingEnabledProperty;

		/// <summary>Bindable property for <see cref="TextTransform"/>.</summary>
		public static readonly BindableProperty TextTransformProperty = TextElement.TextTransformProperty;

		/// <summary>Bindable property for <see cref="TextDecorations"/>.</summary>
		public static readonly BindableProperty TextDecorationsProperty = DecorableTextElement.TextDecorationsProperty;

		/// <summary>Bindable property for <see cref="FormattedText"/>.</summary>
		public static readonly BindableProperty FormattedTextProperty = BindableProperty.Create(nameof(FormattedText), typeof(FormattedString), typeof(Label), default(FormattedString),
			propertyChanging: (bindable, oldvalue, newvalue) =>
			{
				if (oldvalue != null)
				{
					var formattedString = ((FormattedString)oldvalue);
					var label = ((Label)bindable);

					formattedString.SpansCollectionChanged -= label.Span_CollectionChanged;
					formattedString.PropertyChanged -= label.OnFormattedTextChanged;
					formattedString.PropertyChanging -= label.OnFormattedTextChanging;
					formattedString.Parent = null;
					label.RemoveSpans(formattedString.Spans);
				}
			},
			propertyChanged: (bindable, oldvalue, newvalue) =>
			{
				var label = ((Label)bindable);

				if (newvalue != null)
				{
					var formattedString = (FormattedString)newvalue;
					formattedString.Parent = label;
					formattedString.PropertyChanging += label.OnFormattedTextChanging;
					formattedString.PropertyChanged += label.OnFormattedTextChanged;
					formattedString.SpansCollectionChanged += label.Span_CollectionChanged;
					label.SetupSpans(formattedString.Spans);
				}

				label.InvalidateMeasureIfLabelSizeable();

				if (newvalue != null)
					label.Text = null;
			});

		/// <include file="../../docs/Microsoft.Maui.Controls/Label.xml" path="//Member[@MemberName='TextTransform']/Docs/*" />
		public TextTransform TextTransform
		{
			get { return (TextTransform)GetValue(TextTransformProperty); }
			set { SetValue(TextTransformProperty, value); }
		}

		/// <param name="source">The source parameter.</param>
		/// <param name="textTransform">The textTransform parameter.</param>
		public virtual string UpdateFormsText(string source, TextTransform textTransform)
			=> TextTransformUtilities.GetTransformedText(source, textTransform);

		/// <summary>Bindable property for <see cref="LineBreakMode"/>.</summary>
		public static readonly BindableProperty LineBreakModeProperty = BindableProperty.Create(nameof(LineBreakMode), typeof(LineBreakMode), typeof(Label), LineBreakMode.WordWrap,
			propertyChanged: (bindable, oldvalue, newvalue) => ((Label)bindable).InvalidateMeasureIfLabelSizeable());

		/// <summary>Bindable property for <see cref="LineHeight"/>.</summary>
		public static readonly BindableProperty LineHeightProperty = LineHeightElement.LineHeightProperty;

		/// <summary>Bindable property for <see cref="MaxLines"/>.</summary>
		public static readonly BindableProperty MaxLinesProperty = BindableProperty.Create(nameof(MaxLines), typeof(int), typeof(Label), -1,
			propertyChanged: (bindable, oldvalue, newvalue) => ((Label)bindable).InvalidateMeasureIfLabelSizeable());

		/// <summary>Bindable property for <see cref="Padding"/>.</summary>
		public static readonly BindableProperty PaddingProperty = PaddingElement.PaddingProperty;

		/// <summary>Bindable property for <see cref="TextType"/>.</summary>
		public static readonly BindableProperty TextTypeProperty = BindableProperty.Create(nameof(TextType), typeof(TextType), typeof(Label), TextType.Text,
			propertyChanged: (bindable, oldvalue, newvalue) => ((Label)bindable).InvalidateMeasureIfLabelSizeable());

		readonly Lazy<PlatformConfigurationRegistry<Label>> _platformConfigurationRegistry;

		/// <summary>Initializes a new instance of the Label class.</summary>
		public Label()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<Label>>(() => new PlatformConfigurationRegistry<Label>(this));
		}

		protected override void OnBindingContextChanged()
		{
			base.OnBindingContextChanged();
			if (FormattedText != null)
				SetInheritedBindingContext(FormattedText, this.BindingContext);
		}

		/// <summary>Gets or sets the formatted text for the Label. This is a bindable property.</summary>
		/// <remarks>Setting FormattedText to a non-null value will set the Text property to null.</remarks>
		public FormattedString FormattedText
		{
			get { return (FormattedString)GetValue(FormattedTextProperty); }
			set { SetValue(FormattedTextProperty, value); }
		}

		/// <summary>Gets or sets the horizontal alignment of the Text property. This is a bindable property.</summary>
		public TextAlignment HorizontalTextAlignment
		{
			get { return (TextAlignment)GetValue(TextAlignmentElement.HorizontalTextAlignmentProperty); }
			set { SetValue(TextAlignmentElement.HorizontalTextAlignmentProperty, value); }
		}

		/// <summary>Gets or sets the LineBreakMode for the Label. This is a bindable property.</summary>
		public LineBreakMode LineBreakMode
		{
			get { return (LineBreakMode)GetValue(LineBreakModeProperty); }
			set { SetValue(LineBreakModeProperty, value); }
		}

		/// <summary>Gets or sets the text for the Label. This is a bindable property.</summary>
		/// <remarks>Setting Text to a non-null value will set the FormattedText property to null.</remarks>
		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		/// <summary>Gets or sets the <see cref="Microsoft.Maui.Graphics.Color"/> for the text of this Label. This is a bindable property.</summary>
		public Color TextColor
		{
			get { return (Color)GetValue(TextElement.TextColorProperty); }
			set { SetValue(TextElement.TextColorProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Label.xml" path="//Member[@MemberName='CharacterSpacing']/Docs/*" />
		public double CharacterSpacing
		{
			get { return (double)GetValue(TextElement.CharacterSpacingProperty); }
			set { SetValue(TextElement.CharacterSpacingProperty, value); }
		}

		/// <summary>Gets or sets the vertical alignement of the Text property. This is a bindable property.</summary>
		public TextAlignment VerticalTextAlignment
		{
			get { return (TextAlignment)GetValue(VerticalTextAlignmentProperty); }
			set { SetValue(VerticalTextAlignmentProperty, value); }
		}

		/// <summary>Gets a value that indicates whether the font for the label is bold, italic, or neither. This is a bindable property.</summary>
		public FontAttributes FontAttributes
		{
			get { return (FontAttributes)GetValue(FontAttributesProperty); }
			set { SetValue(FontAttributesProperty, value); }
		}

		/// <summary>Gets or sets the <see cref="Microsoft.Maui.TextDecorations"/> applied to <see cref="Microsoft.Maui.Controls.Label.Text"/>.</summary>
		public TextDecorations TextDecorations
		{
			get { return (TextDecorations)GetValue(TextDecorationsProperty); }
			set { SetValue(TextDecorationsProperty, value); }
		}

		/// <summary>Gets or sets the font family to which the font for the label belongs.</summary>
		/// <remarks>The font family can refer to a system font or a custom font.</remarks>
		public string FontFamily
		{
			get { return (string)GetValue(FontFamilyProperty); }
			set { SetValue(FontFamilyProperty, value); }
		}

		/// <summary>Gets the size of the font for the label.</summary>
		[System.ComponentModel.TypeConverter(typeof(FontSizeConverter))]
		public double FontSize
		{
			get { return (double)GetValue(FontSizeProperty); }
			set { SetValue(FontSizeProperty, value); }
		}

		public bool FontAutoScalingEnabled
		{
			get => (bool)GetValue(FontAutoScalingEnabledProperty);
			set => SetValue(FontAutoScalingEnabledProperty, value);
		}

		/// <summary>Gets or sets the multiplier to apply to the default line height when displaying text. This is a bindable property.</summary>
		public double LineHeight
		{
			get { return (double)GetValue(LineHeightProperty); }
			set { SetValue(LineHeightProperty, value); }
		}

		/// <summary>Gets or sets the maximum number of lines allowed in the <see cref="Microsoft.Maui.Controls.Label"/>.</summary>
		public int MaxLines
		{
			get => (int)GetValue(MaxLinesProperty);
			set => SetValue(MaxLinesProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Label.xml" path="//Member[@MemberName='Padding']/Docs/*" />
		public Thickness Padding
		{
			get { return (Thickness)GetValue(PaddingProperty); }
			set { SetValue(PaddingProperty, value); }
		}

		/// <summary>Determines whether the Label should display plain text or HTML text. This is a bindable property.</summary>
		public TextType TextType
		{
			get => (TextType)GetValue(TextTypeProperty);
			set => SetValue(TextTypeProperty, value);
		}

		double IFontElement.FontSizeDefaultValueCreator() =>
			this.GetDefaultFontSize();

		void IFontElement.OnFontAttributesChanged(FontAttributes oldValue, FontAttributes newValue) =>
			HandleFontChanged();

		void IFontElement.OnFontFamilyChanged(string oldValue, string newValue) =>
			HandleFontChanged();

		void IFontElement.OnFontSizeChanged(double oldValue, double newValue) =>
			HandleFontChanged();

		void IFontElement.OnFontAutoScalingEnabledChanged(bool oldValue, bool newValue) =>
			HandleFontChanged();

		void HandleFontChanged()
		{
			Handler?.UpdateValue(nameof(ITextStyle.Font));
			InvalidateMeasureIfLabelSizeable();
		}

		void ILineHeightElement.OnLineHeightChanged(double oldValue, double newValue) =>
			InvalidateMeasureIfLabelSizeable();

		void ITextElement.OnTextTransformChanged(TextTransform oldValue, TextTransform newValue) =>
			InvalidateMeasureIfLabelSizeable();

		void OnFormattedTextChanging(object sender, PropertyChangingEventArgs e) =>
			OnPropertyChanging(nameof(FormattedText));

		void OnFormattedTextChanged(object sender, PropertyChangedEventArgs e)
		{
			OnPropertyChanged(nameof(FormattedText));
			InvalidateMeasureIfLabelSizeable();
		}

		void SetupSpans(IEnumerable spans)
		{
			foreach (Span span in spans)
			{
				span.GestureRecognizersCollectionChanged += Span_GestureRecognizer_CollectionChanged;
				SetupSpanGestureRecognizers(span.GestureRecognizers);
			}
		}

		void SetupSpanGestureRecognizers(IEnumerable gestureRecognizers)
		{
			foreach (GestureRecognizer gestureRecognizer in gestureRecognizers)
				GestureController.CompositeGestureRecognizers.Add(new ChildGestureRecognizer() { GestureRecognizer = gestureRecognizer });
		}


		void RemoveSpans(IEnumerable spans)
		{
			foreach (Span span in spans)
			{
				RemoveSpanGestureRecognizers(span.GestureRecognizers);
				span.GestureRecognizersCollectionChanged -= Span_GestureRecognizer_CollectionChanged;
			}
		}

		void RemoveSpanGestureRecognizers(IEnumerable gestureRecognizers)
		{
			foreach (GestureRecognizer gestureRecognizer in gestureRecognizers)
				foreach (var spanRecognizer in GestureController.CompositeGestureRecognizers.ToList())
					if (spanRecognizer is ChildGestureRecognizer childGestureRecognizer && childGestureRecognizer.GestureRecognizer == gestureRecognizer)
						GestureController.CompositeGestureRecognizers.Remove(spanRecognizer);
		}


		void Span_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					SetupSpans(e.NewItems);
					break;
				case NotifyCollectionChangedAction.Remove:
					RemoveSpans(e.OldItems);
					break;
				case NotifyCollectionChangedAction.Replace:
					RemoveSpans(e.OldItems);
					SetupSpans(e.NewItems);
					break;
				case NotifyCollectionChangedAction.Reset:
					// Is never called, because the clear command is overridden.
					break;
			}
		}

		void Span_GestureRecognizer_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{

			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					SetupSpanGestureRecognizers(e.NewItems);
					break;
				case NotifyCollectionChangedAction.Remove:
					RemoveSpanGestureRecognizers(e.OldItems);
					break;
				case NotifyCollectionChangedAction.Replace:
					RemoveSpanGestureRecognizers(e.OldItems);
					SetupSpanGestureRecognizers(e.NewItems);
					break;
				case NotifyCollectionChangedAction.Reset:
					// is never called, because the clear command is overridden.
					break;
			}
		}

		void ITextAlignmentElement.OnHorizontalTextAlignmentPropertyChanged(TextAlignment oldValue, TextAlignment newValue)
		{
			// This is a no-op since the horizontal text alignment does not affect bounds or
			// any other property that would require a measure invalidation.
		}

		static void OnTextPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
		{
			var label = (Label)bindable;

			if (TextChangedShouldInvalidateMeasure(label))
				label.InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);

			if (newvalue != null)
				label.FormattedText = null;
		}

		/// <inheritdoc/>
		public IPlatformElementConfiguration<T, Label> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		void ITextElement.OnTextColorPropertyChanged(Color oldValue, Color newValue)
		{
			// This is a no-op since the text color does not affect bounds or
			// any other property that would require a measure invalidation.
		}

		void ITextElement.OnCharacterSpacingPropertyChanged(double oldValue, double newValue) =>
			InvalidateMeasureIfLabelSizeable();

		internal bool HasFormattedTextSpans
			=> (FormattedText?.Spans?.Count ?? 0) > 0;

		/// <summary>Returns the child elements that are under the specified point.</summary>
		/// <param name="point">The point under which to look for child elements.</param>
		/// <returns>The child elements that are under the specified point.</returns>
		public override IList<GestureElement> GetChildElements(Point point)
		{
			if (FormattedText?.Spans == null || FormattedText?.Spans.Count == 0)
				return null;

			var spans = new List<GestureElement>();
			for (int i = 0; i < FormattedText.Spans.Count; i++)
			{
				Span span = FormattedText.Spans[i];
				if (span.GestureRecognizers.Count > 0 && (((ISpatialElement)span).Region.Contains(point) || point.IsEmpty))
					spans.Add(span);
			}

			if (!point.IsEmpty && spans.Count > 1) // More than 2 elements overlapping, deflate to see which one is actually hit.
				for (var i = spans.Count - 1; i >= 0; i--)
					if (!((ISpatialElement)spans[i]).Region.Deflate().Contains(point))
						spans.RemoveAt(i);

			return spans;
		}

		Thickness IPaddingElement.PaddingDefaultValueCreator() => default;

		void IPaddingElement.OnPaddingPropertyChanged(Thickness oldValue, Thickness newValue) =>
			InvalidateMeasureIfLabelSizeable();

		Font ITextStyle.Font => this.ToFont();

		/// <summary>
		/// This method prevents unnecessary measure invalidations when the label is not
		/// sizeable. If the label has a fixed width and height, then no matter what the
		/// text is, the label will never change size.
		/// </summary>
		void InvalidateMeasureIfLabelSizeable()
		{
			if (!IsLabelSizeable(this))
				return;

			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
		}

		/// <summary>
		/// Determines if the label can grow in any direction based on the constraints. If the
		/// label cannot grow in any direction, then we usually don't need to do anything.
		/// </summary>
		internal static bool IsLabelSizeable(Label label)
		{
			// Determine in which direction the label can grow/shrink.
			var constraint = label.Constraint;
			var isVerticallySizeable = (constraint & LayoutConstraint.VerticallyFixed) == 0;
			var isHorizontallySizeable = (constraint & LayoutConstraint.HorizontallyFixed) == 0;
			var isSizeable = isVerticallySizeable || isHorizontallySizeable;

			// If the label cannot grow in any direction, then we usually don't need to do anything.
			if (!isSizeable)
				return false;

			// The label may grow/shrink based on the constraints, so we may need to invalidate.
			return true;
		}

		/// <summary>
		/// Determines if the text has changed in a way that would require a measure invalidation.
		/// Unlike FormattedText changes, Text changes may not always require invalidation because
		/// the text size and spacing is all uniform. Formatted text may have a case where even
		/// though the label is a single line, the font size of a span may cause the label to grow
		/// vertically.
		/// </summary>
		internal static bool TextChangedShouldInvalidateMeasure(Label label)
		{
			// If the label cannot grow in any direction, then we don't need to invalidate.
			var isSizeable = IsLabelSizeable(label);
			if (!isSizeable)
				return false;

			// Determine if the label can grow vertically (wrapping means it may grow vertically).
			var constraint = label.Constraint;
			var breakMode = label.LineBreakMode;
			var isHorizontallySizeable = (constraint & LayoutConstraint.HorizontallyFixed) == 0;
			var isMultiline = breakMode == LineBreakMode.CharacterWrap || breakMode == LineBreakMode.WordWrap;
			var isSingleLine = !isMultiline;

			// If the label cannot grow horizontally and is only single line,
			// then we don't need to invalidate since the only direction it can grow in
			// is vertically but it never will.
			if (!isHorizontallySizeable && isSingleLine)
				return false;

			// The label may grow/shrink based on the constraints, so we need to invalidate.
			return true;
		}

		private protected override string GetDebuggerDisplay()
		{
			var debugText = DebuggerDisplayHelpers.GetDebugText(nameof(Text), Text);
			return $"{base.GetDebuggerDisplay()}, {debugText}";
		}

		internal override bool TrySetValue(string text)
		{
			Text = text;
			return true;
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/Label.xml" path="Type[@FullName='Microsoft.Maui.Controls.Label']/Docs/*" />
	[ContentProperty(nameof(Text))]
	public partial class Label : View, IFontElement, ITextElement, ITextAlignmentElement, ILineHeightElement, IElementConfiguration<Label>, IDecorableTextElement, IPaddingElement
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/Label.xml" path="//Member[@MemberName='HorizontalTextAlignmentProperty']/Docs/*" />
		public static readonly BindableProperty HorizontalTextAlignmentProperty = TextAlignmentElement.HorizontalTextAlignmentProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Label.xml" path="//Member[@MemberName='VerticalTextAlignmentProperty']/Docs/*" />
		public static readonly BindableProperty VerticalTextAlignmentProperty = BindableProperty.Create("VerticalTextAlignment", typeof(TextAlignment), typeof(Label), TextAlignment.Start);

		/// <include file="../../docs/Microsoft.Maui.Controls/Label.xml" path="//Member[@MemberName='TextColorProperty']/Docs/*" />
		public static readonly BindableProperty TextColorProperty = TextElement.TextColorProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Label.xml" path="//Member[@MemberName='CharacterSpacingProperty']/Docs/*" />
		public static readonly BindableProperty CharacterSpacingProperty = TextElement.CharacterSpacingProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Label.xml" path="//Member[@MemberName='TextProperty']/Docs/*" />
		public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(Label), default(string), propertyChanged: OnTextPropertyChanged);

		/// <include file="../../docs/Microsoft.Maui.Controls/Label.xml" path="//Member[@MemberName='FontFamilyProperty']/Docs/*" />
		public static readonly BindableProperty FontFamilyProperty = FontElement.FontFamilyProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Label.xml" path="//Member[@MemberName='FontSizeProperty']/Docs/*" />
		public static readonly BindableProperty FontSizeProperty = FontElement.FontSizeProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Label.xml" path="//Member[@MemberName='FontAttributesProperty']/Docs/*" />
		public static readonly BindableProperty FontAttributesProperty = FontElement.FontAttributesProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Label.xml" path="//Member[@MemberName='FontAutoScalingEnabledProperty']/Docs/*" />
		public static readonly BindableProperty FontAutoScalingEnabledProperty = FontElement.FontAutoScalingEnabledProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Label.xml" path="//Member[@MemberName='TextTransformProperty']/Docs/*" />
		public static readonly BindableProperty TextTransformProperty = TextElement.TextTransformProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Label.xml" path="//Member[@MemberName='TextDecorationsProperty']/Docs/*" />
		public static readonly BindableProperty TextDecorationsProperty = DecorableTextElement.TextDecorationsProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Label.xml" path="//Member[@MemberName='FormattedTextProperty']/Docs/*" />
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
			}, propertyChanged: (bindable, oldvalue, newvalue) =>
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

				label.InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
				if (newvalue != null)
					label.Text = null;
			});

		/// <include file="../../docs/Microsoft.Maui.Controls/Label.xml" path="//Member[@MemberName='TextTransform']/Docs/*" />
		public TextTransform TextTransform
		{
			get { return (TextTransform)GetValue(TextTransformProperty); }
			set { SetValue(TextTransformProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Label.xml" path="//Member[@MemberName='UpdateFormsText']/Docs/*" />
		public virtual string UpdateFormsText(string source, TextTransform textTransform)
			=> TextTransformUtilites.GetTransformedText(source, textTransform);

		/// <include file="../../docs/Microsoft.Maui.Controls/Label.xml" path="//Member[@MemberName='LineBreakModeProperty']/Docs/*" />
		public static readonly BindableProperty LineBreakModeProperty = BindableProperty.Create(nameof(LineBreakMode), typeof(LineBreakMode), typeof(Label), LineBreakMode.WordWrap,
			propertyChanged: (bindable, oldvalue, newvalue) => ((Label)bindable).InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged));

		/// <include file="../../docs/Microsoft.Maui.Controls/Label.xml" path="//Member[@MemberName='LineHeightProperty']/Docs/*" />
		public static readonly BindableProperty LineHeightProperty = LineHeightElement.LineHeightProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Label.xml" path="//Member[@MemberName='MaxLinesProperty']/Docs/*" />
		public static readonly BindableProperty MaxLinesProperty = BindableProperty.Create(nameof(MaxLines), typeof(int), typeof(Label), -1, propertyChanged: (bindable, oldvalue, newvalue) =>
			{
				if (bindable != null)
				{
					((Label)bindable).InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
				}
			});

		/// <include file="../../docs/Microsoft.Maui.Controls/Label.xml" path="//Member[@MemberName='PaddingProperty']/Docs/*" />
		public static readonly BindableProperty PaddingProperty = PaddingElement.PaddingProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Label.xml" path="//Member[@MemberName='TextTypeProperty']/Docs/*" />
		public static readonly BindableProperty TextTypeProperty = BindableProperty.Create(nameof(TextType), typeof(TextType), typeof(Label), TextType.Text,
			propertyChanged: (bindable, oldvalue, newvalue) => ((Label)bindable).InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged));

		readonly Lazy<PlatformConfigurationRegistry<Label>> _platformConfigurationRegistry;

		/// <include file="../../docs/Microsoft.Maui.Controls/Label.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/Label.xml" path="//Member[@MemberName='FormattedText']/Docs/*" />
		public FormattedString FormattedText
		{
			get { return (FormattedString)GetValue(FormattedTextProperty); }
			set { SetValue(FormattedTextProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Label.xml" path="//Member[@MemberName='HorizontalTextAlignment']/Docs/*" />
		public TextAlignment HorizontalTextAlignment
		{
			get { return (TextAlignment)GetValue(TextAlignmentElement.HorizontalTextAlignmentProperty); }
			set { SetValue(TextAlignmentElement.HorizontalTextAlignmentProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Label.xml" path="//Member[@MemberName='LineBreakMode']/Docs/*" />
		public LineBreakMode LineBreakMode
		{
			get { return (LineBreakMode)GetValue(LineBreakModeProperty); }
			set { SetValue(LineBreakModeProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Label.xml" path="//Member[@MemberName='Text']/Docs/*" />
		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Label.xml" path="//Member[@MemberName='TextColor']/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/Label.xml" path="//Member[@MemberName='VerticalTextAlignment']/Docs/*" />
		public TextAlignment VerticalTextAlignment
		{
			get { return (TextAlignment)GetValue(VerticalTextAlignmentProperty); }
			set { SetValue(VerticalTextAlignmentProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Label.xml" path="//Member[@MemberName='FontAttributes']/Docs/*" />
		public FontAttributes FontAttributes
		{
			get { return (FontAttributes)GetValue(FontAttributesProperty); }
			set { SetValue(FontAttributesProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Label.xml" path="//Member[@MemberName='TextDecorations']/Docs/*" />
		public TextDecorations TextDecorations
		{
			get { return (TextDecorations)GetValue(TextDecorationsProperty); }
			set { SetValue(TextDecorationsProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Label.xml" path="//Member[@MemberName='FontFamily']/Docs/*" />
		public string FontFamily
		{
			get { return (string)GetValue(FontFamilyProperty); }
			set { SetValue(FontFamilyProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Label.xml" path="//Member[@MemberName='FontSize']/Docs/*" />
		[System.ComponentModel.TypeConverter(typeof(FontSizeConverter))]
		public double FontSize
		{
			get { return (double)GetValue(FontSizeProperty); }
			set { SetValue(FontSizeProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Label.xml" path="//Member[@MemberName='FontAutoScalingEnabled']/Docs/*" />
		public bool FontAutoScalingEnabled
		{
			get => (bool)GetValue(FontAutoScalingEnabledProperty);
			set => SetValue(FontAutoScalingEnabledProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Label.xml" path="//Member[@MemberName='LineHeight']/Docs/*" />
		public double LineHeight
		{
			get { return (double)GetValue(LineHeightProperty); }
			set { SetValue(LineHeightProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Label.xml" path="//Member[@MemberName='MaxLines']/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/Label.xml" path="//Member[@MemberName='TextType']/Docs/*" />
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
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
		}

		void ILineHeightElement.OnLineHeightChanged(double oldValue, double newValue) =>
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);

		void OnFormattedTextChanging(object sender, PropertyChangingEventArgs e)
		{
			OnPropertyChanging("FormattedText");
		}

		void ITextElement.OnTextTransformChanged(TextTransform oldValue, TextTransform newValue) =>
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);

		void OnFormattedTextChanged(object sender, PropertyChangedEventArgs e)
		{
			OnPropertyChanged("FormattedText");
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
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
		}

		static void OnTextPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
		{
			var label = (Label)bindable;
			LineBreakMode breakMode = label.LineBreakMode;
			bool isVerticallyFixed = (label.Constraint & LayoutConstraint.VerticallyFixed) != 0;
			bool isSingleLine = !(breakMode == LineBreakMode.CharacterWrap || breakMode == LineBreakMode.WordWrap);
			if (!isVerticallyFixed || !isSingleLine)
				((Label)bindable).InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
			if (newvalue != null)
				((Label)bindable).FormattedText = null;
		}

		/// <inheritdoc/>
		public IPlatformElementConfiguration<T, Label> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		void ITextElement.OnTextColorPropertyChanged(Color oldValue, Color newValue)
		{
		}

		void ITextElement.OnCharacterSpacingPropertyChanged(double oldValue, double newValue)
		{
			InvalidateMeasure();
		}

		internal bool HasFormattedTextSpans
			=> (FormattedText?.Spans?.Count ?? 0) > 0;

		/// <include file="../../docs/Microsoft.Maui.Controls/Label.xml" path="//Member[@MemberName='GetChildElements']/Docs/*" />
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

		Thickness IPaddingElement.PaddingDefaultValueCreator()
		{
			return default(Thickness);
		}

		void IPaddingElement.OnPaddingPropertyChanged(Thickness oldValue, Thickness newValue)
		{
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[ContentProperty("Text")]
	[RenderWith(typeof(_LabelRenderer))]
	public class Label : View, IFontElement, ITextElement, ITextAlignmentElement, ILineHeightElement, IElementConfiguration<Label>, IDecorableTextElement, IPaddingElement
	{
		public static readonly BindableProperty HorizontalTextAlignmentProperty = TextAlignmentElement.HorizontalTextAlignmentProperty;

		[Obsolete("XAlignProperty is obsolete as of version 2.0.0. Please use HorizontalTextAlignmentProperty instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static readonly BindableProperty XAlignProperty = HorizontalTextAlignmentProperty;

		public static readonly BindableProperty VerticalTextAlignmentProperty = BindableProperty.Create("VerticalTextAlignment", typeof(TextAlignment), typeof(Label), TextAlignment.Start,
			propertyChanged: OnVerticalTextAlignmentPropertyChanged);

		[Obsolete("YAlignProperty is obsolete as of version 2.0.0. Please use VerticalTextAlignmentProperty instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static readonly BindableProperty YAlignProperty = VerticalTextAlignmentProperty;

		public static readonly BindableProperty TextColorProperty = TextElement.TextColorProperty;

		public static readonly BindableProperty CharacterSpacingProperty = TextElement.CharacterSpacingProperty;

		public static readonly BindableProperty FontProperty = FontElement.FontProperty;

		public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(Label), default(string), propertyChanged: OnTextPropertyChanged);

		public static readonly BindableProperty FontFamilyProperty = FontElement.FontFamilyProperty;

		public static readonly BindableProperty FontSizeProperty = FontElement.FontSizeProperty;

		public static readonly BindableProperty FontAttributesProperty = FontElement.FontAttributesProperty;

		public static readonly BindableProperty TextTransformProperty = TextElement.TextTransformProperty;

		public static readonly BindableProperty TextDecorationsProperty = DecorableTextElement.TextDecorationsProperty;

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

		public TextTransform TextTransform
		{
			get { return (TextTransform)GetValue(TextTransformProperty); }
			set { SetValue(TextTransformProperty, value); }
		}

		public virtual string UpdateFormsText(string source, TextTransform textTransform)
			=> TextTransformUtilites.GetTransformedText(source, textTransform);

		public static readonly BindableProperty LineBreakModeProperty = BindableProperty.Create(nameof(LineBreakMode), typeof(LineBreakMode), typeof(Label), LineBreakMode.WordWrap,
			propertyChanged: (bindable, oldvalue, newvalue) => ((Label)bindable).InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged));

		public static readonly BindableProperty LineHeightProperty = LineHeightElement.LineHeightProperty;

		public static readonly BindableProperty MaxLinesProperty = BindableProperty.Create(nameof(MaxLines), typeof(int), typeof(Label), -1, propertyChanged: (bindable, oldvalue, newvalue) =>
			{
				if (bindable != null)
				{
					((Label)bindable).InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
				}
			});

		public static readonly BindableProperty PaddingProperty = PaddingElement.PaddingProperty;

		public static readonly BindableProperty TextTypeProperty = BindableProperty.Create(nameof(TextType), typeof(TextType), typeof(Label), TextType.Text,
			propertyChanged: (bindable, oldvalue, newvalue) => ((Label)bindable).InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged));

		readonly Lazy<PlatformConfigurationRegistry<Label>> _platformConfigurationRegistry;

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

		[Obsolete("Font is obsolete as of version 1.3.0. Please use the Font attributes which are on the class itself.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Font Font
		{
			get { return (Font)GetValue(FontProperty); }
			set { SetValue(FontProperty, value); }
		}

		public FormattedString FormattedText
		{
			get { return (FormattedString)GetValue(FormattedTextProperty); }
			set { SetValue(FormattedTextProperty, value); }
		}

		public TextAlignment HorizontalTextAlignment
		{
			get { return (TextAlignment)GetValue(TextAlignmentElement.HorizontalTextAlignmentProperty); }
			set { SetValue(TextAlignmentElement.HorizontalTextAlignmentProperty, value); }
		}

		public LineBreakMode LineBreakMode
		{
			get { return (LineBreakMode)GetValue(LineBreakModeProperty); }
			set { SetValue(LineBreakModeProperty, value); }
		}

		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		public Color TextColor
		{
			get { return (Color)GetValue(TextElement.TextColorProperty); }
			set { SetValue(TextElement.TextColorProperty, value); }
		}

		public double CharacterSpacing
		{
			get { return (double)GetValue(TextElement.CharacterSpacingProperty); }
			set { SetValue(TextElement.CharacterSpacingProperty, value); }
		}

		public TextAlignment VerticalTextAlignment
		{
			get { return (TextAlignment)GetValue(VerticalTextAlignmentProperty); }
			set { SetValue(VerticalTextAlignmentProperty, value); }
		}

		[Obsolete("XAlign is obsolete as of version 2.0.0. Please use HorizontalTextAlignment instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public TextAlignment XAlign
		{
			get { return (TextAlignment)GetValue(XAlignProperty); }
			set { SetValue(XAlignProperty, value); }
		}

		[Obsolete("YAlign is obsolete as of version 2.0.0. Please use VerticalTextAlignment instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public TextAlignment YAlign
		{
			get { return (TextAlignment)GetValue(YAlignProperty); }
			set { SetValue(YAlignProperty, value); }
		}

		public FontAttributes FontAttributes
		{
			get { return (FontAttributes)GetValue(FontAttributesProperty); }
			set { SetValue(FontAttributesProperty, value); }
		}

		public TextDecorations TextDecorations
		{
			get { return (TextDecorations)GetValue(TextDecorationsProperty); }
			set { SetValue(TextDecorationsProperty, value); }
		}

		public string FontFamily
		{
			get { return (string)GetValue(FontFamilyProperty); }
			set { SetValue(FontFamilyProperty, value); }
		}

		[TypeConverter(typeof(FontSizeConverter))]
		public double FontSize
		{
			get { return (double)GetValue(FontSizeProperty); }
			set { SetValue(FontSizeProperty, value); }
		}

		public double LineHeight
		{
			get { return (double)GetValue(LineHeightProperty); }
			set { SetValue(LineHeightProperty, value); }
		}

		public int MaxLines
		{
			get => (int)GetValue(MaxLinesProperty);
			set => SetValue(MaxLinesProperty, value);
		}

		public Thickness Padding
		{
			get { return (Thickness)GetValue(PaddingProperty); }
			set { SetValue(PaddingProperty, value); }
		}

		public TextType TextType
		{
			get => (TextType)GetValue(TextTypeProperty);
			set => SetValue(TextTypeProperty, value);
		}

		double IFontElement.FontSizeDefaultValueCreator() =>
			Device.GetNamedSize(NamedSize.Default, (Label)this);

		void IFontElement.OnFontAttributesChanged(FontAttributes oldValue, FontAttributes newValue) =>
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);

		void IFontElement.OnFontFamilyChanged(string oldValue, string newValue) =>
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);

		void IFontElement.OnFontSizeChanged(double oldValue, double newValue) =>
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);

		void IFontElement.OnFontChanged(Font oldValue, Font newValue) =>
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);

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

		void SetupSpans(System.Collections.IEnumerable spans)
		{
			foreach (Span span in spans)
			{
				span.GestureRecognizersCollectionChanged += Span_GestureRecognizer_CollectionChanged;
				SetupSpanGestureRecognizers(span.GestureRecognizers);
			}
		}

		void SetupSpanGestureRecognizers(System.Collections.IEnumerable gestureRecognizers)
		{
			foreach (GestureRecognizer gestureRecognizer in gestureRecognizers)
				GestureController.CompositeGestureRecognizers.Add(new ChildGestureRecognizer() { GestureRecognizer = gestureRecognizer });
		}


		void RemoveSpans(System.Collections.IEnumerable spans)
		{
			foreach (Span span in spans)
			{
				RemoveSpanGestureRecognizers(span.GestureRecognizers);
				span.GestureRecognizersCollectionChanged -= Span_GestureRecognizer_CollectionChanged;
			}
		}

		void RemoveSpanGestureRecognizers(System.Collections.IEnumerable gestureRecognizers)
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
#pragma warning disable 0618 // retain until XAlign removed
			OnPropertyChanged(nameof(XAlign));
#pragma warning restore
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

		static void OnVerticalTextAlignmentPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var label = (Label)bindable;
#pragma warning disable 0618 // retain until YAlign removed
			label.OnPropertyChanged(nameof(YAlign));
#pragma warning restore 0618
		}

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
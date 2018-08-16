using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[ContentProperty("Text")]
	[RenderWith(typeof(_LabelRenderer))]
	public class Label : View, IFontElement, ITextElement, ITextAlignmentElement, ILineHeightElement, IElementConfiguration<Label>, IDecorableTextElement
	{
		public static readonly BindableProperty HorizontalTextAlignmentProperty = TextAlignmentElement.HorizontalTextAlignmentProperty;

		[Obsolete("XAlignProperty is obsolete as of version 2.0.0. Please use HorizontalTextAlignmentProperty instead.")]
		public static readonly BindableProperty XAlignProperty = HorizontalTextAlignmentProperty;

		public static readonly BindableProperty VerticalTextAlignmentProperty = BindableProperty.Create("VerticalTextAlignment", typeof(TextAlignment), typeof(Label), TextAlignment.Start,
			propertyChanged: OnVerticalTextAlignmentPropertyChanged);

		[Obsolete("YAlignProperty is obsolete as of version 2.0.0. Please use VerticalTextAlignmentProperty instead.")]
		public static readonly BindableProperty YAlignProperty = VerticalTextAlignmentProperty;

		public static readonly BindableProperty TextColorProperty = TextElement.TextColorProperty;

		public static readonly BindableProperty FontProperty = FontElement.FontProperty;

		public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(Label), default(string), propertyChanged: OnTextPropertyChanged);

		public static readonly BindableProperty FontFamilyProperty = FontElement.FontFamilyProperty;

		public static readonly BindableProperty FontSizeProperty = FontElement.FontSizeProperty;

		public static readonly BindableProperty FontAttributesProperty = FontElement.FontAttributesProperty;

		public static readonly BindableProperty TextDecorationsProperty = DecorableTextElement.TextDecorationsProperty;

		public static readonly BindableProperty FormattedTextProperty = BindableProperty.Create(nameof(FormattedText), typeof(FormattedString), typeof(Label), default(FormattedString),
			propertyChanging: (bindable, oldvalue, newvalue) =>
			{
				if (oldvalue != null)
				{
					var formattedString = ((FormattedString)oldvalue);

					// Remove spans here, to ensure all collection watching, removes necessary event hooks.
					for (int i = formattedString.Spans.Count - 1; i >= 0; i--)
						formattedString.Spans.RemoveAt(i);

					((ObservableCollection<Span>)formattedString.Spans).CollectionChanged -= ((Label)bindable).Span_CollectionChanged;
					formattedString.PropertyChanged -= ((Label)bindable).OnFormattedTextChanged;
					formattedString.Parent = null;
				}
			}, propertyChanged: (bindable, oldvalue, newvalue) =>
			{
				if (newvalue != null)
				{
					var label = ((Label)bindable);
					var formattedString = (FormattedString)newvalue;
					formattedString.Parent = label;
					formattedString.PropertyChanged += label.OnFormattedTextChanged;

					((ObservableCollection<Span>)formattedString.Spans).CollectionChanged += label.Span_CollectionChanged;

					// Initial Load of FormattedText could come preloaded with spans
					for (int i = 0; i < formattedString.Spans.Count; i++)
						for (int j = 0; j < formattedString.Spans[i].GestureRecognizers.Count; j++)
							((IGestureController)label).CompositeGestureRecognizers.Add(new ChildGestureRecognizer() { GestureRecognizer = formattedString.Spans[i].GestureRecognizers[j] });
				}

				((Label)bindable).InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
				if (newvalue != null)
					((Label)bindable).Text = null;
			});

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

		public TextAlignment VerticalTextAlignment
		{
			get { return (TextAlignment)GetValue(VerticalTextAlignmentProperty); }
			set { SetValue(VerticalTextAlignmentProperty, value); }
		}

		[Obsolete("XAlign is obsolete as of version 2.0.0. Please use HorizontalTextAlignment instead.")]
		public TextAlignment XAlign
		{
			get { return (TextAlignment)GetValue(XAlignProperty); }
			set { SetValue(XAlignProperty, value); }
		}

		[Obsolete("YAlign is obsolete as of version 2.0.0. Please use VerticalTextAlignment instead.")]
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

		void OnFormattedTextChanged(object sender, PropertyChangedEventArgs e)
		{
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
			OnPropertyChanged("FormattedText");
		}

		void Span_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					AddItems();
					break;
				case NotifyCollectionChangedAction.Remove:
					RemoveItems();
					break;
				case NotifyCollectionChangedAction.Replace:
					RemoveItems();
					AddItems();
					break;
				case NotifyCollectionChangedAction.Reset:
					// Is never called, because the clear command is overridden.
					break;
			}

			void AddItems()
			{
				foreach (GestureElement span in e.NewItems)
				{
					((ObservableCollection<IGestureRecognizer>)span.GestureRecognizers).CollectionChanged += Span_GestureRecognizer_CollectionChanged;
					// span could be preloaded with GestureRecognizers
					for (int i = 0; i < span.GestureRecognizers.Count; i++)
						((IGestureController)this).CompositeGestureRecognizers.Add(new ChildGestureRecognizer() { GestureRecognizer = span.GestureRecognizers[i] });
				}
			}

			void RemoveItems()
			{
				foreach (GestureElement span in e.OldItems)
				{
					for (int i = span.GestureRecognizers.Count - 1; i >= 0; i--)
						span.GestureRecognizers.RemoveAt(i);

					((ObservableCollection<IGestureRecognizer>)span.GestureRecognizers).CollectionChanged -= Span_GestureRecognizer_CollectionChanged;
				}
			}
		}

		void Span_GestureRecognizer_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			void AddItems()
			{
				for (var i = 0; i < e.NewItems.Count; i++)
					GestureController.CompositeGestureRecognizers.Add(new ChildGestureRecognizer()
					{
						GestureRecognizer = (IGestureRecognizer)e.NewItems[i]
					});
			}

			void RemoveItems()
			{
				for (int i = 0; i < e.OldItems.Count; i++)
					foreach (var spanRecognizer in GestureController.CompositeGestureRecognizers.ToList())
						if (spanRecognizer is ChildGestureRecognizer && spanRecognizer == e.OldItems[i])
							GestureController.CompositeGestureRecognizers.Remove(spanRecognizer);
			}

			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					AddItems();
					break;
				case NotifyCollectionChangedAction.Remove:
					RemoveItems();
					break;
				case NotifyCollectionChangedAction.Replace:
					RemoveItems();
					AddItems();
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

		public override IList<GestureElement> GetChildElements(Point point)
		{
			if (FormattedText?.Spans == null || FormattedText?.Spans.Count == 0)
				return null;

			var spans = new List<GestureElement>();
			foreach (var span in FormattedText.Spans)
				if (span.GestureRecognizers.Count > 0 && (((ISpatialElement)span).Region.Contains(point) || point.IsEmpty))
					spans.Add(span);

			if (spans.Count > 1) // More than 2 elements overlapping, deflate to see which one is actually hit.
				for (var i = spans.Count - 1; i >= 0; i--)
					if (!((ISpatialElement)spans[i]).Region.Deflate().Contains(point))
						spans.RemoveAt(i);

			return spans;
		}
	}
}
#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Microsoft.Maui;

namespace Microsoft.Maui.Controls
{
	/// <summary>Represents a text with attributes applied to some parts.</summary>
	[ContentProperty("Spans")]
	[TypeConverter(typeof(FormattedStringConverter))]
	public class FormattedString : Element
	{
		readonly SpanCollection _spans = new SpanCollection();
		readonly WeakEventManager _weakEventManager = new WeakEventManager();
		readonly List<SpanPropertyProxy> _spanProxies = new List<SpanPropertyProxy>();

		PropertyChangingEventHandler _onItemPropertyChanging;
		PropertyChangedEventHandler _onItemPropertyChanged;

		internal event NotifyCollectionChangedEventHandler SpansCollectionChanged
		{
			add => _weakEventManager.AddEventHandler(value, nameof(SpansCollectionChanged));
			remove => _weakEventManager.RemoveEventHandler(value, nameof(SpansCollectionChanged));
		}

		/// <summary>Initializes a new instance of the FormattedString class.</summary>
		public FormattedString() => _spans.CollectionChanged += OnCollectionChanged;

		~FormattedString()
		{
			for (int i = 0; i < _spanProxies.Count; i++)
				_spanProxies[i].Unsubscribe();
		}

		protected override void OnBindingContextChanged()
		{
			base.OnBindingContextChanged();
			for (int i = 0; i < Spans.Count; i++)
				SetInheritedBindingContext(Spans[i], BindingContext);
		}

		/// <summary>Gets the collection of spans.</summary>
		public IList<Span> Spans => _spans;

		public static explicit operator string(FormattedString formatted) => formatted.ToString();

		public static implicit operator FormattedString(string text) => new FormattedString { Spans = { new Span { Text = text } } };

		/// <summary>Returns the text of the formatted string as an unformatted string.</summary>
		public override string ToString() => string.Concat(Spans.Select(span => span.Text));

		void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.OldItems != null)
			{
				foreach (object item in e.OldItems)
				{
					var bo = item as Span;
					if (bo != null)
					{
						bo.Parent?.RemoveLogicalChild(bo);
						RemoveSpanProxy(bo);
					}

				}
			}

			if (e.NewItems != null)
			{
				foreach (object item in e.NewItems)
				{
					var bo = item as Span;
					if (bo != null)
					{
						this.AddLogicalChild(bo);
						AddSpanProxy(bo);
					}

				}
			}

			OnPropertyChanged(nameof(Spans));
			_weakEventManager.HandleEvent(sender, e, nameof(SpansCollectionChanged));
		}

		void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e) => OnPropertyChanged(nameof(Spans));

		void OnItemPropertyChanging(object sender, PropertyChangingEventArgs e) => OnPropertyChanging(nameof(Spans));

		// Subscribe to each span's PropertyChanging/PropertyChanged through weak proxies so that a
		// shared or long-lived Span does not strongly root this FormattedString (and its owning
		// Label) via the span's event invocation list. See https://github.com/dotnet/maui/issues/36517.
		void AddSpanProxy(Span span)
		{
			_onItemPropertyChanging ??= OnItemPropertyChanging;
			_onItemPropertyChanged ??= OnItemPropertyChanged;
			_spanProxies.Add(new SpanPropertyProxy(span, _onItemPropertyChanging, _onItemPropertyChanged));
		}

		void RemoveSpanProxy(Span span)
		{
			for (int i = _spanProxies.Count - 1; i >= 0; i--)
			{
				if (ReferenceEquals(_spanProxies[i].Span, span))
				{
					_spanProxies[i].Unsubscribe();
					_spanProxies.RemoveAt(i);
					break;
				}
			}
		}

		sealed class SpanPropertyProxy
		{
			readonly WeakNotifyPropertyChangingProxy _changing;
			readonly WeakNotifyPropertyChangedProxy _changed;

			public Span Span { get; }

			public SpanPropertyProxy(Span span, PropertyChangingEventHandler changingHandler, PropertyChangedEventHandler changedHandler)
			{
				Span = span;
				_changing = new WeakNotifyPropertyChangingProxy(span, changingHandler);
				_changed = new WeakNotifyPropertyChangedProxy(span, changedHandler);
			}

			public void Unsubscribe()
			{
				_changing.Unsubscribe();
				_changed.Unsubscribe();
			}
		}

		class SpanCollection : ObservableCollection<Span>
		{
			protected override void InsertItem(int index, Span item) => base.InsertItem(index, item ?? throw new ArgumentNullException(nameof(item)));
			protected override void SetItem(int index, Span item) => base.SetItem(index, item ?? throw new ArgumentNullException(nameof(item)));

			protected override void ClearItems()
			{
				var removed = new List<Span>(this);
				base.ClearItems();
				base.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removed));
			}
		}

		private sealed class FormattedStringConverter : TypeConverter
		{
			public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
				=> sourceType == typeof(string);

			public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
				=> destinationType == typeof(string);

			public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
			{
				if (value is string strValue)
				{
					return (FormattedString)strValue;
				}

				throw new NotSupportedException();
			}

			public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
			{
				if (value is FormattedString formattedStr)
				{
					return (string)formattedStr;
				}

				throw new NotSupportedException();
			}
		}
	}
}
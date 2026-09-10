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

		// _spans is owned by this FormattedString, so subscribing to it directly cannot outlive us.
		// Individual spans are a different matter: a Span the app holds on to -- or shares between
		// FormattedStrings -- would root this instance through its PropertyChanged/PropertyChanging
		// handlers, so those subscriptions go through weak proxies instead.
		readonly Dictionary<Span, (WeakNotifyPropertyChangedProxy Changed, WeakNotifyPropertyChangingProxy Changing)> _subscribedSpans = new();
		PropertyChangedEventHandler _itemPropertyChanged;
		PropertyChangingEventHandler _itemPropertyChanging;

		internal event NotifyCollectionChangedEventHandler SpansCollectionChanged
		{
			add => _weakEventManager.AddEventHandler(value, nameof(SpansCollectionChanged));
			remove => _weakEventManager.RemoveEventHandler(value, nameof(SpansCollectionChanged));
		}

		/// <summary>Initializes a new instance of the FormattedString class.</summary>
		public FormattedString() => _spans.CollectionChanged += OnCollectionChanged;

		~FormattedString()
		{
			foreach (var proxies in _subscribedSpans.Values)
			{
				proxies.Changed.Unsubscribe();
				proxies.Changing.Unsubscribe();
			}
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
						DetachSpan(bo);
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
						AttachSpan(bo);
					}

				}
			}

			OnPropertyChanged(nameof(Spans));
			_weakEventManager.HandleEvent(sender, e, nameof(SpansCollectionChanged));
		}

		void AttachSpan(Span span)
		{
			// The same Span instance can legally appear more than once in the collection; subscribe once.
			if (_subscribedSpans.ContainsKey(span))
				return;

			_itemPropertyChanged ??= OnItemPropertyChanged;
			_itemPropertyChanging ??= OnItemPropertyChanging;

			var changed = new WeakNotifyPropertyChangedProxy();
			changed.Subscribe(span, _itemPropertyChanged);

			var changing = new WeakNotifyPropertyChangingProxy();
			changing.Subscribe(span, _itemPropertyChanging);

			_subscribedSpans[span] = (changed, changing);
		}

		void DetachSpan(Span span)
		{
			// Only tear the subscription down once the last occurrence has gone.
			if (_spans.Contains(span))
				return;

			if (_subscribedSpans.Remove(span, out var proxies))
			{
				proxies.Changed.Unsubscribe();
				proxies.Changing.Unsubscribe();
			}
		}

		void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e) => OnPropertyChanged(nameof(Spans));

		void OnItemPropertyChanging(object sender, PropertyChangingEventArgs e) => OnPropertyChanging(nameof(Spans));

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
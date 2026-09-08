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

		internal event NotifyCollectionChangedEventHandler SpansCollectionChanged
		{
			add => _weakEventManager.AddEventHandler(value, nameof(SpansCollectionChanged));
			remove => _weakEventManager.RemoveEventHandler(value, nameof(SpansCollectionChanged));
		}

		// Subscribe to each Span's PropertyChanging/PropertyChanged via per-occurrence weak
		// subscription tokens so that a shared or long-lived Span (e.g. one held by a view-model or
		// App.Resources) does not keep this FormattedString alive through the event subscriptions.
		readonly SpanSubscriptions _spanSubscriptions;

		/// <summary>Initializes a new instance of the FormattedString class.</summary>
		public FormattedString()
		{
			_spanSubscriptions = new SpanSubscriptions(this);
			_spans.CollectionChanged += OnCollectionChanged;
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
						_spanSubscriptions.Remove(bo);
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
						_spanSubscriptions.Add(bo);
					}

				}
			}

			OnPropertyChanged(nameof(Spans));
			_weakEventManager.HandleEvent(sender, e, nameof(SpansCollectionChanged));
		}

		void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e) => OnPropertyChanged(nameof(Spans));

		void OnItemPropertyChanging(object sender, PropertyChangingEventArgs e) => OnPropertyChanging(nameof(Spans));

		sealed class SpanSubscriptions
		{
			readonly WeakReference<FormattedString> _owner;
			readonly List<SpanSubscription> _subscriptions = new();

			public SpanSubscriptions(FormattedString owner) => _owner = new(owner);

			~SpanSubscriptions() => Clear();

			public void Add(Span span) => _subscriptions.Add(new SpanSubscription(_owner, span));

			public void Remove(Span span)
			{
				for (int i = 0; i < _subscriptions.Count; i++)
				{
					if (_subscriptions[i].Span == span)
					{
						_subscriptions[i].Unsubscribe();
						_subscriptions.RemoveAt(i);
						return;
					}
				}
			}

			void Clear()
			{
				foreach (var subscription in _subscriptions)
				{
					subscription.Unsubscribe();
				}

				_subscriptions.Clear();
			}
		}

		sealed class SpanSubscription
		{
			// The Span's event delegate holds a strong reference to this SpanSubscription, so the
			// instance is kept alive until either the Span fires an event (which triggers self-cleanup
			// via the weak-owner check in OnPropertyChanged/OnPropertyChanging) or the SpanSubscriptions
			// finalizer runs. This is an accepted trade-off of weak-event cleanup: only these small
			// tokens may linger briefly, while the owning FormattedString is free to be collected.
			readonly WeakReference<FormattedString> _owner;
			Span _span;

			public SpanSubscription(WeakReference<FormattedString> owner, Span span)
			{
				_owner = owner;
				_span = span;
				_span.PropertyChanging += OnPropertyChanging;
				_span.PropertyChanged += OnPropertyChanged;
			}

			public Span Span => _span;

			public void Unsubscribe()
			{
				if (_span is null)
				{
					return;
				}

				_span.PropertyChanging -= OnPropertyChanging;
				_span.PropertyChanged -= OnPropertyChanged;
				_span = null;
			}

			void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
			{
				if (_owner.TryGetTarget(out var owner))
				{
					owner.OnItemPropertyChanged(sender, e);
				}
				else
				{
					Unsubscribe();
				}
			}

			void OnPropertyChanging(object sender, PropertyChangingEventArgs e)
			{
				if (_owner.TryGetTarget(out var owner))
				{
					owner.OnItemPropertyChanging(sender, e);
				}
				else
				{
					Unsubscribe();
				}
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
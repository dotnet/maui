using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Xamarin.Forms
{
	[ContentProperty("Spans")]
	public class FormattedString : Element
	{
		readonly SpanCollection _spans = new SpanCollection();
		internal event NotifyCollectionChangedEventHandler SpansCollectionChanged;

		public FormattedString() => _spans.CollectionChanged += OnCollectionChanged;

		protected override void OnBindingContextChanged()
		{
			base.OnBindingContextChanged();
			for (int i = 0; i < Spans.Count; i++)
				SetInheritedBindingContext(Spans[i], BindingContext);
		}

		public IList<Span> Spans => _spans;

		public static explicit operator string(FormattedString formatted) => formatted.ToString();

		public static implicit operator FormattedString(string text) => new FormattedString { Spans = { new Span { Text = text } } };

		public override string ToString() => string.Concat(Spans.Select(span => span.Text));

		void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.OldItems != null)
			{
				foreach (object item in e.OldItems)
				{
					var bo = item as Span;
					bo.Parent = null;
					if (bo != null)
					{
						bo.PropertyChanging -= OnItemPropertyChanging;
						bo.PropertyChanged -= OnItemPropertyChanged;
					}

				}
			}

			if (e.NewItems != null)
			{
				foreach (object item in e.NewItems)
				{
					var bo = item as Span;
					bo.Parent = this;
					if (bo != null)
					{
						bo.PropertyChanging += OnItemPropertyChanging;
						bo.PropertyChanged += OnItemPropertyChanged;
					}

				}
			}

			OnPropertyChanged(nameof(Spans));
			SpansCollectionChanged?.Invoke(sender, e);
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
	}
}
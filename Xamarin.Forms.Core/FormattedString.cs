using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Xamarin.Forms
{
	[ContentProperty("Spans")]
	public class FormattedString : INotifyPropertyChanged
	{
		readonly SpanCollection _spans = new SpanCollection();

		public FormattedString()
		{
			_spans.CollectionChanged += OnCollectionChanged;
		}

		public IList<Span> Spans
		{
			get { return _spans; }
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public static explicit operator string(FormattedString formatted)
		{
			return formatted.ToString();
		}

		public static implicit operator FormattedString(string text)
		{
			return new FormattedString { Spans = { new Span { Text = text } } };
		}

		public override string ToString()
		{
			return string.Concat(Spans.Select(span => span.Text));
		}

		void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.OldItems != null)
			{
				foreach (object item in e.OldItems)
				{
					var bo = item as Span;
					if (bo != null)
						bo.PropertyChanged -= OnItemPropertyChanged;
				}
			}

			if (e.NewItems != null)
			{
				foreach (object item in e.NewItems)
				{
					var bo = item as Span;
					if (bo != null)
						bo.PropertyChanged += OnItemPropertyChanged;
				}
			}

			OnPropertyChanged("Spans");
		}

		void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			OnPropertyChanged("Spans");
		}

		void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}

		class SpanCollection : ObservableCollection<Span>
		{
			protected override void InsertItem(int index, Span item)
			{
				if (item == null)
					throw new ArgumentNullException("item");

				base.InsertItem(index, item);
			}

			protected override void SetItem(int index, Span item)
			{
				if (item == null)
					throw new ArgumentNullException("item");

				base.SetItem(index, item);
			}
		}
	}
}
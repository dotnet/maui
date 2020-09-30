using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Xamarin.Forms
{
	public sealed class TableRoot : TableSectionBase<TableSection>
	{
		public TableRoot()
		{
			SetupEvents();
		}

		public TableRoot(string title) : base(title)
		{
			SetupEvents();
		}

		internal event EventHandler<ChildCollectionChangedEventArgs> SectionCollectionChanged;

		void ChildCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
		{
			EventHandler<ChildCollectionChangedEventArgs> handler = SectionCollectionChanged;
			if (handler != null)
				handler(this, new ChildCollectionChangedEventArgs(notifyCollectionChangedEventArgs));
		}

		void ChildPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
		{
			if (propertyChangedEventArgs.PropertyName == TitleProperty.PropertyName)
			{
				OnPropertyChanged(TitleProperty.PropertyName);
			}
		}

		void SetupEvents()
		{
			CollectionChanged += (sender, args) =>
			{
				if (args.NewItems != null)
				{
					foreach (TableSection section in args.NewItems)
					{
						section.CollectionChanged += ChildCollectionChanged;
						section.PropertyChanged += ChildPropertyChanged;
					}
				}

				if (args.OldItems != null)
				{
					foreach (TableSection section in args.OldItems)
					{
						section.CollectionChanged -= ChildCollectionChanged;
						section.PropertyChanged -= ChildPropertyChanged;
					}
				}
			};
		}
	}
}
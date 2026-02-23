#nullable disable
using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// The root element of a <see cref="TableView"/> that contains <see cref="TableSection"/> items.
	/// </summary>
	public sealed class TableRoot : TableSectionBase<TableSection>
	{
		/// <summary>
		/// Creates a new <see cref="TableRoot"/> with default values.
		/// </summary>
		public TableRoot()
		{
			SetupEvents();
		}

		/// <summary>
		/// Creates a new <see cref="TableRoot"/> with the specified title.
		/// </summary>
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
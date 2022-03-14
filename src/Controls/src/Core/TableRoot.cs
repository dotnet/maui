using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/TableRoot.xml" path="Type[@FullName='Microsoft.Maui.Controls.TableRoot']/Docs" />
	public sealed class TableRoot : TableSectionBase<TableSection>
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/TableRoot.xml" path="//Member[@MemberName='.ctor'][1]/Docs" />
		public TableRoot()
		{
			SetupEvents();
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/TableRoot.xml" path="//Member[@MemberName='.ctor'][2]/Docs" />
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
#nullable disable
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using Microsoft.Maui;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// The root element of a <see cref="TableView"/> that contains <see cref="TableSection"/> items.
	/// </summary>
	public sealed class TableRoot : TableSectionBase<TableSection>
	{
		readonly WeakEventManager _weakEventManager = new();

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

		internal event NotifyCollectionChangedEventHandler WeakCollectionChanged
		{
			add => _weakEventManager.AddEventHandler(value);
			remove => _weakEventManager.RemoveEventHandler(value);
		}

		internal event EventHandler<ChildCollectionChangedEventArgs> WeakSectionCollectionChanged
		{
			add => _weakEventManager.AddEventHandler(value);
			remove => _weakEventManager.RemoveEventHandler(value);
		}

		internal event PropertyChangedEventHandler WeakPropertyChanged
		{
			add => _weakEventManager.AddEventHandler(value);
			remove => _weakEventManager.RemoveEventHandler(value);
		}

		void ChildCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
		{
			var args = new ChildCollectionChangedEventArgs(notifyCollectionChangedEventArgs);
			_weakEventManager.HandleEvent(this, args, nameof(WeakSectionCollectionChanged));
		}

		void ChildPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
		{
			if (propertyChangedEventArgs.PropertyName == TitleProperty.PropertyName)
			{
				OnPropertyChanged(TitleProperty.PropertyName);
			}
		}

		protected override void OnPropertyChanged(string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);

			if (propertyName == TitleProperty.PropertyName)
				_weakEventManager.HandleEvent(this, new PropertyChangedEventArgs(propertyName), nameof(WeakPropertyChanged));
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

				_weakEventManager.HandleEvent(this, args, nameof(WeakCollectionChanged));
			};
		}
	}
}
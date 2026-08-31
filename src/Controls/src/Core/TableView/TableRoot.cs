#nullable disable
using System;
using System.Collections.Generic;
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
		SectionSubscriptions _sectionSubscriptions;
		NotifyCollectionChangedEventHandler _childCollectionChangedHandler;
		PropertyChangedEventHandler _childPropertyChangedHandler;

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
				if (args.Action == NotifyCollectionChangedAction.Reset)
				{
					_sectionSubscriptions?.UnsubscribeAll();

					foreach (TableSection section in this)
						SubscribeToSection(section);
				}
				else
				{
					if (args.OldItems != null)
					{
						foreach (TableSection section in args.OldItems)
							UnsubscribeFromSection(section);
					}

					if (args.NewItems != null)
					{
						foreach (TableSection section in args.NewItems)
							SubscribeToSection(section);
					}
				}

				_weakEventManager.HandleEvent(this, args, nameof(WeakCollectionChanged));
			};
		}

		void SubscribeToSection(TableSection section)
		{
			var subscriptions = _sectionSubscriptions ??= new SectionSubscriptions();
			if (subscriptions.Contains(section))
				return;

			_childCollectionChangedHandler ??= ChildCollectionChanged;
			_childPropertyChangedHandler ??= ChildPropertyChanged;
			subscriptions.Add(section, _childCollectionChangedHandler, _childPropertyChangedHandler);
		}

		void UnsubscribeFromSection(TableSection section)
		{
			if (Contains(section))
				return;

			_sectionSubscriptions?.Remove(section);
		}

		sealed class SectionSubscriptions
		{
			readonly List<SectionSubscription> _subscriptions = new();

			// Keep finalization private so TableRoot does not gain a public API finalizer entry.
			~SectionSubscriptions() => UnsubscribeAll();

			public bool Contains(TableSection source)
			{
				for (int i = 0; i < _subscriptions.Count; i++)
				{
					if (_subscriptions[i].IsSubscribedTo(source))
						return true;
				}

				return false;
			}

			public void Add(TableSection source, NotifyCollectionChangedEventHandler collectionChangedHandler, PropertyChangedEventHandler propertyChangedHandler)
			{
				_subscriptions.Add(new SectionSubscription(source, collectionChangedHandler, propertyChangedHandler));
			}

			public void Remove(TableSection source)
			{
				for (int i = _subscriptions.Count - 1; i >= 0; i--)
				{
					var subscription = _subscriptions[i];
					if (subscription.IsSubscribedTo(source))
					{
						subscription.Unsubscribe();
						_subscriptions.RemoveAt(i);

						break;
					}
				}
			}

			public void UnsubscribeAll()
			{
				for (int i = 0; i < _subscriptions.Count; i++)
					_subscriptions[i].Unsubscribe();

				_subscriptions.Clear();
			}
		}

		sealed class SectionSubscription
		{
			readonly WeakNotifyCollectionChangedProxy _collectionChangedProxy;
			readonly WeakNotifyPropertyChangedProxy _propertyChangedProxy;

			public SectionSubscription(TableSection source, NotifyCollectionChangedEventHandler collectionChangedHandler, PropertyChangedEventHandler propertyChangedHandler)
			{
				_collectionChangedProxy = new WeakNotifyCollectionChangedProxy(source, collectionChangedHandler);
				_propertyChangedProxy = new WeakNotifyPropertyChangedProxy(source, propertyChangedHandler);
			}

			public bool IsSubscribedTo(TableSection source)
			{
				return _collectionChangedProxy.TryGetSource(out var proxySource) && ReferenceEquals(proxySource, source);
			}

			public void Unsubscribe()
			{
				_collectionChangedProxy.Unsubscribe();
				_propertyChangedProxy.Unsubscribe();
			}
		}
	}
}
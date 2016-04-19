using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Xamarin.Forms
{
	public abstract class ItemsView : View, IItemViewController
	{
		public static readonly BindableProperty ItemsSourceProperty =
			BindableProperty.Create(
				propertyName: "ItemsSource",
				returnType: typeof(IEnumerable),
				declaringType: typeof(ItemsView),
				defaultValue: Enumerable.Empty<object>()
			);

		public static readonly BindableProperty ItemTemplateProperty =
			BindableProperty.Create(
				propertyName: "ItemTemplate",
				returnType: typeof(DataTemplate),
				declaringType: typeof(ItemsView)
			);

		ItemSource _itemSource;

		internal ItemsView()
		{
		}

		public IEnumerable ItemsSource
		{
			get
			{
				return (IEnumerable)GetValue(ItemsSourceProperty);
			}
			set
			{
				SetValue(ItemsSourceProperty, value);
			}
		}

		public DataTemplate ItemTemplate
		{
			get
			{
				return (DataTemplate)GetValue(ItemTemplateProperty);
			}
			set
			{
				SetValue(ItemTemplateProperty, value);
			}
		}

		int IItemViewController.Count => _itemSource.Count;

		void IItemViewController.BindView(View view, object item)
		{
			view.BindingContext = item;
		}

		View IItemViewController.CreateView(object type)
		{
			var dataTemplate = (DataTemplate)type;
			object content = dataTemplate.CreateContent();
			var view = (View)content;
			view.Parent = this;
			return view;
		}

		object IItemViewController.GetItem(int index) => _itemSource[index];

		object IItemViewController.GetItemType(object item)
		{
			DataTemplate dataTemplate = ItemTemplate;
			var dataTemplateSelector = dataTemplate as DataTemplateSelector;
			if (dataTemplateSelector != null)
				dataTemplate = dataTemplateSelector.SelectTemplate(item, this);

			if (item == null)
				throw new ArgumentException($"No DataTemplate resolved for item: {item}.");

			return dataTemplate;
		}

		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			if (propertyName == nameof(ItemsSource))
			{
				var itemsSource = ItemsSource;
				if (itemsSource == null)
					itemsSource = Enumerable.Empty<object>();

				// abstract enumerable, IList, IList<T>, and IReadOnlyList<T>
				_itemSource = new ItemSource(itemsSource);

				// subscribe to collection changed events
				var dynamicItemSource = _itemSource as INotifyCollectionChanged;
				if (dynamicItemSource != null)
				{
					new WeakNotifyCollectionChanged(this, dynamicItemSource);
				}
			}

			base.OnPropertyChanged(propertyName);
		}

		internal event NotifyCollectionChangedEventHandler CollectionChanged;

		internal void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			CollectionChanged?.Invoke(sender, e);
		}

		sealed class WeakNotifyCollectionChanged
		{
			readonly WeakReference<INotifyCollectionChanged> _weakCollection;
			readonly WeakReference<ItemsView> _weakSource;

			public WeakNotifyCollectionChanged(ItemsView source, INotifyCollectionChanged incc)
			{
				incc.CollectionChanged += OnCollectionChanged;

				_weakSource = new WeakReference<ItemsView>(source);
				_weakCollection = new WeakReference<INotifyCollectionChanged>(incc);
			}

			void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
			{
				ItemsView source;
				if (!_weakSource.TryGetTarget(out source))
				{
					INotifyCollectionChanged collection;
					if (_weakCollection.TryGetTarget(out collection))
						collection.CollectionChanged -= OnCollectionChanged;

					return;
				}

				source.OnCollectionChanged(sender, e);
			}
		}

		sealed class ItemSource : IEnumerable<object>, INotifyCollectionChanged
		{
			IndexableCollection _indexable;

			internal ItemSource(IEnumerable enumerable)
			{
				_indexable = new IndexableCollection(enumerable);
				var dynamicItemSource = enumerable as INotifyCollectionChanged;
				if (dynamicItemSource != null)
					dynamicItemSource.CollectionChanged += OnCollectionChanged;
			}

			public int Count => _indexable.Count;

			public IEnumerable Enumerable => _indexable.Enumerable;

			public object this[int index]
			{
				get
				{
					// madness ported from listProxy
					CollectionSynchronizationContext syncContext = SyncContext;
					if (syncContext != null)
					{
						object value = null;
						syncContext.Callback(Enumerable, SyncContext.Context, () => value = _indexable[index], false);

						return value;
					}

					return _indexable[index];
				}
			}

			CollectionSynchronizationContext SyncContext
			{
				get
				{
					CollectionSynchronizationContext syncContext;
					BindingBase.TryGetSynchronizedCollection(Enumerable, out syncContext);
					return syncContext;
				}
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			IEnumerator<object> IEnumerable<object>.GetEnumerator()
			{
				return GetEnumerator();
			}

			public event NotifyCollectionChangedEventHandler CollectionChanged;

			public Enumerator GetEnumerator()
			{
				return new Enumerator(this);
			}

			void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
			{
				Action onCollectionChanged = () =>
				{
					if (CollectionChanged != null)
						CollectionChanged(this, e);
				};

				// madness ported from listProxy
				CollectionSynchronizationContext syncContext = SyncContext;
				if (syncContext != null)
				{
					syncContext.Callback(Enumerable, syncContext.Context, () => Device.BeginInvokeOnMainThread(onCollectionChanged), false);
				}

				else if (Device.IsInvokeRequired)
					Device.BeginInvokeOnMainThread(onCollectionChanged);

				else
					onCollectionChanged();
			}

			internal struct Enumerator : IEnumerator<object>
			{
				readonly ItemSource _itemSource;
				int _index;

				internal Enumerator(ItemSource itemSource) : this()
				{
					_itemSource = itemSource;
				}

				public bool MoveNext()
				{
					if (_index == _itemSource.Count)
						return false;

					Current = _itemSource[_index++];
					return true;
				}

				public object Current
				{
					get; private set;
				}

				public void Reset()
				{
					Current = null;
					_index = 0;
				}

				public void Dispose()
				{
				}
			}

			struct IndexableCollection : IEnumerable<object>
			{
				internal IndexableCollection(IEnumerable list)
				{
					Enumerable = list;

					if (list is IList)
						return;

					if (list is IList<object>)
						return;

					if (list is IReadOnlyList<object>)
						return;

					Enumerable = list.Cast<object>().ToArray();
				}

				internal IEnumerable Enumerable
				{
					get;
				}

				internal int Count
				{
					get
					{
						var list = Enumerable as IList;
						if (list != null)
							return list.Count;

						var listOf = Enumerable as IList<object>;
						if (listOf != null)
							return listOf.Count;

						var readOnlyList = (IReadOnlyList<object>)Enumerable;
						return readOnlyList.Count;
					}
				}

				internal object this[int index]
				{
					get
					{
						var list = Enumerable as IList;
						if (list != null)
							return list[index];

						var listOf = Enumerable as IList<object>;
						if (listOf != null)
							return listOf[index];

						var readOnlyList = (IReadOnlyList<object>)Enumerable;
						return readOnlyList[index];
					}
				}

				internal int IndexOf(object item)
				{
					var list = Enumerable as IList;
					if (list != null)
						return list.IndexOf(item);

					var listOf = Enumerable as IList<object>;
					if (listOf != null)
						return listOf.IndexOf(item);

					var readOnlyList = (IReadOnlyList<object>)Enumerable;
					return readOnlyList.IndexOf(item);
				}

				public IEnumerator<object> GetEnumerator()
				{
					var list = Enumerable as IList;
					if (list != null)
						return list.Cast<object>().GetEnumerator();

					var listOf = Enumerable as IList<object>;
					if (listOf != null)
						return listOf.GetEnumerator();

					var readOnlyList = (IReadOnlyList<object>)Enumerable;
					return readOnlyList.GetEnumerator();
				}

				IEnumerator IEnumerable.GetEnumerator()
				{
					return GetEnumerator();
				}
			}
		}
	}
}
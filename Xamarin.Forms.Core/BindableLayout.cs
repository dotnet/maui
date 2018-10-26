using System;
using System.Collections;
using System.Collections.Specialized;

namespace Xamarin.Forms
{
	public static class BindableLayout
	{
		public static readonly BindableProperty ItemsSourceProperty =
			BindableProperty.CreateAttached("ItemsSource", typeof(IEnumerable), typeof(Layout<View>), default(IEnumerable),
				propertyChanged: (b, o, n) => { GetBindableLayoutController(b).ItemsSource = (IEnumerable)n; });

		public static readonly BindableProperty ItemTemplateProperty =
			BindableProperty.CreateAttached("ItemTemplate", typeof(DataTemplate), typeof(Layout<View>), default(DataTemplate),
				propertyChanged: (b, o, n) => { GetBindableLayoutController(b).ItemTemplate = (DataTemplate)n; });

		public static readonly BindableProperty ItemTemplateSelectorProperty =
			BindableProperty.CreateAttached("ItemTemplateSelector", typeof(DataTemplateSelector), typeof(Layout<View>), default(DataTemplateSelector),
				propertyChanged: (b, o, n) => { GetBindableLayoutController(b).ItemTemplateSelector = (DataTemplateSelector)n; });

		static readonly BindableProperty BindableLayoutControllerProperty =
			 BindableProperty.CreateAttached("BindableLayoutController", typeof(BindableLayoutController), typeof(Layout<View>), default(BindableLayoutController),
				 defaultValueCreator: (b) => new BindableLayoutController((Layout<View>)b),
				 propertyChanged: (b, o, n) => OnControllerChanged(b, (BindableLayoutController)o, (BindableLayoutController)n));

		public static void SetItemsSource(BindableObject b, IEnumerable value)
		{
			b.SetValue(ItemsSourceProperty, value);
		}

		public static IEnumerable GetItemsSource(BindableObject b)
		{
			return (IEnumerable)b.GetValue(ItemsSourceProperty);
		}

		public static void SetItemTemplate(BindableObject b, DataTemplate value)
		{
			b.SetValue(ItemTemplateProperty, value);
		}

		public static DataTemplate GetItemTemplate(BindableObject b)
		{
			return (DataTemplate)b.GetValue(ItemTemplateProperty);
		}

		public static void SetItemTemplateSelector(BindableObject b, DataTemplateSelector value)
		{
			b.SetValue(ItemTemplateSelectorProperty, value);
		}

		public static DataTemplateSelector GetItemTemplateSelector(BindableObject b)
		{
			return (DataTemplateSelector)b.GetValue(ItemTemplateSelectorProperty);
		}

		static BindableLayoutController GetBindableLayoutController(BindableObject b)
		{
			return (BindableLayoutController)b.GetValue(BindableLayoutControllerProperty);
		}

		static void SetBindableLayoutController(BindableObject b, BindableLayoutController value)
		{
			b.SetValue(BindableLayoutControllerProperty, value);
		}

		static void OnControllerChanged(BindableObject b, BindableLayoutController oldC, BindableLayoutController newC)
		{
			if (oldC != null)
			{
				oldC.ItemsSource = null;
			}

			if (newC == null)
			{
				return;
			}

			newC.StartBatchUpdate();
			newC.ItemsSource = GetItemsSource(b);
			newC.ItemTemplate = GetItemTemplate(b);
			newC.ItemTemplateSelector = GetItemTemplateSelector(b);
			newC.EndBatchUpdate();
		}
	}

	class BindableLayoutController
	{
		readonly WeakReference<Layout<View>> _layoutWeakReference;
		IEnumerable _itemsSource;
		DataTemplate _itemTemplate;
		DataTemplateSelector _itemTemplateSelector;
		bool _isBatchUpdate;

		public IEnumerable ItemsSource { get => _itemsSource; set => SetItemsSource(value); }
		public DataTemplate ItemTemplate { get => _itemTemplate; set => SetItemTemplate(value); }
		public DataTemplateSelector ItemTemplateSelector { get => _itemTemplateSelector; set => SetItemTemplateSelector(value); }


		public BindableLayoutController(Layout<View> layout)
		{
			_layoutWeakReference = new WeakReference<Layout<View>>(layout);
		}

		internal void StartBatchUpdate()
		{
			_isBatchUpdate = true;
		}

		internal void EndBatchUpdate()
		{
			_isBatchUpdate = false;
			CreateChildren();
		}

		void SetItemsSource(IEnumerable itemsSource)
		{
			if (_itemsSource is INotifyCollectionChanged c)
			{
				c.CollectionChanged -= ItemsSourceCollectionChanged;
			}

			_itemsSource = itemsSource;

			if (_itemsSource is INotifyCollectionChanged c1)
			{
				c1.CollectionChanged += ItemsSourceCollectionChanged;
			}

			if (!_isBatchUpdate)
			{
				CreateChildren();
			}
		}

		void SetItemTemplate(DataTemplate itemTemplate)
		{
			if (itemTemplate is DataTemplateSelector)
			{
				throw new NotSupportedException($"You are using an instance of {nameof(DataTemplateSelector)} to set the {nameof(BindableLayout)}.{BindableLayout.ItemTemplateProperty.PropertyName} property. Use {nameof(BindableLayout)}.{BindableLayout.ItemTemplateSelectorProperty.PropertyName} property instead to set an item template selector");
			}

			_itemTemplate = itemTemplate;

			if (!_isBatchUpdate)
			{
				CreateChildren();
			}
		}

		void SetItemTemplateSelector(DataTemplateSelector itemTemplateSelector)
		{
			_itemTemplateSelector = itemTemplateSelector;

			if (!_isBatchUpdate)
			{
				CreateChildren();
			}
		}

		void CreateChildren()
		{
			Layout<View> layout;
			if (!_layoutWeakReference.TryGetTarget(out layout))
			{
				return;
			}

			layout.Children.Clear();

			if (_itemsSource == null)
			{
				return;
			}

			int i = 0;
			foreach (object item in _itemsSource)
			{
				layout.Children.Add(CreateItemView(item, i++));
			}
		}

		View CreateItemView(object item, int index)
		{
			return CreateItemView(item, index, _itemTemplate ?? _itemTemplateSelector?.SelectTemplate(item, null));
		}

		View CreateItemView(object item, int index, DataTemplate dataTemplate)
		{
			if (dataTemplate != null)
			{
				var view = (View)dataTemplate.CreateContent();
				view.BindingContext = item;
				return view;
			}
			else
			{
				return new Label() { Text = item?.ToString() };
			}
		}

		void ItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			Layout<View> layout;
			if (!_layoutWeakReference.TryGetTarget(out layout))
			{
				return;
			}

			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					{
						if (e.NewStartingIndex == -1)
							goto case NotifyCollectionChangedAction.Reset;
						int i = e.NewStartingIndex;
						foreach (object item in e.NewItems)
						{
							layout.Children.Add(CreateItemView(item, i++));
						}
					}
					break;
				case NotifyCollectionChangedAction.Remove:
					{
						if (e.OldStartingIndex == -1)
							goto case NotifyCollectionChangedAction.Reset;
						for (int i = 0; i < e.OldItems.Count; ++i)
						{
							layout.Children.RemoveAt(i + e.OldStartingIndex);
						}
					}
					break;
				case NotifyCollectionChangedAction.Replace:
					{
						if (e.OldStartingIndex == -1)
							goto case NotifyCollectionChangedAction.Reset;
						int i = e.NewStartingIndex;
						foreach (object item in e.NewItems)
						{
							layout.Children[i] = CreateItemView(item, i);
							++i;
						}
					}
					break;
				case NotifyCollectionChangedAction.Move:
					{
						if (e.OldStartingIndex == -1 || e.NewStartingIndex == -1)
							goto case NotifyCollectionChangedAction.Reset;
						for (int i = 0; i < e.NewItems.Count; ++i)
						{
							int iFrom = e.OldStartingIndex + i;
							int iTo = e.NewStartingIndex + i;
							View fromView = layout.Children[iFrom];
							View toView = layout.Children[iTo];
							layout.Children.Remove(fromView);
							layout.Children.Remove(toView);
							layout.Children.Insert(iFrom, toView);
							layout.Children.Insert(iTo, fromView);
						}
					}
					break;

				case NotifyCollectionChangedAction.Reset:
					layout.Children.Clear();
					break;
			}
		}
	}
}

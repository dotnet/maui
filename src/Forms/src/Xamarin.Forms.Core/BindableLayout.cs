using System;
using System.Collections;
using System.Collections.Specialized;
using Xamarin.Forms.Internals;

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

		public static readonly BindableProperty EmptyViewProperty =
			BindableProperty.Create("EmptyView", typeof(object), typeof(Layout<View>), null, propertyChanged: (b, o, n) => { GetBindableLayoutController(b).EmptyView = n; });

		public static readonly BindableProperty EmptyViewTemplateProperty =
			BindableProperty.Create("EmptyViewTemplate", typeof(DataTemplate), typeof(Layout<View>), null, propertyChanged: (b, o, n) => { GetBindableLayoutController(b).EmptyViewTemplate = (DataTemplate)n; });

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

		public static object GetEmptyView(BindableObject b)
		{
			return b.GetValue(EmptyViewProperty);
		}

		public static void SetEmptyView(BindableObject b, object value)
		{
			b.SetValue(EmptyViewProperty, value);
		}

		public static DataTemplate GetEmptyViewTemplate(BindableObject b)
		{
			return (DataTemplate)b.GetValue(EmptyViewTemplateProperty);
		}

		public static void SetEmptyViewTemplate(BindableObject b, DataTemplate value)
		{
			b.SetValue(EmptyViewProperty, value);
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
			newC.EmptyView = GetEmptyView(b);
			newC.EmptyViewTemplate = GetEmptyViewTemplate(b);
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
		object _emptyView;
		DataTemplate _emptyViewTemplate;
		View _currentEmptyView;

		public IEnumerable ItemsSource { get => _itemsSource; set => SetItemsSource(value); }
		public DataTemplate ItemTemplate { get => _itemTemplate; set => SetItemTemplate(value); }
		public DataTemplateSelector ItemTemplateSelector { get => _itemTemplateSelector; set => SetItemTemplateSelector(value); }

		public object EmptyView { get => _emptyView; set => SetEmptyView(value); }
		public DataTemplate EmptyViewTemplate { get => _emptyViewTemplate; set => SetEmptyViewTemplate(value); }

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

		void SetEmptyView(object emptyView)
		{
			_emptyView = emptyView;

			_currentEmptyView = CreateEmptyView(_emptyView, _emptyViewTemplate);

			if (!_isBatchUpdate)
			{
				CreateChildren();
			}
		}

		void SetEmptyViewTemplate(DataTemplate emptyViewTemplate)
		{
			_emptyViewTemplate = emptyViewTemplate;

			_currentEmptyView = CreateEmptyView(_emptyView, _emptyViewTemplate);

			if (!_isBatchUpdate)
			{
				CreateChildren();
			}
		}

		void CreateChildren()
		{
			if (!_layoutWeakReference.TryGetTarget(out Layout<View> layout))
			{
				return;
			}

			layout.Children.Clear();

			UpdateEmptyView(layout);

			if (_itemsSource == null)
				return;

			foreach (object item in _itemsSource)
			{
				layout.Children.Add(CreateItemView(item, layout));
			}
		}

		void UpdateEmptyView(Layout<View> layout)
		{
			if (_currentEmptyView == null)
				return;

			if (!_itemsSource?.GetEnumerator().MoveNext() ?? true)
			{
				layout.Children.Add(_currentEmptyView);
				return;
			}

			layout.Children.Remove(_currentEmptyView);
		}

		View CreateItemView(object item, Layout<View> layout)
		{
			return CreateItemView(item, _itemTemplate ?? _itemTemplateSelector?.SelectTemplate(item, layout));
		}

		View CreateItemView(object item, DataTemplate dataTemplate)
		{
			if (dataTemplate != null)
			{
				var view = (View)dataTemplate.CreateContent();
				view.BindingContext = item;
				return view;
			}
			else
			{
				return new Label { Text = item?.ToString(), HorizontalTextAlignment = TextAlignment.Center };
			}
		}

		View CreateEmptyView(object emptyView, DataTemplate dataTemplate)
		{
			if (!_layoutWeakReference.TryGetTarget(out Layout<View> layout))
			{
				return null;
			}

			if (dataTemplate != null)
			{
				var view = (View)dataTemplate.CreateContent();
				view.BindingContext = layout.BindingContext;
				return view;
			}

			if (emptyView is View emptyLayout)
			{
				return emptyLayout;
			}

			return new Label { Text = emptyView?.ToString(), HorizontalTextAlignment = TextAlignment.Center };
		}

		void ItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (!_layoutWeakReference.TryGetTarget(out Layout<View> layout))
			{
				return;
			}

			e.Apply(
				insert: (item, index, _) => layout.Children.Insert(index, CreateItemView(item, layout)),
				removeAt: (item, index) => layout.Children.RemoveAt(index),
				reset: CreateChildren);

			// UpdateEmptyView is called from within CreateChildren, therefor skip it for Reset
			if (e.Action != NotifyCollectionChangedAction.Reset)
				UpdateEmptyView(layout);
		}
	}
}
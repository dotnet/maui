#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	// TODO ezhart 2021-07-16 This interface is just here to give Layout and Compatibility.Layout common ground for BindableLayout
	// once we have the IContainer changes in, we may be able to drop this in favor of simply Core.ILayout
	// See also IndicatorView.cs 
	public interface IBindableLayout
	{
		public IList Children { get; }
	}

	/// <include file="../../docs/Microsoft.Maui.Controls/BindableLayout.xml" path="Type[@FullName='Microsoft.Maui.Controls.BindableLayout']/Docs/*" />
	public static class BindableLayout
	{
		/// <summary>Bindable property for attached property <c>ItemsSource</c>.</summary>
		public static readonly BindableProperty ItemsSourceProperty =
			BindableProperty.CreateAttached("ItemsSource", typeof(IEnumerable), typeof(IBindableLayout), default(IEnumerable),
				propertyChanged: (b, o, n) => { GetBindableLayoutController(b).ItemsSource = (IEnumerable)n; });

		/// <summary>Bindable property for attached property <c>ItemTemplate</c>.</summary>
		public static readonly BindableProperty ItemTemplateProperty =
			BindableProperty.CreateAttached("ItemTemplate", typeof(DataTemplate), typeof(IBindableLayout), default(DataTemplate),
				propertyChanged: (b, o, n) => { GetBindableLayoutController(b).ItemTemplate = (DataTemplate)n; });

		/// <summary>Bindable property for attached property <c>ItemTemplateSelector</c>.</summary>
		public static readonly BindableProperty ItemTemplateSelectorProperty =
			BindableProperty.CreateAttached("ItemTemplateSelector", typeof(DataTemplateSelector), typeof(IBindableLayout), default(DataTemplateSelector),
				propertyChanged: (b, o, n) => { GetBindableLayoutController(b).ItemTemplateSelector = (DataTemplateSelector)n; });

		static readonly BindableProperty BindableLayoutControllerProperty =
			 BindableProperty.CreateAttached("BindableLayoutController", typeof(BindableLayoutController), typeof(IBindableLayout), default(BindableLayoutController),
				 defaultValueCreator: (b) => new BindableLayoutController((IBindableLayout)b),
				 propertyChanged: (b, o, n) => OnControllerChanged(b, (BindableLayoutController)o, (BindableLayoutController)n));

		/// <summary>Bindable property for attached property <c>EmptyView</c>.</summary>
		public static readonly BindableProperty EmptyViewProperty =
			BindableProperty.Create("EmptyView", typeof(object), typeof(IBindableLayout), null, propertyChanged: (b, o, n) => { GetBindableLayoutController(b).EmptyView = n; });

		/// <summary>Bindable property for attached property <c>EmptyViewTemplate</c>.</summary>
		public static readonly BindableProperty EmptyViewTemplateProperty =
			BindableProperty.Create("EmptyViewTemplate", typeof(DataTemplate), typeof(IBindableLayout), null, propertyChanged: (b, o, n) => { GetBindableLayoutController(b).EmptyViewTemplate = (DataTemplate)n; });

		/// <include file="../../docs/Microsoft.Maui.Controls/BindableLayout.xml" path="//Member[@MemberName='SetItemsSource']/Docs/*" />
		public static void SetItemsSource(BindableObject b, IEnumerable value)
		{
			b.SetValue(ItemsSourceProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/BindableLayout.xml" path="//Member[@MemberName='GetItemsSource']/Docs/*" />
		public static IEnumerable GetItemsSource(BindableObject b)
		{
			return (IEnumerable)b.GetValue(ItemsSourceProperty);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/BindableLayout.xml" path="//Member[@MemberName='SetItemTemplate']/Docs/*" />
		public static void SetItemTemplate(BindableObject b, DataTemplate value)
		{
			b.SetValue(ItemTemplateProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/BindableLayout.xml" path="//Member[@MemberName='GetItemTemplate']/Docs/*" />
		public static DataTemplate GetItemTemplate(BindableObject b)
		{
			return (DataTemplate)b.GetValue(ItemTemplateProperty);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/BindableLayout.xml" path="//Member[@MemberName='SetItemTemplateSelector']/Docs/*" />
		public static void SetItemTemplateSelector(BindableObject b, DataTemplateSelector value)
		{
			b.SetValue(ItemTemplateSelectorProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/BindableLayout.xml" path="//Member[@MemberName='GetItemTemplateSelector']/Docs/*" />
		public static DataTemplateSelector GetItemTemplateSelector(BindableObject b)
		{
			return (DataTemplateSelector)b.GetValue(ItemTemplateSelectorProperty);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/BindableLayout.xml" path="//Member[@MemberName='GetEmptyView']/Docs/*" />
		public static object GetEmptyView(BindableObject b)
		{
			return b.GetValue(EmptyViewProperty);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/BindableLayout.xml" path="//Member[@MemberName='SetEmptyView']/Docs/*" />
		public static void SetEmptyView(BindableObject b, object value)
		{
			b.SetValue(EmptyViewProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/BindableLayout.xml" path="//Member[@MemberName='GetEmptyViewTemplate']/Docs/*" />
		public static DataTemplate GetEmptyViewTemplate(BindableObject b)
		{
			return (DataTemplate)b.GetValue(EmptyViewTemplateProperty);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/BindableLayout.xml" path="//Member[@MemberName='SetEmptyViewTemplate']/Docs/*" />
		public static void SetEmptyViewTemplate(BindableObject b, DataTemplate value)
		{
			b.SetValue(EmptyViewTemplateProperty, value);
		}

		internal static BindableLayoutController GetBindableLayoutController(BindableObject b)
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

		internal static void Add(this IBindableLayout layout, object item)
		{
			if (layout is Maui.ILayout mauiLayout && item is IView view)
			{
				mauiLayout.Add(view);
			}
			else
			{
				_ = layout.Children.Add(item);
			}
		}

		internal static void Insert(this IBindableLayout layout, object item, int index)
		{
			if (layout is Maui.ILayout mauiLayout && item is IView view)
			{
				mauiLayout.Insert(index, view);
			}
			else
			{
				layout.Children.Insert(index, item);
			}
		}

		internal static void Remove(this IBindableLayout layout, object item)
		{
			if (layout is Maui.ILayout mauiLayout && item is IView view)
			{
				_ = mauiLayout.Remove(view);
			}
			else
			{
				layout.Children.Remove(item);
			}
		}

		internal static void RemoveAt(this IBindableLayout layout, int index)
		{
			if (layout is Maui.ILayout mauiLayout)
			{
				mauiLayout.RemoveAt(index);
			}
			else
			{
				layout.Children.RemoveAt(index);
			}
		}

		internal static void Clear(this IBindableLayout layout)
		{
			if (layout is Maui.ILayout mauiLayout)
			{
				mauiLayout.Clear();
			}
			else
			{
				layout.Children.Clear();
			}
		}
	}

	class BindableLayoutController
	{
		readonly WeakReference<IBindableLayout> _layoutWeakReference;
		readonly WeakNotifyCollectionChangedProxy _collectionChangedProxy = new();
		readonly NotifyCollectionChangedEventHandler _collectionChangedEventHandler;
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

		public BindableLayoutController(IBindableLayout layout)
		{
			_layoutWeakReference = new WeakReference<IBindableLayout>(layout);
			_collectionChangedEventHandler = ItemsSourceCollectionChanged;
		}

		~BindableLayoutController() => _collectionChangedProxy.Unsubscribe();

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
			if (_itemsSource is INotifyCollectionChanged)
			{
				_collectionChangedProxy.Unsubscribe();
			}

			_itemsSource = itemsSource;

			if (_itemsSource is INotifyCollectionChanged c)
			{
				_collectionChangedProxy.Subscribe(c, _collectionChangedEventHandler);
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
			if (!_layoutWeakReference.TryGetTarget(out IBindableLayout layout))
			{
				return;
			}

			ClearChildren(layout);

			UpdateEmptyView(layout);

			if (_itemsSource == null)
				return;

			foreach (object item in _itemsSource)
			{
				layout.Add(CreateItemView(item, layout));
			}
		}

		void ClearChildren(IBindableLayout layout)
		{
			var index = layout.Children.Count;
			while (--index >= 0)
			{
				var child = (View)layout.Children[index]!;
				layout.RemoveAt(index);

				// Empty view inherits the BindingContext automatically,
				// we don't want to mess up with automatic inheritance.
				if (child == _currentEmptyView) continue;
				
				// Given that we've set BindingContext manually on children we have to clear it on removal.
				child.BindingContext = null;
			}
		}

		void UpdateEmptyView(IBindableLayout layout)
		{
			if (_currentEmptyView == null)
				return;

			if (!_itemsSource?.GetEnumerator().MoveNext() ?? true)
			{
				layout.Add(_currentEmptyView);
				return;
			}

			layout.Remove(_currentEmptyView);
		}

		View CreateItemView(object item, IBindableLayout layout)
		{
			return CreateItemView(item, _itemTemplate ?? _itemTemplateSelector?.SelectTemplate(item, layout as BindableObject));
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
			if (!_layoutWeakReference.TryGetTarget(out IBindableLayout layout))
			{
				return null;
			}

			if (dataTemplate != null)
			{
				var view = (View)dataTemplate.CreateContent();
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
			if (!_layoutWeakReference.TryGetTarget(out IBindableLayout layout))
			{
				return;
			}

			e.Apply(
				insert: (item, index, _) => layout.Insert(CreateItemView(item, layout), index),
				removeAt: (item, index) =>
				{
					var child = (View)layout.Children[index]!;
					layout.RemoveAt(index);
					
					// It's our responsibility to clear the BindingContext for the children
					// Given that we've set them manually in CreateItemView
					child.BindingContext = null;
				},
				reset: CreateChildren);

			// UpdateEmptyView is called from within CreateChildren, therefor skip it for Reset
			if (e.Action != NotifyCollectionChangedAction.Reset)
				UpdateEmptyView(layout);
		}
	}
}
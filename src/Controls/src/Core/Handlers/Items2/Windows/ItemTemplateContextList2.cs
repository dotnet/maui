using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	internal class ItemTemplateContextList2 : IReadOnlyList<ItemTemplateContext2>
	{
		readonly IList _itemsSource;
		readonly DataTemplate _itemTemplate;
		readonly BindableObject _container;
		readonly IMauiContext? _mauiContext;
		readonly double _itemHeight;
		readonly double _itemWidth;
		readonly Thickness _itemSpacing;

		readonly Dictionary<int, ItemTemplateContext2> _itemTemplateContexts;

		public int Count => _itemsSource.Count;

		public ItemTemplateContext2 this[int index]
		{
			get
			{
				if (!_itemTemplateContexts.TryGetValue(index, out var context))
				{
					var item = _itemsSource[index];
					if (item is null)
						throw new InvalidOperationException($"Item at index {index} is null. ItemTemplateContext2 requires a non-null item.");
					
					_itemTemplateContexts[index] = context = new ItemTemplateContext2(_itemTemplate, item,
						_container, _itemHeight, _itemWidth, _itemSpacing, false, false, _mauiContext);
				}

				return context;
			}
		}

		public ItemTemplateContextList2(IList itemsSource, DataTemplate itemTemplate, BindableObject container,
			double? itemHeight = null, double? itemWidth = null, Thickness? itemSpacing = null, IMauiContext? mauiContext = null)
		{
			_itemsSource = itemsSource;
			_itemTemplate = itemTemplate;
			_container = container;
			_mauiContext = mauiContext;
			if (itemHeight.HasValue)
				_itemHeight = itemHeight.Value;

			if (itemWidth.HasValue)
				_itemWidth = itemWidth.Value;

			if (itemSpacing.HasValue)
				_itemSpacing = itemSpacing.Value;

			_itemTemplateContexts = new(capacity: Math.Min(64, _itemsSource.Count));
		}

		public IEnumerator<ItemTemplateContext2> GetEnumerator()
		{
			return new ItemTemplateContextListEnumerator2(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

#nullable disable
		internal class ItemTemplateContextListEnumerator2 : IEnumerator<ItemTemplateContext2>
		{
			public ItemTemplateContext2 Current { get; private set; }
			object IEnumerator.Current => Current;
			int _currentIndex = -1;
			private ItemTemplateContextList2 _itemTemplateContextList;

			public ItemTemplateContextListEnumerator2(ItemTemplateContextList2 observableItemTemplateCollection)
			{
				_itemTemplateContextList = observableItemTemplateCollection;
			}

			public void Dispose()
			{
			}

			public bool MoveNext()
			{
				if (_currentIndex >= _itemTemplateContextList.Count - 1)
				{
					return false;
				}

				_currentIndex += 1;
				Current = _itemTemplateContextList[_currentIndex];

				return true;
			}

			public void Reset()
			{
				Current = null;
				_currentIndex = -1;
			}
		}
#nullable enable
	}
}
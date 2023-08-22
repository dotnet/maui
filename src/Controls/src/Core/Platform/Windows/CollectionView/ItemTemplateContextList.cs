#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls.Platform
{
	internal class ItemTemplateContextList : IReadOnlyList<ItemTemplateContext>
	{
		readonly IList _itemsSource;
		readonly DataTemplate _itemTemplate;
		readonly BindableObject _container;
		readonly IMauiContext _mauiContext;
		readonly double _itemHeight;
		readonly double _itemWidth;
		readonly Thickness _itemSpacing;

		readonly Dictionary<int, ItemTemplateContext> _itemTemplateContexts;

		public int Count => _itemsSource.Count;

		public ItemTemplateContext this[int index]
		{
			get
			{
				if (!_itemTemplateContexts.TryGetValue(index, out var context))
				{
					_itemTemplateContexts[index] = context = new ItemTemplateContext(_itemTemplate, _itemsSource[index],
						_container, _itemHeight, _itemWidth, _itemSpacing, _mauiContext);
				}

				return context;
			}
		}

		public ItemTemplateContextList(IList itemsSource, DataTemplate itemTemplate, BindableObject container,
			double? itemHeight = null, double? itemWidth = null, Thickness? itemSpacing = null, IMauiContext mauiContext = null)
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

		public IEnumerator<ItemTemplateContext> GetEnumerator()
		{
			return new ItemTemplateContextListEnumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		internal class ItemTemplateContextListEnumerator : IEnumerator<ItemTemplateContext>
		{
			public ItemTemplateContext Current { get; private set; }
			object IEnumerator.Current => Current;
			int _currentIndex = -1;
			private ItemTemplateContextList _itemTemplateContextList;

			public ItemTemplateContextListEnumerator(ItemTemplateContextList observableItemTemplateCollection) =>
				_itemTemplateContextList = observableItemTemplateCollection;

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
	}
}
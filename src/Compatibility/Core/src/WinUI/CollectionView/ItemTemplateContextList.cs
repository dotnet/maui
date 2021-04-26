using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	internal class ItemTemplateContextList : IReadOnlyList<ItemTemplateContext>
	{
		readonly IList _itemsSource;
		readonly DataTemplate _itemTemplate;
		readonly BindableObject _container;
		readonly double _itemHeight;
		readonly double _itemWidth;
		readonly Thickness _itemSpacing;

		readonly List<ItemTemplateContext> _itemTemplateContexts;

		public int Count => _itemsSource.Count;

		public ItemTemplateContext this[int index]
		{
			get
			{
				if (_itemTemplateContexts[index] == null)
				{
					_itemTemplateContexts[index] = new ItemTemplateContext(_itemTemplate, _itemsSource[index], 
						_container, _itemHeight, _itemWidth, _itemSpacing);
				}

				return _itemTemplateContexts[index];
			}
		}

		public ItemTemplateContextList(IList itemsSource, DataTemplate itemTemplate, BindableObject container,
			double? itemHeight = null, double? itemWidth = null, Thickness? itemSpacing = null)
		{
			_itemsSource = itemsSource;
			_itemTemplate = itemTemplate;
			_container = container;

			if (itemHeight.HasValue)
				_itemHeight = itemHeight.Value;

			if (itemWidth.HasValue)
				_itemWidth = itemWidth.Value;

			if (itemSpacing.HasValue)
				_itemSpacing = itemSpacing.Value;

			_itemTemplateContexts = new List<ItemTemplateContext>(_itemsSource.Count);

			for (int n = 0; n < _itemsSource.Count; n++)
			{
				_itemTemplateContexts.Add(null);
			}
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
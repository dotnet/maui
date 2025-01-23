#nullable disable
using System.Collections;

namespace Microsoft.Maui.Controls.Platform
{
	internal class ItemTemplateContextEnumerable : IEnumerable
	{
		readonly IEnumerable _itemsSource;
		readonly DataTemplate _formsDataTemplate;
		readonly BindableObject _container;
		readonly IMauiContext _mauiContext;
		readonly double _itemHeight;
		readonly double _itemWidth;
		readonly Thickness _itemSpacing;

		public ItemTemplateContextEnumerable(IEnumerable itemsSource, DataTemplate formsDataTemplate, BindableObject container,
			double? itemHeight = null, double? itemWidth = null, Thickness? itemSpacing = null, IMauiContext mauiContext = null)
		{
			_itemsSource = itemsSource;
			_formsDataTemplate = formsDataTemplate;
			_container = container;
			_mauiContext = mauiContext;
			if (itemHeight.HasValue)
				_itemHeight = itemHeight.Value;

			if (itemWidth.HasValue)
				_itemWidth = itemWidth.Value;

			if (itemSpacing.HasValue)
				_itemSpacing = itemSpacing.Value;
		}

		public IEnumerator GetEnumerator()
		{
			if(_itemsSource is not null)
			{
				foreach (var item in _itemsSource)
				{
					yield return new ItemTemplateContext(_formsDataTemplate, item, _container, _itemHeight, _itemWidth, _itemSpacing, _mauiContext);
				}
			}
		}
	}
}
using System.Collections;

namespace Microsoft.Maui.Controls.Handlers.Items2;
/// <summary>
/// Wraps a non-list <see cref="IEnumerable"/> items source, yielding an
/// <see cref="ItemTemplateContext2"/> for each item on enumeration.
/// Used when the source does not implement <see cref="System.Collections.IList"/>.
/// </summary>
internal class ItemTemplateContextEnumerable2 : IEnumerable
{
	readonly IEnumerable _itemsSource;
	readonly DataTemplate _itemTemplate;
	readonly BindableObject _container;
	readonly IMauiContext? _mauiContext;
	readonly double _itemHeight;
	readonly double _itemWidth;
	readonly Thickness _itemSpacing;

	ItemTemplateContextEnumerable2(IEnumerable itemsSource, DataTemplate itemTemplate, BindableObject container,
		double? itemHeight = null, double? itemWidth = null, Thickness? itemSpacing = null, IMauiContext? mauiContext = null)
	{
		_itemsSource = itemsSource;
		_itemTemplate = itemTemplate;
		_container = container;
		_mauiContext = mauiContext;
		_itemHeight = itemHeight ?? 0;
		_itemWidth = itemWidth ?? 0;
		_itemSpacing = itemSpacing ?? default;
	}

	public IEnumerator GetEnumerator()
	{
		foreach (var item in _itemsSource)
		{
			yield return new ItemTemplateContext2(_itemTemplate, item, _container, _itemHeight, _itemWidth, _itemSpacing,
				false, false, _mauiContext);
		}
	}
}
using System.Collections;

namespace Xamarin.Forms.Platform.UWP
{
	internal class ItemTemplateEnumerator : IEnumerable, IEnumerator
	{
		readonly DataTemplate _formsDataTemplate;
		readonly IEnumerator _innerEnumerator;
		readonly BindableObject _container;
		readonly double _itemHeight;
		readonly double _itemWidth;
		readonly Thickness _itemSpacing;

		public ItemTemplateEnumerator(IEnumerable itemsSource, DataTemplate formsDataTemplate, BindableObject container, double? itemHeight = null, double? itemWidth = null, Thickness? itemSpacing = null)
		{
			_formsDataTemplate = formsDataTemplate;
			_container = container;
			_innerEnumerator = itemsSource.GetEnumerator();

			if (itemHeight.HasValue)
				_itemHeight = itemHeight.Value;

			if (itemWidth.HasValue)
				_itemWidth = itemWidth.Value;

			if (itemSpacing.HasValue)
				_itemSpacing = itemSpacing.Value;
		}

		public IEnumerator GetEnumerator()
		{
			return this;
		}

		public bool MoveNext()
		{
			var moveNext = _innerEnumerator.MoveNext();
			
			if (moveNext)
			{
				Current = new ItemTemplateContext(_formsDataTemplate, _innerEnumerator.Current, _container, _itemHeight, _itemWidth, _itemSpacing);
			}

			return moveNext;
		}

		public void Reset()
		{
			_innerEnumerator.Reset();
		}

		public object Current { get; private set; }
	}
}
namespace Xamarin.Forms.Platform.Android
{
	internal class UngroupedItemsSource : IGroupableItemsViewSource
	{
		readonly IItemsViewSource _source;

		public UngroupedItemsSource(IItemsViewSource source)
		{
			_source = source;
		}

		public int Count => _source.Count;

		public bool HasHeader { get => _source.HasHeader; set => _source.HasHeader = value; }
		public bool HasFooter { get => _source.HasFooter; set => _source.HasFooter = value; }

		public void Dispose()
		{
			_source.Dispose();
		}

		public object GetItem(int position)
		{
			return _source.GetItem(position);
		}

		public int GetPosition(object item)
		{
			return _source.GetPosition(item);
		}

		public bool IsFooter(int position)
		{
			return _source.IsFooter(position);
		}

		public bool IsGroupFooter(int position)
		{
			return false;
		}

		public bool IsGroupHeader(int position)
		{
			return false;
		}

		public bool IsHeader(int position)
		{
			return _source.IsHeader(position);
		}
	}
}
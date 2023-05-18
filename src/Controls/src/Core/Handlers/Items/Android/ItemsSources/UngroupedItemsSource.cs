#nullable disable
using System.Collections;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	internal class UngroupedItemsSource : IGroupableItemsViewSource, IObservableItemsViewSource
	{
		readonly IItemsViewSource _source;

		public UngroupedItemsSource(IItemsViewSource source)
		{
			_source = source;
		}

		public int Count => _source.Count;

		public bool HasHeader { get => _source.HasHeader; set => _source.HasHeader = value; }
		public bool HasFooter { get => _source.HasFooter; set => _source.HasFooter = value; }

		public bool ObserveChanges
		{
			get { return (_source as IObservableItemsViewSource)?.ObserveChanges == true; }
			set
			{
				if (_source is IObservableItemsViewSource observableSource)
				{
					observableSource.ObserveChanges = value;
				}
			}
		}

		public void Dispose()
		{
			_source.Dispose();
		}

		public object GetGroup(int groupIndex)
		{
			return null;
		}

		public (int group, int index) GetGroupAndIndex(int position)
		{
			return (-1, -1);
		}

		public IItemsViewSource GetGroupItemsViewSource(int groupIndex)
		{
			return null;
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

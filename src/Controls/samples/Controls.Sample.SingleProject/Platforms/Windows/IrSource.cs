using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui
{
	internal record IrDataWrapper (PositionInfo position, object data);

	class IrSource : IReadOnlyList<IrDataWrapper>, INotifyCollectionChanged
	{

		public IrSource(PositionalViewSelector positionalViewSelector)
		{
			PositionalViewSelector = positionalViewSelector;
		}

		readonly PositionalViewSelector PositionalViewSelector;

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		public void Reset()
		{
			CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		public int Count
			=> PositionalViewSelector?.TotalCount ?? 0;

		public bool IsReadOnly
			=> true;

		public IrDataWrapper this[int index]
		{
			get
			{
				var info = PositionalViewSelector?.GetInfo(index);

				if (info == null)
					return default;

				return new(info, PositionalViewSelector?.Adapter?.DataFor(info.Kind, info.SectionIndex, info.ItemIndex));
			}
			set => throw new NotImplementedException();
		}

		public int IndexOf(IrDataWrapper item)
		{
			throw new NotImplementedException();
		}

		public void Insert(int index, IrDataWrapper item)
		{
			throw new NotImplementedException();
		}

		public void RemoveAt(int index)
		{
			throw new NotImplementedException();
		}

		public void Add(IrDataWrapper item)
		{
			throw new NotImplementedException();
		}

		public void Clear()
		{
			throw new NotImplementedException();
		}

		public bool Contains(IrDataWrapper item)
		{
			throw new NotImplementedException();
		}

		public void CopyTo(IrDataWrapper[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public bool Remove(IrDataWrapper item)
		{
			throw new NotImplementedException();
		}

		public IEnumerator<IrDataWrapper> GetEnumerator()
		{
			throw new NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}
	}
}

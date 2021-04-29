using System;
namespace Microsoft.Maui
{
	public interface IItemDelegate<T>
	{
		int GetCount();
		T GetItem(int index);
	}
}

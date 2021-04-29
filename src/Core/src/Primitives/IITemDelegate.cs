using System;
namespace Microsoft.Maui
{
	public interface IITemDelegate<T>
	{
		int GetCount();
		T GetItem(int index);
	}
}

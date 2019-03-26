using System;

namespace Xamarin.Forms.Platform.iOS
{
	sealed class EmptySource : IItemsViewSource
	{
		public int Count => 0;

		public object this[int index] => throw new IndexOutOfRangeException("IItemsViewSource is empty");

		public void Dispose()
		{
		}
	}
}
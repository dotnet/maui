using System;

namespace Xamarin.Forms.Platform.Android
{
	sealed internal class EmptySource : IItemsViewSource
	{
		public int Count => 0;

		public object this[int index] => throw new IndexOutOfRangeException("IItemsViewSource is empty");

		public void Dispose()
		{
			
		}
	}
}
using System.Collections;
using System.Collections.Generic;

namespace Xamarin.Forms.Platform.iOS
{
	internal class ListSource : List<object>, IItemsViewSource
	{
		public ListSource()
		{
		}

		public ListSource(IEnumerable<object> enumerable) : base(enumerable)
		{
			
		}

		public ListSource(IEnumerable enumerable)
		{
			foreach (object item in enumerable)
			{
				Add(item);
			}
		}
	}
}
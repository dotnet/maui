//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	sealed internal class EmptySource : IItemsViewSource
	{
		public int Count => 0;

		public bool HasHeader { get; set; }
		public bool HasFooter { get; set; }

		public void Dispose()
		{

		}

		public bool IsHeader(int index)
		{
			return HasHeader && index == 0;
		}

		public bool IsFooter(int index)
		{
			if (!HasFooter)
			{
				return false;
			}

			if (HasHeader)
			{
				return index == 1;
			}

			return index == 0;
		}

		public int GetPosition(object item)
		{
			return -1;
		}

		public object GetItem(int position)
		{
			throw new IndexOutOfRangeException("IItemsViewSource is empty");
		}
	}
}
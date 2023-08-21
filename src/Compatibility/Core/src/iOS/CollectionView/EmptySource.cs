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
using Foundation;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	internal class EmptySource : ILoopItemsViewSource
	{
		public int GroupCount => 0;

		public int ItemCount => 0;

		public bool Loop { get; set; }

		public int LoopCount => 0;

		public object this[NSIndexPath indexPath] => throw new IndexOutOfRangeException("IItemsViewSource is empty");

		public int ItemCountInGroup(nint group) => 0;

		public object Group(NSIndexPath indexPath)
		{
			throw new IndexOutOfRangeException("IItemsViewSource is empty");
		}

		public NSIndexPath GetIndexForItem(object item)
		{
			throw new IndexOutOfRangeException("IItemsViewSource is empty");
		}

		public void Dispose()
		{
		}
	}
}
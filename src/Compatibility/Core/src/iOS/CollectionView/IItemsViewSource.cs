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

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	public interface IItemsViewSource : IDisposable
	{
		int ItemCount { get; }
		int ItemCountInGroup(nint group);
		int GroupCount { get; }
		object this[Foundation.NSIndexPath indexPath] { get; }
		object Group(Foundation.NSIndexPath indexPath);
		Foundation.NSIndexPath GetIndexForItem(object item);
	}
}
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

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	// Lets observable items sources notify observers about dataset changes
	internal interface ICollectionChangedNotifier
	{
		void NotifyDataSetChanged();
		void NotifyItemChanged(IItemsViewSource source, int startIndex);
		void NotifyItemInserted(IItemsViewSource source, int startIndex);
		void NotifyItemMoved(IItemsViewSource source, int fromPosition, int toPosition);
		void NotifyItemRangeChanged(IItemsViewSource source, int start, int end);
		void NotifyItemRangeInserted(IItemsViewSource source, int startIndex, int count);
		void NotifyItemRangeRemoved(IItemsViewSource source, int startIndex, int count);
		void NotifyItemRemoved(IItemsViewSource source, int startIndex);
	}
}
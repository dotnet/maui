using System;
using System.Collections.Generic;
using System.Text;
using WListView = Microsoft.UI.Xaml.Controls.ListView;
using WCornerRadius = Microsoft.UI.Xaml.CornerRadius;

namespace Microsoft.Maui.Controls.Platform
{
	internal static class ListViewExtensions
	{
		public static void ApplyListViewStyles(this WListView listView)
		{
			// https://github.com/microsoft/microsoft-ui-xaml/blob/9052972906c8a0a1b6cb5d5c61b27d6d27cd7f11/dev/CommonStyles/ListViewItem_themeresources_21h1.xaml#L298
			listView.SetApplicationResource("ListViewItemCornerRadius", new WCornerRadius(0));

			// https://github.com/microsoft/microsoft-ui-xaml/blob/9052972906c8a0a1b6cb5d5c61b27d6d27cd7f11/dev/CommonStyles/ListViewItem_themeresources_21h1.xaml#L314
			listView.SetApplicationResource("ListViewItemSelectionIndicatorVisualEnabled", false);
		}
	}
}

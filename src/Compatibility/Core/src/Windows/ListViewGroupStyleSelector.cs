using System;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	[Obsolete("Use Microsoft.Maui.Controls.Platform.Compatibility.ListViewGroupStyleSelector instead")]
	public partial class ListViewGroupStyleSelector : GroupStyleSelector
	{
		protected override GroupStyle SelectGroupStyleCore(object group, uint level)
		{
			return (GroupStyle)Microsoft.UI.Xaml.Application.Current.Resources["ListViewGroup"];
		}
	}
}
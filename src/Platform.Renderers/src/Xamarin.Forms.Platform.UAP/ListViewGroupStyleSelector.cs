using Microsoft.UI.Xaml.Controls;

namespace Xamarin.Forms.Platform.UWP
{
	public class ListViewGroupStyleSelector : GroupStyleSelector
	{
		protected override GroupStyle SelectGroupStyleCore(object group, uint level)
		{
			return (GroupStyle)Microsoft.UI.Xaml.Application.Current.Resources["ListViewGroup"];
		}
	}
}
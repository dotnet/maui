using Windows.UI.Xaml.Controls;

namespace Xamarin.Forms.Platform.UWP
{
	public class ListViewGroupStyleSelector : GroupStyleSelector
	{
		protected override GroupStyle SelectGroupStyleCore(object group, uint level)
		{
			return (GroupStyle)Windows.UI.Xaml.Application.Current.Resources["ListViewGroup"];
		}
	}
}
using global::Windows.UI.Xaml.Controls;

namespace System.Maui.Platform.UWP
{
	public class ListViewGroupStyleSelector : GroupStyleSelector
	{
		protected override GroupStyle SelectGroupStyleCore(object group, uint level)
		{
			return (GroupStyle)global::Windows.UI.Xaml.Application.Current.Resources["ListViewGroup"];
		}
	}
}
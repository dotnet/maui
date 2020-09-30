using Windows.UI.Xaml.Controls;
using UWPApp = Windows.UI.Xaml.Application;
using UWPDataTemplate = Windows.UI.Xaml.DataTemplate;

namespace Xamarin.Forms.Platform.UWP
{
	internal class GroupHeaderStyleSelector : GroupStyleSelector
	{
		protected override GroupStyle SelectGroupStyleCore(object group, uint level)
		{
			return new GroupStyle
			{
				HeaderTemplate = (UWPDataTemplate)UWPApp.Current.Resources["GroupHeaderTemplate"]
			};
		}
	}
}

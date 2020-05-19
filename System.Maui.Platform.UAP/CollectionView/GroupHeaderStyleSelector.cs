using global::Windows.UI.Xaml.Controls;
using UWPApp = global::Windows.UI.Xaml.Application;
using UWPDataTemplate = global::Windows.UI.Xaml.DataTemplate;

namespace System.Maui.Platform.UWP
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

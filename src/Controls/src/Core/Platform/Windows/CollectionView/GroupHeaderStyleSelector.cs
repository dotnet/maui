#nullable disable
using Microsoft.UI.Xaml.Controls;
using UWPApp = Microsoft.UI.Xaml.Application;
using UWPDataTemplate = Microsoft.UI.Xaml.DataTemplate;

namespace Microsoft.Maui.Controls.Platform
{
	internal partial class GroupHeaderStyleSelector : GroupStyleSelector
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

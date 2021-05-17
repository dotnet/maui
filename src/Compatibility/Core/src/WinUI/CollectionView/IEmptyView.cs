using Microsoft.UI.Xaml;
using WVisibility = Microsoft.UI.Xaml.Visibility;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	internal interface IEmptyView
	{
		WVisibility EmptyViewVisibility { get; set; }
		void SetEmptyView(FrameworkElement emptyView, View formsEmptyView);
	}
}
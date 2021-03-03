using Microsoft.UI.Xaml;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	internal interface IEmptyView
	{
		Visibility EmptyViewVisibility { get; set; }
		void SetEmptyView(FrameworkElement emptyView, View formsEmptyView);
	}
}
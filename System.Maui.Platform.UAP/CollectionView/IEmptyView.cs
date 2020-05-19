using global::Windows.UI.Xaml;

namespace System.Maui.Platform.UWP
{
	internal interface IEmptyView
	{
		Visibility EmptyViewVisibility { get; set; }
		void SetEmptyView(FrameworkElement emptyView, View formsEmptyView);
	}
}
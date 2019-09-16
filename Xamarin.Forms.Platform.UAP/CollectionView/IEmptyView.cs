using Windows.UI.Xaml;

namespace Xamarin.Forms.Platform.UWP
{
	internal interface IEmptyView
	{
		Visibility EmptyViewVisibility { get; set; }
		void SetEmptyView(FrameworkElement emptyView);
	}
}
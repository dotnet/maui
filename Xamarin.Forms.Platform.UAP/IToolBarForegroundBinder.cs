using Windows.UI.Xaml.Controls;

namespace Xamarin.Forms.Platform.UWP
{
	internal interface IToolBarForegroundBinder
	{
		void BindForegroundColor(AppBar appBar);
		void BindForegroundColor(AppBarButton button);
	}
}
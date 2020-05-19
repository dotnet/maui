using global::Windows.UI.Xaml.Controls;

namespace System.Maui.Platform.UWP
{
	internal interface IToolBarForegroundBinder
	{
		void BindForegroundColor(AppBar appBar);
		void BindForegroundColor(AppBarButton button);
	}
}
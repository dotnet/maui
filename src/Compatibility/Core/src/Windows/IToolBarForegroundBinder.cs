using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	internal interface IToolBarForegroundBinder
	{
		void BindForegroundColor(AppBar appBar);
		void BindForegroundColor(AppBarButton button);
	}
}
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Controls.Platform
{
	internal interface IToolBarForegroundBinder
	{
		void BindForegroundColor(AppBar appBar);
		void BindForegroundColor(AppBarButton button);
	}
}
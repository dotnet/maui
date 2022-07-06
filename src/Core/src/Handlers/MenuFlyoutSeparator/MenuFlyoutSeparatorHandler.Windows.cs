using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class MenuFlyoutSeparatorHandler
	{
		protected override MenuFlyoutSeparator CreatePlatformElement()
		{
			return new MenuFlyoutSeparator();
		}
	}
}

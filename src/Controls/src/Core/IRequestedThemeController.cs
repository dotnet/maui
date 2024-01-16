using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Controls
{
	interface IRequestedThemeController
	{
		AppTheme RequestedTheme { get; set; }
	}
}
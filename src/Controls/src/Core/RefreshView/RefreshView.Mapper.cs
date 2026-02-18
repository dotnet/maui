#nullable disable
using System;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	public partial class RefreshView
	{
		static RefreshView()
		{
			// Register dependency: Command depends on CommandParameter for CanExecute evaluation
			// See https://github.com/dotnet/maui/issues/31939
			CommandProperty.DependsOn(CommandParameterProperty);

			// Force VisualElement's static constructor to run first so base-level
			// mapper remappings are applied before these Control-specific ones.
RemappingHelper.EnsureBaseTypeRemapped(typeof(RefreshView), typeof(VisualElement));

			// Adjust the mappings to preserve Controls.RefreshView legacy behaviors
#if WINDOWS
			RefreshViewHandler.Mapper.ReplaceMapping<RefreshView, IRefreshViewHandler>(PlatformConfiguration.WindowsSpecific.RefreshView.RefreshPullDirectionProperty.PropertyName, MapRefreshPullDirection);
#endif
		}
	}
}
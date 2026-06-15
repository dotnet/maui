#nullable disable
using System;
using System.Threading;
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
		}

		static int s_remappedForControls;
		internal override void RemapForControls()
		{
			if (Interlocked.CompareExchange(ref s_remappedForControls, 1, 0) != 0)
				return;

			base.RemapForControls();

			// Adjust the mappings to preserve Controls.RefreshView legacy behaviors
#if WINDOWS
			RefreshViewHandler.Mapper.ReplaceMapping<RefreshView, IRefreshViewHandler>(PlatformConfiguration.WindowsSpecific.RefreshView.RefreshPullDirectionProperty.PropertyName, MapRefreshPullDirection);
#endif
		}
	}
}
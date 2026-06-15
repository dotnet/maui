#nullable disable
using System;
using System.Threading;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	public partial class Window
	{
		static int s_remappedForControls;

		internal new static void RemapForControls()
		{
			if (Interlocked.CompareExchange(ref s_remappedForControls, 1, 0) != 0)
				return;

			Element.RemapForControls();

#if ANDROID
			// This property is also on the Application Mapper since that's where the attached property exists
			WindowHandler.Mapper.ReplaceMapping<IWindow, IWindowHandler>(PlatformConfiguration.AndroidSpecific.Application.WindowSoftInputModeAdjustProperty.PropertyName, MapWindowSoftInputModeAdjust);
#endif

#if WINDOWS
			WindowHandler.Mapper.PrependToMapping<Window, IWindowHandler>(nameof(ITitledElement.Title), MapTitle);
#endif
		}
	}
}

﻿#nullable disable
using System;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	public partial class Window
	{
		internal static new void RemapForControls()
		{
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

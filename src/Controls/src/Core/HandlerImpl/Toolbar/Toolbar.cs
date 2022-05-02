#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{

	public partial class Toolbar
	{
		IMauiContext MauiContext => Handler?.MauiContext ?? throw new InvalidOperationException("MauiContext not set");

		public static IPropertyMapper<Toolbar, ToolbarHandler> ControlsToolbarMapper =
			   new PropertyMapper<Toolbar, ToolbarHandler>(ToolbarHandler.Mapper)
			   {
#if ANDROID || WINDOWS
				   [nameof(IToolbar.IsVisible)] = MapIsVisible,
				   [nameof(IToolbar.BackButtonVisible)] = MapBackButtonVisible,				   
				   [nameof(Toolbar.TitleIcon)] = MapTitleIcon,
				   [nameof(Toolbar.TitleView)] = MapTitleView,
				   [nameof(Toolbar.IconColor)] = MapIconColor,
				   [nameof(Toolbar.ToolbarItems)] = MapToolbarItems,
				   [nameof(Toolbar.BackButtonTitle)] = MapBackButtonTitle,
				   [nameof(Toolbar.BarBackground)] = MapBarBackground,
				   [nameof(Toolbar.BarTextColor)] = MapBarTextColor,
#if WINDOWS
				   [nameof(Toolbar.BackButtonEnabled)] = MapBackButtonEnabled,
				   [PlatformConfiguration.WindowsSpecific.Page.ToolbarPlacementProperty.PropertyName] = MapToolbarPlacement,
				   [PlatformConfiguration.WindowsSpecific.Page.ToolbarDynamicOverflowEnabledProperty.PropertyName] = MapToolbarDynamicOverflowEnabled,
#endif
#endif
			   };

		internal static void RemapForControls()
		{
			ToolbarHandler.Mapper = ControlsToolbarMapper;
		}
	}
}

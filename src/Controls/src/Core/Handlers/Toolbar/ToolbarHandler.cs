#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Handlers
{
#if ANDROID || WINDOWS
	public partial class ToolbarHandler
	{
		public static IPropertyMapper<Toolbar, ToolbarHandler> Mapper =
			   new PropertyMapper<Toolbar, ToolbarHandler>(ElementMapper)
			   {
				   [nameof(Toolbar.IsVisible)] = MapIsVisible,
				   [nameof(Toolbar.BackButtonVisible)] = MapBackButtonVisible,
				   [nameof(Toolbar.TitleIcon)] = MapTitleIcon,
				   [nameof(Toolbar.TitleView)] = MapTitleView,
				   [nameof(Toolbar.IconColor)] = MapIconColor,
				   [nameof(Toolbar.Title)] = MapTitle,
				   [nameof(Toolbar.ToolbarItems)] = MapToolbarItems,
				   [nameof(Toolbar.BackButtonTitle)] = MapBackButtonTitle,
				   [nameof(Toolbar.BarBackgroundColor)] = MapBarBackgroundColor,
				   [nameof(Toolbar.BarBackground)] = MapBarBackground,
				   [nameof(Toolbar.BarTextColor)] = MapBarTextColor,
				   [nameof(Toolbar.IconColor)] = MapIconColor,
#if WINDOWS
				   [PlatformConfiguration.WindowsSpecific.Page.ToolbarPlacementProperty.PropertyName] = MapToolbarPlacement,
				   [PlatformConfiguration.WindowsSpecific.Page.ToolbarDynamicOverflowEnabledProperty.PropertyName] = MapToolbarDynamicOverflowEnabled,
#endif
			   };

		public static CommandMapper<Toolbar, ToolbarHandler> CommandMapper = new()
		{
		};

		public ToolbarHandler() : base(Mapper, CommandMapper)
		{
		}
	}
#endif
}

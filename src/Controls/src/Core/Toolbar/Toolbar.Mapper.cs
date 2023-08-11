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

		[Obsolete("Use ToolbarHandler.Mapper instead.")]
		public static IPropertyMapper<Toolbar, ToolbarHandler> ControlsToolbarMapper =
			new PropertyMapper<Toolbar, ToolbarHandler>(ToolbarHandler.Mapper);

		internal static void RemapForControls()
		{
#if ANDROID || WINDOWS || TIZEN
			ToolbarHandler.Mapper.ReplaceMapping<Toolbar, IToolbarHandler>(nameof(IToolbar.IsVisible), MapIsVisible);
			ToolbarHandler.Mapper.ReplaceMapping<Toolbar, IToolbarHandler>(nameof(IToolbar.BackButtonVisible), MapBackButtonVisible);
			ToolbarHandler.Mapper.ReplaceMapping<Toolbar, IToolbarHandler>(nameof(Toolbar.TitleIcon), MapTitleIcon);
			ToolbarHandler.Mapper.ReplaceMapping<Toolbar, IToolbarHandler>(nameof(Toolbar.TitleView), MapTitleView);
			ToolbarHandler.Mapper.ReplaceMapping<Toolbar, IToolbarHandler>(nameof(Toolbar.IconColor), MapIconColor);
			ToolbarHandler.Mapper.ReplaceMapping<Toolbar, IToolbarHandler>(nameof(Toolbar.ToolbarItems), MapToolbarItems);
			ToolbarHandler.Mapper.ReplaceMapping<Toolbar, IToolbarHandler>(nameof(Toolbar.BackButtonTitle), MapBackButtonTitle);
			ToolbarHandler.Mapper.ReplaceMapping<Toolbar, IToolbarHandler>(nameof(Toolbar.BarBackground), MapBarBackground);
			ToolbarHandler.Mapper.ReplaceMapping<Toolbar, IToolbarHandler>(nameof(Toolbar.BarTextColor), MapBarTextColor);
#endif
#if WINDOWS
			ToolbarHandler.Mapper.ReplaceMapping<Toolbar, IToolbarHandler>(nameof(Toolbar.BackButtonEnabled), MapBackButtonEnabled);
			ToolbarHandler.Mapper.ReplaceMapping<Toolbar, IToolbarHandler>(PlatformConfiguration.WindowsSpecific.Page.ToolbarPlacementProperty.PropertyName, MapToolbarPlacement);
			ToolbarHandler.Mapper.ReplaceMapping<Toolbar, IToolbarHandler>(PlatformConfiguration.WindowsSpecific.Page.ToolbarDynamicOverflowEnabledProperty.PropertyName, MapToolbarDynamicOverflowEnabled);
#endif
		}
	}
}

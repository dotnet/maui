using System;
using System.ComponentModel;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{

	public partial class Toolbar
		: IControlsMapperRemappable
	{
		IMauiContext MauiContext => Handler?.MauiContext ?? throw new InvalidOperationException("MauiContext not set");

		void IControlsMapperRemappable.RemapForControls() => RemapForControls();

		static readonly OneTimeInitializationAction s_remappedForControls = new(RemapForControlsOnce);
		internal virtual void RemapForControls()
		{
			s_remappedForControls.InvokeOnce();
		}

		static void RemapForControlsOnce()
		{
#if ANDROID || WINDOWS || TIZEN
			ToolbarHandler.Mapper.ReplaceMappingForControls<Toolbar, IToolbarHandler>(nameof(IToolbar.IsVisible), MapIsVisible);
			ToolbarHandler.Mapper.ReplaceMappingForControls<Toolbar, IToolbarHandler>(nameof(IToolbar.BackButtonVisible), MapBackButtonVisible);
			ToolbarHandler.Mapper.ReplaceMappingForControls<Toolbar, IToolbarHandler>(nameof(Toolbar.TitleIcon), MapTitleIcon);
			ToolbarHandler.Mapper.ReplaceMappingForControls<Toolbar, IToolbarHandler>(nameof(Toolbar.TitleView), MapTitleView);
			ToolbarHandler.Mapper.ReplaceMappingForControls<Toolbar, IToolbarHandler>(nameof(Toolbar.IconColor), MapIconColor);
			ToolbarHandler.Mapper.ReplaceMappingForControls<Toolbar, IToolbarHandler>(nameof(Toolbar.ToolbarItems), MapToolbarItems);
			ToolbarHandler.Mapper.ReplaceMappingForControls<Toolbar, IToolbarHandler>(nameof(Toolbar.BackButtonTitle), MapBackButtonTitle);
			ToolbarHandler.Mapper.ReplaceMappingForControls<Toolbar, IToolbarHandler>(nameof(Toolbar.BackButtonAccessibilityLabel), MapBackButtonAccessibilityLabel);
			ToolbarHandler.Mapper.ReplaceMappingForControls<Toolbar, IToolbarHandler>(nameof(Toolbar.BarBackground), MapBarBackground);
			ToolbarHandler.Mapper.ReplaceMappingForControls<Toolbar, IToolbarHandler>(nameof(Toolbar.BarTextColor), MapBarTextColor);
#endif
#if WINDOWS
			ToolbarHandler.Mapper.ReplaceMappingForControls<Toolbar, IToolbarHandler>(nameof(Toolbar.BackButtonEnabled), MapBackButtonEnabled);
			ToolbarHandler.Mapper.ReplaceMappingForControls<Toolbar, IToolbarHandler>(PlatformConfiguration.WindowsSpecific.Page.ToolbarPlacementProperty.PropertyName, MapToolbarPlacement);
			ToolbarHandler.Mapper.ReplaceMappingForControls<Toolbar, IToolbarHandler>(PlatformConfiguration.WindowsSpecific.Page.ToolbarDynamicOverflowEnabledProperty.PropertyName, MapToolbarDynamicOverflowEnabled);
#endif
		}
	}
}

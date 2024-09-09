using System;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/FlyoutPage.xml" path="Type[@FullName='Microsoft.Maui.Controls.FlyoutPage']/Docs/*" />
	public partial class FlyoutPage
	{
		[Obsolete("Use FlyoutViewHandler.Mapper instead.")]
		public static IPropertyMapper<IFlyoutView, FlyoutViewHandler> ControlsFlyoutPageMapper = new ControlsMapper<IFlyoutView, FlyoutViewHandler>(FlyoutViewHandler.Mapper);

		internal new static void RemapForControls()
		{
			FlyoutViewHandler.Mapper.ReplaceMapping<IFlyoutView, IFlyoutViewHandler>(nameof(FlyoutLayoutBehavior), MapFlyoutLayoutBehavior);
#if IOS
			FlyoutViewHandler.Mapper.ReplaceMapping<IFlyoutView, IFlyoutViewHandler>(nameof(PlatformConfiguration.iOSSpecific.Page.PrefersHomeIndicatorAutoHiddenProperty), MapPrefersHomeIndicatorAutoHiddenProperty);
			FlyoutViewHandler.Mapper.ReplaceMapping<IFlyoutView, IFlyoutViewHandler>(nameof(PlatformConfiguration.iOSSpecific.Page.PrefersStatusBarHiddenProperty), MapPrefersPrefersStatusBarHiddenProperty);
#endif
		}

		internal static void MapFlyoutLayoutBehavior(IFlyoutViewHandler handler, IFlyoutView view)
		{
			handler.UpdateValue(nameof(IFlyoutView.FlyoutBehavior));
		}

#if IOS
		internal static void MapPrefersHomeIndicatorAutoHiddenProperty(IFlyoutViewHandler handler, IFlyoutView view)
		{
			handler.UpdateValue(nameof(PlatformConfiguration.iOSSpecific.Page.PrefersHomeIndicatorAutoHiddenProperty));
		}

		internal static void MapPrefersPrefersStatusBarHiddenProperty(IFlyoutViewHandler handler, IFlyoutView view)
		{
			handler.UpdateValue(nameof(PlatformConfiguration.iOSSpecific.Page.PrefersStatusBarHiddenProperty));
		}
#endif
	}
}

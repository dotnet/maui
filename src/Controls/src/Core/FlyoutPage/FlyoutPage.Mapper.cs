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
		}

		internal static void MapFlyoutLayoutBehavior(IFlyoutViewHandler handler, IFlyoutView view)
		{
			handler.UpdateValue(nameof(IFlyoutView.FlyoutBehavior));
		}
	}
}

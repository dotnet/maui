#nullable enable

namespace Microsoft.Maui.Controls
{
	public partial class FlyoutPage
	{
		public static IPropertyMapper<IFlyoutView, FlyoutViewHandler> ControlsFlyoutPageMapper = new PropertyMapper<IFlyoutView, FlyoutViewHandler>(FlyoutViewHandler.Mapper)
		{
			[nameof(FlyoutLayoutBehavior)] = (handler, __) => handler.UpdateValue(nameof(IFlyoutView.FlyoutBehavior)),
		};

		internal new static void RemapForControls()
		{
			FlyoutViewHandler.Mapper = ControlsFlyoutPageMapper;
		}
	}
}

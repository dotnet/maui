using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests.Handlers.Layout
{
	public partial class LayoutHandlerTests
	{
		double GetNativeChildCount(LayoutHandler layoutHandler)
		{
			return (layoutHandler.NativeView as LayoutViewGroup).ChildCount;
		}
	}
}
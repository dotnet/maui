using Microsoft.Maui.DeviceTests.Stubs;

namespace Microsoft.Maui.DeviceTests
{
#if WINDOWS
	[Category(TestCategory.Button)]
	public class ButtonFocusTests : FocusHandlerTests<ButtonHandler, ButtonStub, VerticalStackLayoutStub>
	{
		public ButtonFocusTests()
		{
		}
	}
#endif
}

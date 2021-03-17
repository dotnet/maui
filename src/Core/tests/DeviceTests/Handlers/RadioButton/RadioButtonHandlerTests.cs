using Microsoft.Maui.Handlers;
using Microsoft.Maui.DeviceTests.Stubs;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.RadioButton)]
	public partial class RadioButtonHandlerTests : HandlerTestBase<RadioButtonHandler, RadioButtonStub>
	{
		public RadioButtonHandlerTests(HandlerTestFixture fixture) : base(fixture)
		{
		}
	}
}
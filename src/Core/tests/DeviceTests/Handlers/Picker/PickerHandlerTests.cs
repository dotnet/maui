using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests
{
	[Category("PickerHandler")]
	public partial class PickerHandlerTests : HandlerTestBase<PickerHandler>
	{
		public PickerHandlerTests(HandlerTestFixture fixture) : base(fixture)
		{
		}

		[Fact(DisplayName = "[PickerHandler] Title Initializes Correctly")]
		public async Task TitleInitializesCorrectly()
		{
			var picker = new PickerStub
			{
				Title = "Select an Item"
			};

			await ValidatePropertyInitValue(picker, () => picker.Title, GetNativeTitle, picker.Title);
		}
	}
}
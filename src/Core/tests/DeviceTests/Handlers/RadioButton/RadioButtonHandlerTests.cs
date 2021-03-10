using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.RadioButton)]
	public partial class RadioButtonHandlerTests : HandlerTestBase<RadioButtonHandler>
	{
		public RadioButtonHandlerTests(HandlerTestFixture fixture) : base(fixture)
		{
		}

		[Theory(DisplayName = "IsChecked Initializes Correctly")]
		[InlineData(false)]
		[InlineData(true)]
		public async Task ThumbColorInitializesCorrectly(bool isChecked)
		{
			var radioButton = new RadioButtonStub()
			{
				IsChecked = isChecked
			};

			await ValidatePropertyInitValue(radioButton, () => radioButton.IsChecked, GetNativeIsChecked, radioButton.IsChecked);
		}
	}
}
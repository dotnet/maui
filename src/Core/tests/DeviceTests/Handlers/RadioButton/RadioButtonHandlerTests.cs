#if !IOS
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class RadioButtonHandlerTests : HandlerTestBase<RadioButtonHandler, RadioButtonStub>
	{
		[Theory(DisplayName = "IsChecked Initializes Correctly")]
		[InlineData(false)]
		[InlineData(true)]
		public async Task IsCheckedInitializesCorrectly(bool isChecked)
		{
			bool xplatIsChecked = isChecked;

			var radioButton = new RadioButtonStub()
			{
				IsChecked = xplatIsChecked
			};

			bool expectedValue = isChecked;

			var values = await GetValueAsync(radioButton, (handler) =>
			{
				return new
				{
					ViewValue = radioButton.IsChecked,
					PlatformViewValue = GetNativeIsChecked(handler)
				};
			});

			Assert.Equal(xplatIsChecked, values.ViewValue);
			Assert.Equal(expectedValue, values.PlatformViewValue);
		}
	}
}
#endif

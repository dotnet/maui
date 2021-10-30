using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.CheckBox)]
	public partial class CheckBoxHandlerTests : HandlerTestBase<CheckBoxHandler, CheckBoxStub>
	{
		[Theory(DisplayName = "IsChecked Initializes Correctly")]
		[InlineData(true)]
		[InlineData(false)]
		public async Task IsCheckedInitializesCorrectly(bool isChecked)
		{
			var checkBoxStub = new CheckBoxStub()
			{
				IsChecked = isChecked
			};

			await ValidatePropertyInitValue(checkBoxStub, () => checkBoxStub.IsChecked, GetNativeIsChecked, checkBoxStub.IsChecked);
		}

		[Fact(DisplayName = "Foreground Updates Correctly")]
		public async Task ForegroundUpdatesCorrectly()
		{
			var checkBoxStub = new CheckBoxStub()
			{
				Foreground = new SolidPaint(Colors.Red),
				IsChecked = true
			};

			await ValidateColor(checkBoxStub, Colors.Red);
		}
	}
}
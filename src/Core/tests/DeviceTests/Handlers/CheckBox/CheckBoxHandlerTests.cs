using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.CheckBox)]
	public partial class CheckBoxHandlerTests : CoreHandlerTestBase<CheckBoxHandler, CheckBoxStub>
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

		[Fact(DisplayName = "Foreground Initializes Correctly")]
		public async Task ForegroundInitializesCorrectly()
		{
			var checkBoxStub = new CheckBoxStub()
			{
				Foreground = new SolidPaint(Colors.Red),
				IsChecked = true
			};

			await ValidateColor(checkBoxStub, Colors.Red);
		}

		[Theory(DisplayName = "Foreground Updates Correctly")]
		[InlineData(0xFF0000)]
		[InlineData(0x0000FF)]
		public async Task ForegroundUpdatesCorrectly(uint color)
		{
			var checkBoxStub = new CheckBoxStub
			{
				Foreground = new SolidPaint(Colors.Black),
				IsChecked = true
			};

			var expected = Color.FromUint(color);
			checkBoxStub.Foreground = new SolidPaint(expected);

			await ValidateColor(checkBoxStub, expected);
		}
	}
}
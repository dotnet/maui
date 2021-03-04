using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category("EntryHandler")]
	public partial class EntryHandlerTests : HandlerTestBase<EntryHandler>
	{
		public EntryHandlerTests(HandlerTestFixture fixture) : base(fixture)
		{
		}

		[Fact(DisplayName = "Text Initializes Correctly")]
		public async Task TextInitializesCorrectly()
		{
			var entry = new EntryStub()
			{
				Text = "Test"
			};

			await ValidatePropertyInitValue(entry, () => entry.Text, GetNativeText, entry.Text);
		}

		[Fact(DisplayName = "TextColor Initializes Correctly")]
		public async Task TextColorInitializesCorrectly()
		{
			var entry = new EntryStub()
			{
				Text = "Test",
				TextColor = Color.Yellow
			};

			await ValidatePropertyInitValue(entry, () => entry.TextColor, GetNativeTextColor, entry.TextColor);
		}

		[Theory(DisplayName = "IsPassword Initializes Correctly")]
		[InlineData(true)]
		[InlineData(false)]
		public async Task IsPasswordInitializesCorrectly(bool isPassword)
		{
			var entry = new EntryStub()
			{
				IsPassword = isPassword
			};

			await ValidatePropertyInitValue(entry, () => entry.IsPassword, GetNativeIsPassword, isPassword);
		}

		[Theory(DisplayName = "Is Text Prediction Enabled")]
		[InlineData(true)]
		[InlineData(false)]
		public async Task IsTextPredictionEnabledCorrectly(bool isEnabled)
		{
			var entry = new EntryStub()
			{
				IsTextPredictionEnabled = isEnabled
			};

			await ValidatePropertyInitValue(entry, () => entry.IsTextPredictionEnabled, GetNativeIsTextPredictionEnabled , isEnabled);
		}
	}
}

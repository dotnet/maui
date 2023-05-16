using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.TextInput)]
	public abstract partial class TextInputTests<THandler, TView> : ControlsHandlerTestBase
		where THandler : class, IViewHandler, IPlatformViewHandler, new()
		where TView : class, IView, ITextInput, new()
	{
		[Theory(DisplayName = "CursorPosition Initializes Correctly")]
		[InlineData(2)]
		public async Task CursorPositionInitializesCorrectly(int initialPosition)
		{
			var entry = new TView
			{
				Text = "This is TEXT!",
				CursorPosition = initialPosition
			};

			await ValidatePropertyInitValue<int, THandler>(
				entry,
				() => entry.CursorPosition,
				GetPlatformCursorPosition,
				initialPosition);
		}

		[Theory(DisplayName = "SelectionLength Initializes Correctly")]
		[InlineData(2)]
		public async Task SelectionLengthInitializesCorrectly(int initialLength)
		{
			var entry = new TView
			{
				Text = "This is TEXT!",
				SelectionLength = initialLength
			};

			await ValidatePropertyInitValue<int, THandler>(
				entry,
				() => entry.SelectionLength,
				GetPlatformSelectionLength,
				initialLength);
		}

		protected abstract int GetPlatformCursorPosition(THandler handler);

		protected abstract int GetPlatformSelectionLength(THandler handler);
	}
}

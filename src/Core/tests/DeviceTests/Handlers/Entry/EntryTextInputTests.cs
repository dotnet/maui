using Microsoft.Maui.DeviceTests.Stubs;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Entry)]
	public class EntryTextInputTests : TextInputHandlerTests<EntryHandler, EntryStub>
	{
		protected override void SetNativeText(EntryHandler entryHandler, string text)
		{
			EntryHandlerTests.SetNativeText(entryHandler, text);
		}
		protected override int GetCursorStartPosition(EntryHandler entryHandler)
		{
			return EntryHandlerTests.GetCursorStartPosition(entryHandler);
		}

		protected override void UpdateCursorStartPosition(EntryHandler entryHandler, int position)
		{
			EntryHandlerTests.UpdateCursorStartPosition(entryHandler, position);
		}
	}
}
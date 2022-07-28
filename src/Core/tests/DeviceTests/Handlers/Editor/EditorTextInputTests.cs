using Microsoft.Maui.DeviceTests.Stubs;

namespace Microsoft.Maui.DeviceTests
{

	[Category(TestCategory.Editor)]
	public class EditorTextInputTests : TextInputHandlerTests<EditorHandler, EditorStub>
	{
		public EditorTextInputTests()
		{
		}

		protected override void SetNativeText(EditorHandler editorHandler, string text)
		{
			EditorHandlerTests.SetNativeText(editorHandler, text);
		}

		protected override int GetCursorStartPosition(EditorHandler editorHandler)
		{
			return EditorHandlerTests.GetCursorStartPosition(editorHandler);
		}

		protected override void UpdateCursorStartPosition(EditorHandler editorHandler, int position)
		{
			EditorHandlerTests.UpdateCursorStartPosition(editorHandler, position);
		}
	}
}
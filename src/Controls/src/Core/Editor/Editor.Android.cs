#nullable disable
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls
{
	public partial class Editor
	{
		public static void MapText(EditorHandler handler, Editor editor) =>
			MapText((IEditorHandler)handler, editor);

		public static void MapText(IEditorHandler handler, Editor editor)
		{
			if (handler is ViewHandler viewHandler && viewHandler.DataFlowDirection == DataFlowDirection.FromPlatform)
			{
				Platform.EditTextExtensions.UpdateTextFromPlatform(handler.PlatformView, editor);
				return;
			}

			Platform.EditTextExtensions.UpdateText(handler.PlatformView, editor);
		}

		public static void MapText(EditorHandler2 handler, Editor editor)
		{
			if (handler.PlatformView is null)
			{
				return;
			}

			if (handler.DataFlowDirection == DataFlowDirection.FromPlatform)
			{
				Platform.EditTextExtensions.UpdateTextFromPlatform(handler.PlatformView, editor);
				return;
			}

			Platform.EditTextExtensions.UpdateText(handler.PlatformView, editor);
		}

		internal static void MapTextTransform(EditorHandler2 handler, Editor editor)
		{
			if (editor.IsConnectingHandler())
			{
				// If we're connecting the handler, we don't want to map the text multiple times.
				return;
			}

			MapText(handler, editor);
		}
	}
}

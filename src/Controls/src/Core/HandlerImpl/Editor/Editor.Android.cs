#nullable disable
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls
{
	public partial class Editor
	{
		public static void MapText(EditorHandler handler, Editor editor)
		{
			if (handler.DataFlowDirection == DataFlowDirection.FromPlatform)
			{
				Platform.EditTextExtensions.UpdateTextFromPlatform(handler.PlatformView, editor);
				return;
			}

			MapText((IEditorHandler)handler, editor);
		}

		public static void MapText(IEditorHandler handler, Editor editor)
		{
			Platform.EditTextExtensions.UpdateText(handler.PlatformView, editor);
		}

		static void MapFocus(IViewHandler handler, IView view, object args)
		{
			handler.ShowKeyboardIfFocused(view);
		}
	}
}

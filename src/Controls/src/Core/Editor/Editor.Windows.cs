// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
namespace Microsoft.Maui.Controls
{
	public partial class Editor
	{
		public static void MapText(EditorHandler handler, Editor editor) =>
			MapText((IEditorHandler)handler, editor);

		public static void MapDetectReadingOrderFromContent(EditorHandler handler, Editor editor) =>
			MapDetectReadingOrderFromContent((IEditorHandler)handler, editor);

		public static void MapText(IEditorHandler handler, Editor editor)
		{
			Platform.TextBoxExtensions.UpdateText(handler.PlatformView, editor);
		}

		public static void MapDetectReadingOrderFromContent(IEditorHandler handler, Editor editor)
		{
			Platform.InputViewExtensions.UpdateDetectReadingOrderFromContent(handler.PlatformView, editor);
		}
	}
}

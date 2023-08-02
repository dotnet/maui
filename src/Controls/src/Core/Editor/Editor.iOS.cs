#nullable disable
using System;
using UIKit;

namespace Microsoft.Maui.Controls
{
	public partial class Editor
	{
		public static void MapText(EditorHandler handler, Editor editor) =>
			MapText((IEditorHandler)handler, editor);

		public static void MapText(IEditorHandler handler, Editor editor)
		{
			Platform.TextExtensions.UpdateText(handler.PlatformView, editor);
		}

		IDisposable _watchingForTaps;
		private protected override void AddedToPlatformVisualTree()
		{
			base.AddedToPlatformVisualTree();

			_watchingForTaps =
				this
					.FindParentOfType<ContentPage>()?
					.SetupTapIntoNothingness(Handler?.PlatformView as UIView);

		}

		private protected override void RemovedFromPlatformVisualTree()
		{
			base.RemovedFromPlatformVisualTree();

			_watchingForTaps?.Dispose();
			_watchingForTaps = null;
		}
	}
}

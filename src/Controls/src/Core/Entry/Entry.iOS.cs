#nullable disable
using System;
using UIKit;

namespace Microsoft.Maui.Controls
{
	public partial class Entry
	{
		public static void MapCursorColor(IEntryHandler handler, Entry entry)
		{
			Platform.TextExtensions.UpdateCursorColor(handler.PlatformView, entry);
		}

		public static void MapAdjustsFontSizeToFitWidth(IEntryHandler handler, Entry entry)
		{
			Platform.TextExtensions.UpdateAdjustsFontSizeToFitWidth(handler.PlatformView, entry);
		}

		public static void MapText(IEntryHandler handler, Entry entry)
		{
			Platform.TextExtensions.UpdateText(handler.PlatformView, entry);
		}

		public static void MapCursorColor(EntryHandler handler, Entry entry) =>
			MapCursorColor((IEntryHandler)handler, entry);

		public static void MapAdjustsFontSizeToFitWidth(EntryHandler handler, Entry entry) =>
			MapAdjustsFontSizeToFitWidth((IEntryHandler)handler, entry);

		public static void MapText(EntryHandler handler, Entry entry) =>
			MapText((IEntryHandler)handler, entry);

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

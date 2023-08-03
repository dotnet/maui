using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls
{
	public partial class Entry
	{
#if MACCATALYST || IOS || ANDROID

		IDisposable? _watchingForTaps;
		private protected override void AddedToPlatformVisualTree()
		{
			base.AddedToPlatformVisualTree();

			_watchingForTaps =
				this
					.FindParentOfType<ContentPage>()?
					.SetupTapIntoNothingness((Handler as IPlatformViewHandler)?.PlatformView);

		}

		private protected override void RemovedFromPlatformVisualTree(IWindow? oldWindow)
		{
			base.RemovedFromPlatformVisualTree(oldWindow);

			_watchingForTaps?.Dispose();
			_watchingForTaps = null;
		}
#endif

	}
}

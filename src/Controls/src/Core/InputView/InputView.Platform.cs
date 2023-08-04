using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls
{
	public partial class InputView
	{

#if MACCATALYST || IOS || ANDROID
		IDisposable? _watchingForTaps;
		private protected override void AddedToPlatformVisualTree()
		{
			base.AddedToPlatformVisualTree();
			SetupHideSoftInputOnTappedTriggered();
		}

		private protected override void RemovedFromPlatformVisualTree(IWindow? oldWindow)
		{
			base.RemovedFromPlatformVisualTree(oldWindow);

			_watchingForTaps?.Dispose();
			_watchingForTaps = null;
		}

		internal void SetupHideSoftInputOnTappedTriggered()
		{
			_watchingForTaps?.Dispose();
			_watchingForTaps = null;

			var contentPage =
				this.FindParentOfType<ContentPage>();

			if (contentPage is null)
				return;

			if (contentPage.HideSoftInputOnTapped)
			{
				_watchingForTaps =
					this
						.FindParentOfType<ContentPage>()?
						.SetupHideSoftInputOnTapped((Handler as IPlatformViewHandler)?.PlatformView);
			}
			else
			{
				contentPage.PropertyChanged += OnPropertyChanged;
				_watchingForTaps = new ActionDisposable(() =>
				{
					contentPage.PropertyChanged -= OnPropertyChanged;
				});

				void OnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
				{
					if (e.Is(ContentPage.HideSoftInputOnTappedProperty))
					{
						SetupHideSoftInputOnTappedTriggered();
					}
				}
			}

		}
#endif
	}
}

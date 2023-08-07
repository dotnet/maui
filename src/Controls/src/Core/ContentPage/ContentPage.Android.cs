using Android.Views;
using Android.Widget;
using AView = Android.Views.View;
using AViewGroup = Android.Views.ViewGroup;
using System;
using System.Linq;

namespace Microsoft.Maui.Controls
{
	public partial class ContentPage
	{
		private protected override void AddedToPlatformVisualTree()
		{
			base.AddedToPlatformVisualTree();
			_hideSoftInputOnTappedChangedManager.PageAddedToPlatformVisualTree();
		}

		private protected override void RemovedFromPlatformVisualTree(IWindow? oldWindow)
		{
			base.RemovedFromPlatformVisualTree(oldWindow);
			_hideSoftInputOnTappedChangedManager.PageRemovedFromPlatformVisualTree(oldWindow);
		}

		internal IDisposable? SetupHideSoftInputOnTapped(InputView inputView) =>
			_hideSoftInputOnTappedChangedManager.SetCurrentlyFocusedView(inputView);
	}
}

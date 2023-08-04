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
			_hideSoftInputOnTappedChangedManager.AddedToPlatformVisualTree();
		}

		private protected override void RemovedFromPlatformVisualTree(IWindow? oldWindow)
		{
			base.RemovedFromPlatformVisualTree(oldWindow);
			_hideSoftInputOnTappedChangedManager.RemovedFromPlatformVisualTree(oldWindow);
		}

		internal IDisposable? SetupHideSoftInputOnTapped(AView? aView) =>
			_hideSoftInputOnTappedChangedManager.SetupHideSoftInputOnTapped(aView);
	}
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls
{
	public partial class InputView
	{
#if ANDROID || IOS
		IDisposable? _hideSoftInputOnTappedTriggeredToken;
		internal static void MapIsFocused(IViewHandler _, IView view)
		{
			if (view is InputView iv)
			{
				iv.SetupHideSoftInputOnTappedTriggered();
			}
		}

		private protected override void AddedToPlatformVisualTree()
		{
			base.AddedToPlatformVisualTree();
			SetupHideSoftInputOnTappedTriggered();
		}

		private protected override void RemovedFromPlatformVisualTree(IWindow? oldWindow)
		{
			base.RemovedFromPlatformVisualTree(oldWindow);
			SetupHideSoftInputOnTappedTriggered();
		}

		internal void SetupHideSoftInputOnTappedTriggered()
		{
			_hideSoftInputOnTappedTriggeredToken?.Dispose();
			_hideSoftInputOnTappedTriggeredToken =
				this.FindParentOfType<ContentPage>()?
					.SetupHideSoftInputOnTapped(this);
		}
#endif
	}
}

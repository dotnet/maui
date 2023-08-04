using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace Microsoft.Maui.Controls
{
	public partial class ContentPage
	{
		internal IDisposable? SetupHideSoftInputOnTapped(UIView? uIView) =>
			_hideSoftInputOnTappedChangedManager.SetupHideSoftInputOnTapped(uIView);
	}
}

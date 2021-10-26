using System;
using System.Collections.Generic;
using System.Text;
using AView = Android.Views.View;

namespace Microsoft.Maui
{
	class ProcessBackClick : AndroidX.Activity.OnBackPressedCallback, AView.IOnClickListener
	{
		StackNavigationManager _navigationManager;

		public ProcessBackClick(StackNavigationManager navHostPageFragment)
			: base(true)
		{
			_navigationManager = navHostPageFragment;
		}

		public override void HandleOnBackPressed()
		{
			_navigationManager.HardwareBackButtonClicked();
		}

		public void OnClick(AView? v)
		{
			_navigationManager.ToolbarBackButtonClicked();
		}
	}
}

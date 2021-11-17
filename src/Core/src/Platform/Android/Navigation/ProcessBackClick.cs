using System;
using System.Collections.Generic;
using System.Text;
using AView = Android.Views.View;

namespace Microsoft.Maui
{
	class ProcessBackClick : AndroidX.Activity.OnBackPressedCallback, AView.IOnClickListener
	{
		StackNavigationManager _stackNavigationManager;

		public ProcessBackClick(StackNavigationManager navHostPageFragment)
			: base(true)
		{
			_stackNavigationManager = navHostPageFragment;
		}

		public override void HandleOnBackPressed()
		{
			_stackNavigationManager.HardwareBackButtonClicked();
		}

		public void OnClick(AView? v)
		{
			_stackNavigationManager.ToolbarBackButtonClicked();
		}
	}
}

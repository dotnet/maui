using System;
using System.Collections.Generic;
using System.Text;
using AView = Android.Views.View;

namespace Microsoft.Maui
{
	class ProcessBackClick : AndroidX.Activity.OnBackPressedCallback, AView.IOnClickListener
	{
		NavigationManager _navigationManager;

		public ProcessBackClick(NavigationManager navHostPageFragment)
			: base(true)
		{
			_navigationManager = navHostPageFragment;
		}

		public override void HandleOnBackPressed()
		{
			_navigationManager.BackButtonPressed();
		}

		public void OnClick(AView? v)
		{
			HandleOnBackPressed();
		}
	}
}

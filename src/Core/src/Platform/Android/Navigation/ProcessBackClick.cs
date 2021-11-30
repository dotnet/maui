using System;
using System.Collections.Generic;
using System.Text;
using AView = Android.Views.View;

namespace Microsoft.Maui.Platform
{
	class ProcessBackClick : Java.Lang.Object, AView.IOnClickListener
	{
		StackNavigationManager _stackNavigationManager;

		public ProcessBackClick(StackNavigationManager navHostPageFragment)
		{
			_stackNavigationManager = navHostPageFragment;
		}

		public void OnClick(AView? v)
		{
			_stackNavigationManager.ToolbarBackButtonClicked();
		}
	}
}

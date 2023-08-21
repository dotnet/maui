//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using Android.Views;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	public static class ButtonElementManager
	{
		public static bool OnTouch(VisualElement element, IButtonController buttonController, AView v, MotionEvent e)
		{
			switch (e.ActionMasked)
			{
				case MotionEventActions.Down:
					buttonController?.SendPressed();
					break;
				case MotionEventActions.Up:
					buttonController?.SendReleased();
					break;
			}

			return false;
		}

		public static void OnClick(VisualElement element, IButtonController buttonController, AView v)
		{
			buttonController?.SendClicked();
		}
	}
}

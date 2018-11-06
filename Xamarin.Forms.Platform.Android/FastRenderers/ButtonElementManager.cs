
using Android.Views;
using AView = Android.Views.View;
using AMotionEventActions = Android.Views.MotionEventActions;


namespace Xamarin.Forms.Platform.Android.FastRenderers
{
	public static class ButtonElementManager
	{

		public static bool OnTouch(VisualElement element, IButtonController buttonController, AView v, MotionEvent e)
		{
			switch (e.Action)
			{
				case AMotionEventActions.Down:
					buttonController?.SendPressed();
					break;
				case AMotionEventActions.Up:
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
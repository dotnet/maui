using Android.Views;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
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
using System;
using Android.App;
using Android.Views;
using AndroidX.AppCompat.App;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	public partial class Window : IPlatformEventsListener
	{
		internal Activity PlatformActivity =>
			(Handler?.PlatformView as Activity) ?? throw new InvalidOperationException("Window should have an Activity set.");

		[Obsolete]
		public static void MapContent(WindowHandler handler, IWindow view)
		{
		}

		[Obsolete]
		public static void MapContent(IWindowHandler handler, IWindow view)
		{
		}

		internal static void MapWindowSoftInputModeAdjust(IWindowHandler handler, IWindow view)
		{
			if (view.Parent is Application app)
			{
				var setting = PlatformConfiguration.AndroidSpecific.Application.GetWindowSoftInputModeAdjust(app);
				view.UpdateWindowSoftInputModeAdjust(setting.ToPlatform());
			}
		}

		private protected override void OnParentChangedCore()
		{
			base.OnParentChangedCore();
			Handler?.UpdateValue(PlatformConfiguration.AndroidSpecific.Application.WindowSoftInputModeAdjustProperty.PropertyName);
		}


		internal event EventHandler<MotionEvent?>? DispatchTouchEvent;
		bool IPlatformEventsListener.DispatchTouchEvent(MotionEvent? e)
		{
			DispatchTouchEvent?.Invoke(this, e);
			return false;
		}
	}
}
using System;
using Android.App;
using Android.Views;
using AndroidX.AppCompat.App;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	public partial class Window : IPlatformEventsListener, IBackNavigationState
	{
		bool IBackNavigationState.CanConsumeBackNavigation =>
			Navigation.ModalStack.Count > 0 || CanConsumeBackNavigation(Page);

		static bool CanConsumeBackNavigation(Page? page)
		{
			if (page is null)
				return false;

			switch (page)
			{
				case Shell shell:
					if (CanConsumeBackNavigation(shell.CurrentPage))
						return true;

					if (shell.FlyoutIsPresented && shell.GetEffectiveFlyoutBehavior() != FlyoutBehavior.Locked)
						return true;

					return shell.CurrentItem?.CurrentItem?.Stack.Count > 1;

				case NavigationPage navigationPage:
					if (CanConsumeBackNavigation(navigationPage.CurrentPage))
						return true;

					return navigationPage.Navigation.NavigationStack.Count > 1;

				case FlyoutPage flyoutPage:
					if (flyoutPage.IsPresented)
						return true;

					if (flyoutPage.HasBackButtonPressedSubscribers)
						return true;

					return CanConsumeBackNavigation(flyoutPage.Detail);

				case MultiPage<Page> multiPage:
					return CanConsumeBackNavigation(multiPage.CurrentPage);

				default:
					return CanPageDefaultConsumeBackNavigation(page);
			}
		}

		static bool CanPageDefaultConsumeBackNavigation(Page page) =>
			page.RealParent is not null &&
			page.RealParent is not (BaseShellItem or Shell or Window or NavigationPage or FlyoutPage or MultiPage<Page>);

		void RefreshPredictiveBackRegistration() =>
			(Handler?.PlatformView as MauiAppCompatActivity)
				?.UpdatePredictiveBackRegistration();
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
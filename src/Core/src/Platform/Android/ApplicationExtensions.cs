using System;
using Android.App;
using Android.Content;
using Android.OS;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.LifecycleEvents;

namespace Microsoft.Maui.Platform
{
	public static class ApplicationExtensions
	{
		public static void CreateNativeWindow(this Activity activity, IApplication application, Bundle? savedInstanceState = null)
		{
			if (application.Handler?.MauiContext is not IMauiContext applicationContext)
				return;

			savedInstanceState ??= activity.Intent?.Extras;

			var mauiContext = applicationContext.MakeScoped(activity);

			applicationContext.Services.InvokeLifecycleEvents<AndroidLifecycle.OnMauiContextCreated>(del => del(mauiContext));

			var activationState = new ActivationState(mauiContext, savedInstanceState);

			var window = application.CreateWindow(activationState);

			activity.SetWindowHandler(window, mauiContext);
		}
	}
}
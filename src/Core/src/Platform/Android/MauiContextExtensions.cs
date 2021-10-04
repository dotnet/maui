using System;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.Fragment.App;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui
{
	internal static partial class MauiContextExtensions
	{
		public static FlowDirection GetFlowDirection(this IMauiContext mauiContext)
		{
			var config = mauiContext?.Context?.Resources?.Configuration;
			if (config == null)
				return FlowDirection.LeftToRight;

			return config.LayoutDirection.ToFlowDirection();
		}

		public static NavigationManager GetNavigationManager(this IMauiContext mauiContext) =>
			mauiContext.Services.GetRequiredService<NavigationManager>();

		public static LayoutInflater GetLayoutInflater(this IMauiContext mauiContext)
		{
			var layoutInflater = mauiContext.Services.GetService<LayoutInflater>();

			if (layoutInflater == null && mauiContext.Context != null)
			{
				var activity = mauiContext.Context.GetActivity();

				if (activity != null)
					layoutInflater = LayoutInflater.From(activity);
			}

			return layoutInflater ?? throw new InvalidOperationException("LayoutInflater Not Found");
		}

		public static FragmentManager GetFragmentManager(this IMauiContext mauiContext)
		{
			var fragmentManager = mauiContext.Services.GetService<FragmentManager>();

			return fragmentManager
				?? mauiContext.Context?.GetFragmentManager()
				?? throw new InvalidOperationException("FragmentManager Not Found");
		}

		public static AppCompatActivity GetActivity(this IMauiContext mauiContext) =>
			(mauiContext.Context?.GetActivity() as AppCompatActivity)
			?? throw new InvalidOperationException("AppCompatActivity Not Found");

		public static IMauiContext MakeScoped(this IMauiContext mauiContext, LayoutInflater? layoutInflater = null, FragmentManager? fragmentManager = null)
		{
			var scopedContext = new MauiContext(mauiContext);

			if (layoutInflater != null)
				scopedContext.AddWeakSpecific(layoutInflater);

			if (fragmentManager != null)
				scopedContext.AddWeakSpecific(fragmentManager);

			return scopedContext;
		}

		public static IMauiContext MakeScoped(this IMauiContext mauiContext, NavigationManager navigationManager)
		{
			var scopedContext = new MauiContext(mauiContext);

			scopedContext.AddSpecific(navigationManager);

			return scopedContext;
		}

		public static IMauiContext MakeScoped(this IMauiContext mauiContext, Android.App.Activity nativeWindow)
		{
			var scopedContext = new MauiContext(mauiContext.Services, nativeWindow, mauiContext);

			scopedContext.AddSpecific(nativeWindow);

			return scopedContext;
		}
	}
}

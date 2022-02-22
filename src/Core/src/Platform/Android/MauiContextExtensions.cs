using System;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.Fragment.App;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Platform
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

		public static NavigationRootManager GetNavigationRootManager(this IMauiContext mauiContext) =>
			mauiContext.Services.GetRequiredService<NavigationRootManager>();

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

		public static IMauiContext MakeScoped(this IMauiContext mauiContext,
			LayoutInflater? layoutInflater = null,
			FragmentManager? fragmentManager = null,
			Android.Content.Context? context = null,
			bool registerNewNavigationRoot = false)
		{
			var scopedContext = new MauiContext(mauiContext.Services);

			if (layoutInflater != null)
				scopedContext.AddWeakSpecific(layoutInflater);

			if (fragmentManager != null)
				scopedContext.AddWeakSpecific(fragmentManager);

			if (context != null)
				scopedContext.AddWeakSpecific(context);

			if (registerNewNavigationRoot)
			{
				if (fragmentManager == null)
					throw new InvalidOperationException("If you're creating a new Navigation Root you need to use a new Fragment Manager");

				scopedContext.AddWeakSpecific(new NavigationRootManager(scopedContext));
			}

			return scopedContext;
		}

		public static IMauiContext MakeScopededArgs<TArgs>(this IMauiContext mauiContext, TArgs args)
			where TArgs : class
		{
			var scopedContext = new MauiContext(mauiContext.Services);
			scopedContext.AddWeakSpecific(args);
			return scopedContext;
		}

		internal static IServiceProvider GetApplicationServices(this IMauiContext mauiContext)
		{
			if (mauiContext.Context?.ApplicationContext is MauiApplication ma)
				return ma.Services;

			throw new InvalidOperationException("Unable to find Application Services");
		}

		public static Android.App.Activity GetPlatformWindow(this IMauiContext mauiContext) =>
			mauiContext.Services.GetRequiredService<Android.App.Activity>();
	}
}
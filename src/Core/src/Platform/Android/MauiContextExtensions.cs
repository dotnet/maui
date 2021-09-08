using System;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.Fragment.App;

namespace Microsoft.Maui
{
	internal static class MauiContextExtensions
	{
		public static FlowDirection GetFlowDirection(this IMauiContext mauiContext)
		{
			var config = mauiContext?.Context?.Resources?.Configuration;
			if (config == null)
				return FlowDirection.LeftToRight;

			return config.LayoutDirection.ToFlowDirection();
		}

		public static LayoutInflater GetLayoutInflater(this IMauiContext mauiContext)
		{
			LayoutInflater? layoutInflater = null;
			if (mauiContext is IScopedMauiContext smc)
				layoutInflater = smc.LayoutInflater;

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
			FragmentManager? fragmentManager = null;
			if (mauiContext is IScopedMauiContext smc)
				fragmentManager = smc.FragmentManager;

			return fragmentManager 
				?? mauiContext.Context?.GetFragmentManager()
				?? throw new InvalidOperationException("LayoutInflater Not Found");
		}

		public static AppCompatActivity GetActivity(this IMauiContext mauiContext) =>
			(mauiContext.Context?.GetActivity() as AppCompatActivity)
			?? throw new InvalidOperationException("AppCompatActivity Not Found");
	}
}

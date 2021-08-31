using System;
using System.Collections.Generic;
using System.Text;
using AndroidX.Navigation;
using AndroidX.Navigation.Fragment;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui
{
	class MauiFragmentNavDestination : FragmentNavigator.Destination
	{
		public IView Page { get; }
		public IMauiContext MauiContext => NavigationPageHandler.MauiContext ?? throw new InvalidOperationException($"MauiContext cannot be null here");
		public NavigationPageHandler NavigationPageHandler { get; }

		// Todo we want to generate the same ids for each page so if the app is recreated
		// we want these to match up
		static Dictionary<IView, int> Pages = new Dictionary<IView, int>();

		public MauiFragmentNavDestination(Navigator fragmentNavigator, IView page, NavigationPageHandler navigationPageHandler) : base(fragmentNavigator)
		{
			_ = page ?? throw new ArgumentNullException(nameof(page));
			_ = navigationPageHandler ?? throw new ArgumentNullException(nameof(navigationPageHandler));
			SetClassName(Java.Lang.Class.FromType(typeof(NavHostPageFragment)).CanonicalName);

			if (!Pages.ContainsKey(page))
			{
				Id = global::Android.Views.View.GenerateViewId();
				Pages.Add(page, Id);
			}

			Id = Pages[page];
			this.Page = page;
			this.NavigationPageHandler = navigationPageHandler;
		}

		public static MauiFragmentNavDestination AddDestination(
			IView page,
			NavigationPageHandler navigationPageHandler,
			NavGraph navGraph,
			FragmentNavigator navigator)
		{
			var destination = new MauiFragmentNavDestination(navigator, page, navigationPageHandler);

			navGraph.AddDestination(destination);
			return destination;
		}
	}
}

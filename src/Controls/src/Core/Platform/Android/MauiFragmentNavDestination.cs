using System;
using System.Collections.Generic;
using System.Text;
using AndroidX.Navigation;
using AndroidX.Navigation.Fragment;
using Microsoft.Maui.Controls.Handlers;

namespace Microsoft.Maui.Controls.Platform
{
	class MauiFragmentNavDestination : FragmentNavigator.Destination
	{
		public IPage Page { get; }
		public IMauiContext MauiContext => NavigationPageHandler.MauiContext;
		public NavigationPageHandler NavigationPageHandler { get; }

		// Todo we want to generate the same ids for each page so if the app is recreated
		// we want these to match up
		static Dictionary<IPage, int> Pages = new Dictionary<IPage, int>();

		public MauiFragmentNavDestination(Navigator fragmentNavigator, IPage page, NavigationPageHandler navigationPageHandler) : base(fragmentNavigator)
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
			IPage page,
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

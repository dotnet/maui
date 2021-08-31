using System;
using System.Collections.Generic;
using AndroidX.Navigation.Fragment;

namespace Microsoft.Maui
{
	public class FragmentDestination : FragmentNavigator.Destination
	{
		public IView Page { get; internal set; }
		internal IMauiContext MauiContext => NavigationLayout.MauiContext ?? throw new InvalidOperationException($"MauiContext cannot be null here");
		internal NavigationLayout NavigationLayout { get; }

		MauiNavGraph _navGraph { get; }
		Dictionary<IView, int> Pages => _navGraph.Pages;

		public FragmentDestination(IView page, NavigationLayout navigationLayout, MauiNavGraph navGraphDestination) : base(navigationLayout.FragmentNavigator)
		{
			_ = page ?? throw new ArgumentNullException(nameof(page));
			_ = navigationLayout ?? throw new ArgumentNullException(nameof(navigationLayout));
			SetClassName(Java.Lang.Class.FromType(typeof(NavHostPageFragment)).CanonicalName);
			_navGraph = navGraphDestination;

			if (!Pages.ContainsKey(page))
			{
				Id = global::Android.Views.View.GenerateViewId();
				Pages.Add(page, Id);
			}

			Id = Pages[page];
			Page = page;
			NavigationLayout = navigationLayout;
		}
	}
}

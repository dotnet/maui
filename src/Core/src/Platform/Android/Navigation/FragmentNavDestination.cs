using System;
using System.Collections.Generic;
using System.Text;
using AndroidX.Navigation;
using AndroidX.Navigation.Fragment;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui
{
	internal class FragmentNavDestination : FragmentNavigator.Destination
	{
		public IView Page { get; internal set; }
		public IMauiContext MauiContext => NavigationLayout.MauiContext ?? throw new InvalidOperationException($"MauiContext cannot be null here");
		public NavigationLayout NavigationLayout { get; }

		NavGraphDestination _navGraph { get; }
		Dictionary<IView, int> Pages => _navGraph.Pages;

		public FragmentNavDestination(IView page, NavigationLayout navigationLayout, NavGraphDestination navGraphDestination) : base(navigationLayout.FragmentNavigator)
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

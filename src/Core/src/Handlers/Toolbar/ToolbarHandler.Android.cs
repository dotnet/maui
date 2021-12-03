#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Google.Android.Material.AppBar;
using Microsoft.Maui.Handlers;
using LP = Android.Views.ViewGroup.LayoutParams;

namespace Microsoft.Maui.Handlers
{
	public partial class ToolbarHandler : ElementHandler<IToolbar, MaterialToolbar>
	{
		NavigationRootManager? NavigationRootManager =>
			MauiContext?.GetNavigationRootManager();

		protected override MaterialToolbar CreateNativeElement()
		{
			LayoutInflater? li = MauiContext?.GetLayoutInflater();
			_ = li ?? throw new InvalidOperationException($"LayoutInflater cannot be null");

			var view = li.Inflate(Microsoft.Maui.Resource.Layout.maui_toolbar, null)?.JavaCast<MaterialToolbar>();
			_ = view ?? throw new InvalidOperationException($"Resource.Layout.maui_toolbar view not found");

			view.LayoutParameters = new AppBarLayout.LayoutParams(LP.MatchParent, MauiContext?.Context?.GetActionBarHeight() ?? LP.WrapContent)
			{
				ScrollFlags = 0
			};

			return view;
		}
	}
}

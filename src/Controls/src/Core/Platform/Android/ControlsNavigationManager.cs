using System;
using System.Collections.Generic;
using System.ComponentModel;
using Android.Animation;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.Graphics.Drawable;
using AndroidX.DrawerLayout.Widget;
using AndroidX.Navigation;
using static Android.Views.View;
using static Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.AppCompat.NavigationPage;
using ActionBarDrawerToggle = AndroidX.AppCompat.App.ActionBarDrawerToggle;
using AToolbar = AndroidX.AppCompat.Widget.Toolbar;
using AView = Android.Views.View;
using Color = Microsoft.Maui.Graphics.Color;
using FragmentManager = AndroidX.Fragment.App.FragmentManager;
using Object = Java.Lang.Object;

namespace Microsoft.Maui.Controls.Platform
{
	// This class is still a bit too over complicated
	// As we move things into Core we'll better split it apart
	public class ControlsNavigationManager : NavigationManager, IManageFragments
	{
		FragmentManager _fragmentManager;

		new NavigationPage NavigationView => (NavigationPage)base.VirtualView;
		new Page CurrentPage => (Page)base.CurrentPage;

		public ControlsNavigationManager(IMauiContext mauiContext) : base(mauiContext)
		{
		}

		INavigationPageController NavigationPageController => NavigationView;

		IPageController PageController => NavigationView;


		void IManageFragments.SetFragmentManager(FragmentManager childFragmentManager)
		{
			if (_fragmentManager == null)
				_fragmentManager = childFragmentManager;
		}

		// These are only relevant when nested inside a drawer layout
		void AnimateArrowIn()
		{
			var icon = Toolbar.NavigationIcon as DrawerArrowDrawable;
			if (icon == null)
				return;

			ValueAnimator valueAnim = ValueAnimator.OfFloat(0, 1);
			valueAnim.SetDuration(200);
			valueAnim.Update += (s, a) => icon.Progress = (float)a.Animation.AnimatedValue;
			valueAnim.Start();
		}

		void AnimateArrowOut()
		{
			var icon = Toolbar.NavigationIcon as DrawerArrowDrawable;
			if (icon == null)
				return;

			ValueAnimator valueAnim = ValueAnimator.OfFloat(1, 0);
			valueAnim.SetDuration(200);
			valueAnim.Update += (s, a) => icon.Progress = (float)a.Animation.AnimatedValue;
			valueAnim.Start();
		}

		public void OnClick(AView v)
		{
			NavigationView?.PopAsync();
		}

		public override void RequestNavigation(NavigationRequest e)
		{
			NavAnimationInProgress = true;
			base.RequestNavigation(e);
			NavAnimationInProgress = false;
		}


		// AFAICT this is specific to ListView and Context Items
		bool _navAnimationInProgress;
		internal const string CloseContextActionsSignalName = "Xamarin.CloseContextActions";
		internal bool NavAnimationInProgress
		{
			get { return _navAnimationInProgress; }
			set
			{
				if (_navAnimationInProgress == value)
					return;
				_navAnimationInProgress = value;
				if (value)
					MessagingCenter.Send(this, CloseContextActionsSignalName);
			}
		}


		class ClickListener : Object, IOnClickListener
		{
			readonly NavigationPage _element;

			public ClickListener(NavigationPage element)
			{
				_element = element;
			}

			public void OnClick(AView v)
			{
				_element?.PopAsync();
			}
		}

		class DrawerMultiplexedListener : Object, DrawerLayout.IDrawerListener
		{
			public List<DrawerLayout.IDrawerListener> Listeners { get; } = new List<DrawerLayout.IDrawerListener>(2);

			public void OnDrawerClosed(AView drawerView)
			{
				foreach (DrawerLayout.IDrawerListener listener in Listeners)
					listener.OnDrawerClosed(drawerView);
			}

			public void OnDrawerOpened(AView drawerView)
			{
				foreach (DrawerLayout.IDrawerListener listener in Listeners)
					listener.OnDrawerOpened(drawerView);
			}

			public void OnDrawerSlide(AView drawerView, float slideOffset)
			{
				foreach (DrawerLayout.IDrawerListener listener in Listeners)
					listener.OnDrawerSlide(drawerView, slideOffset);
			}

			public void OnDrawerStateChanged(int newState)
			{
				foreach (DrawerLayout.IDrawerListener listener in Listeners)
					listener.OnDrawerStateChanged(newState);
			}
		}
	}
}

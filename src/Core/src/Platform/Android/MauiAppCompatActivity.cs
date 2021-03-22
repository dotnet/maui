using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.CoordinatorLayout.Widget;
using Google.Android.Material.AppBar;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui
{
	public class MauiAppCompatActivity : AppCompatActivity
	{
		protected override void OnCreate(Bundle? savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			if (MauiApp.Current == null)
				throw new InvalidOperationException($"App is not {nameof(MauiApp)}");

			var mauiApp = MauiApp.Current;

			if (mauiApp.Services == null)
				throw new InvalidOperationException("App was not initialized");

			var mauiContext = new MauiContext(mauiApp.Services, this);
			var state = new ActivationState(mauiContext, savedInstanceState);
			var window = mauiApp.CreateWindow(state);

			window.MauiContext = mauiContext;

			var content = (window.Page as IView) ??
				window.Page.View;

			CoordinatorLayout parent = new CoordinatorLayout(this);

			SetContentView(parent, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));

			//AddToolbar(parent);

			parent.AddView(content.ToNative(window.MauiContext), new CoordinatorLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));

			foreach (var androidLifecycleHandler in GetAndroidLifecycleHandler())
				androidLifecycleHandler.OnCreate(this, savedInstanceState);
		}

		protected override void OnStart()
		{
			base.OnStart();

			foreach (var androidLifecycleHandler in GetAndroidLifecycleHandler())
				androidLifecycleHandler.OnStart(this);
		}

		protected override void OnPause()
		{
			base.OnPause();

			foreach (var androidLifecycleHandler in GetAndroidLifecycleHandler())
				androidLifecycleHandler.OnPause(this);
		}

		protected override void OnResume()
		{
			base.OnResume();

			foreach (var androidLifecycleHandler in GetAndroidLifecycleHandler())
				androidLifecycleHandler.OnResume(this);
		}

		protected override void OnRestart()
		{
			base.OnRestart();

			foreach (var androidLifecycleHandler in GetAndroidLifecycleHandler())
				androidLifecycleHandler.OnRestart(this);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			foreach (var androidLifecycleHandler in GetAndroidLifecycleHandler())
				androidLifecycleHandler.OnDestroy(this);
		}

		protected override void OnSaveInstanceState(Bundle outState)
		{
			base.OnSaveInstanceState(outState);

			foreach (var androidLifecycleHandler in GetAndroidLifecycleHandler())
				androidLifecycleHandler.OnSaveInstanceState(this, outState);
		}

		protected override void OnRestoreInstanceState(Bundle savedInstanceState)
		{
			base.OnRestoreInstanceState(savedInstanceState);

			foreach (var androidLifecycleHandler in GetAndroidLifecycleHandler())
				androidLifecycleHandler.OnRestoreInstanceState(this, savedInstanceState);
		}

		public override void OnConfigurationChanged(Configuration newConfig)
		{
			base.OnConfigurationChanged(newConfig);

			foreach (var androidLifecycleHandler in GetAndroidLifecycleHandler())
				androidLifecycleHandler.OnConfigurationChanged(this, newConfig);
		}

		protected override void OnActivityResult(int requestCode, [GeneratedEnum] Android.App.Result resultCode, Intent? data)
		{
			base.OnActivityResult(requestCode, resultCode, data);

			foreach (var androidLifecycleHandler in GetAndroidLifecycleHandler())
				androidLifecycleHandler.OnActivityResult(this, requestCode, resultCode, data);
		}

		void AddToolbar(ViewGroup parent)
		{
			Toolbar toolbar = new Toolbar(this);
			var appbarLayout = new AppBarLayout(this);

			appbarLayout.AddView(toolbar, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, Android.Resource.Attribute.ActionBarSize));
			SetSupportActionBar(toolbar);
			parent.AddView(appbarLayout, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
		}

		IEnumerable<IAndroidLifecycleHandler> GetAndroidLifecycleHandler() =>
			App.Current?.Services?.GetServices<IAndroidLifecycleHandler>() ?? Enumerable.Empty<IAndroidLifecycleHandler>();
	}
}

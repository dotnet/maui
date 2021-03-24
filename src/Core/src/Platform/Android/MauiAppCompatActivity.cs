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

			var mauiApp = MauiApplication.Current.Application;
			if (mauiApp == null)
				throw new InvalidOperationException($"The {nameof(IApplication)} instance was not found.");

			var services = MauiApplication.Current.Services;
			if (mauiApp == null)
				throw new InvalidOperationException($"The {nameof(IServiceProvider)} instance was not found.");

			var mauiContext = new MauiContext(services, this);

			var state = new ActivationState(mauiContext, savedInstanceState);
			var window = mauiApp.CreateWindow(state);
			window.MauiContext = mauiContext;

			var content = (window.Page as IView) ?? window.Page.View;

			CoordinatorLayout parent = new CoordinatorLayout(this);

			SetContentView(parent, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));

			//AddToolbar(parent);

			parent.AddView(content.ToNative(window.MauiContext), new CoordinatorLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));

			foreach (var androidApplicationLifetime in GetAndroidApplicationLifetime())
				androidApplicationLifetime.OnCreate(this, savedInstanceState);
		}

		protected override void OnPostCreate(Bundle? savedInstanceState)
		{
			base.OnPostCreate(savedInstanceState);

			foreach (var androidApplicationLifetime in GetAndroidApplicationLifetime())
				androidApplicationLifetime.OnPostCreate(this, savedInstanceState);
		}

		protected override void OnStart()
		{
			base.OnStart();

			foreach (var androidApplicationLifetime in GetAndroidApplicationLifetime())
				androidApplicationLifetime.OnStart(this);
		}

		protected override void OnPause()
		{
			base.OnPause();

			foreach (var androidApplicationLifetime in GetAndroidApplicationLifetime())
				androidApplicationLifetime.OnPause(this);
		}

		protected override void OnResume()
		{
			base.OnResume();

			foreach (var androidApplicationLifetime in GetAndroidApplicationLifetime())
				androidApplicationLifetime.OnResume(this);
		}

		protected override void OnPostResume()
		{
			base.OnPostResume();

			foreach (var androidApplicationLifetime in GetAndroidApplicationLifetime())
				androidApplicationLifetime.OnPostResume(this);
		}

		protected override void OnRestart()
		{
			base.OnRestart();

			foreach (var androidApplicationLifetime in GetAndroidApplicationLifetime())
				androidApplicationLifetime.OnRestart(this);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			foreach (var androidApplicationLifetime in GetAndroidApplicationLifetime())
				androidApplicationLifetime.OnDestroy(this);
		}

		protected override void OnSaveInstanceState(Bundle outState)
		{
			base.OnSaveInstanceState(outState);

			foreach (var androidApplicationLifetime in GetAndroidApplicationLifetime())
				androidApplicationLifetime.OnSaveInstanceState(this, outState);
		}

		protected override void OnRestoreInstanceState(Bundle savedInstanceState)
		{
			base.OnRestoreInstanceState(savedInstanceState);

			foreach (var androidApplicationLifetime in GetAndroidApplicationLifetime())
				androidApplicationLifetime.OnRestoreInstanceState(this, savedInstanceState);
		}

		public override void OnConfigurationChanged(Configuration newConfig)
		{
			base.OnConfigurationChanged(newConfig);

			foreach (var androidApplicationLifetime in GetAndroidApplicationLifetime())
				androidApplicationLifetime.OnConfigurationChanged(this, newConfig);
		}

		protected override void OnActivityResult(int requestCode, [GeneratedEnum] Android.App.Result resultCode, Intent? data)
		{
			base.OnActivityResult(requestCode, resultCode, data);

			foreach (var androidApplicationLifetime in GetAndroidApplicationLifetime())
				androidApplicationLifetime.OnActivityResult(this, requestCode, resultCode, data);
		}

		void AddToolbar(ViewGroup parent)
		{
			Toolbar toolbar = new Toolbar(this);
			var appbarLayout = new AppBarLayout(this);

			appbarLayout.AddView(toolbar, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, Android.Resource.Attribute.ActionBarSize));
			SetSupportActionBar(toolbar);
			parent.AddView(appbarLayout, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
		}

		IEnumerable<IAndroidApplicationLifetime> GetAndroidApplicationLifetime() =>
			MauiApplication.Current?.Services?.GetServices<IAndroidApplicationLifetime>() ?? Enumerable.Empty<IAndroidApplicationLifetime>();
	}
}

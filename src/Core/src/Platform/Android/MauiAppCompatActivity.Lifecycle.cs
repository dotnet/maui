using System;
using System.Collections.Generic;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.LifecycleEvents;

namespace Microsoft.Maui
{
	public partial class MauiAppCompatActivity
	{
		protected override void OnActivityResult(int requestCode, Android.App.Result resultCode, Intent? data)
		{
			base.OnActivityResult(requestCode, resultCode, data);

			InvokeLifecycleEvents<AndroidLifecycle.OnActivityResult>(del => del(this, requestCode, resultCode, data));
		}

		public override void OnBackPressed()
		{
			base.OnBackPressed();

			InvokeLifecycleEvents<AndroidLifecycle.OnBackPressed>(del => del(this));
		}

		public override void OnConfigurationChanged(Configuration newConfig)
		{
			base.OnConfigurationChanged(newConfig);

			InvokeLifecycleEvents<AndroidLifecycle.OnConfigurationChanged>(del => del(this, newConfig));
		}

		protected override void OnCreate(Bundle? savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			MauiOnCreate(savedInstanceState);

			InvokeLifecycleEvents<AndroidLifecycle.OnCreate>(del => del(this, savedInstanceState));
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			InvokeLifecycleEvents<AndroidLifecycle.OnDestroy>(del => del(this));
		}

		protected override void OnPause()
		{
			base.OnPause();

			InvokeLifecycleEvents<AndroidLifecycle.OnPause>(del => del(this));
		}

		protected override void OnPostCreate(Bundle? savedInstanceState)
		{
			base.OnPostCreate(savedInstanceState);

			InvokeLifecycleEvents<AndroidLifecycle.OnPostCreate>(del => del(this, savedInstanceState));
		}

		protected override void OnPostResume()
		{
			base.OnPostResume();

			InvokeLifecycleEvents<AndroidLifecycle.OnPostResume>(del => del(this));
		}

		protected override void OnRestart()
		{
			base.OnRestart();

			InvokeLifecycleEvents<AndroidLifecycle.OnRestart>(del => del(this));
		}

		protected override void OnRestoreInstanceState(Bundle savedInstanceState)
		{
			base.OnRestoreInstanceState(savedInstanceState);

			InvokeLifecycleEvents<AndroidLifecycle.OnRestoreInstanceState>(del => del(this, savedInstanceState));
		}

		protected override void OnResume()
		{
			base.OnResume();

			InvokeLifecycleEvents<AndroidLifecycle.OnResume>(del => del(this));
		}

		protected override void OnSaveInstanceState(Bundle outState)
		{
			InvokeLifecycleEvents<AndroidLifecycle.OnSaveInstanceState>(del => del(this, outState));

			base.OnSaveInstanceState(outState);
		}

		protected override void OnStart()
		{
			base.OnStart();

			InvokeLifecycleEvents<AndroidLifecycle.OnStart>(del => del(this));
		}

		protected override void OnStop()
		{
			base.OnStop();

			InvokeLifecycleEvents<AndroidLifecycle.OnStop>(del => del(this));
		}

		void InvokeLifecycleEvents<TDelegate>(Action<TDelegate> action)
			where TDelegate : Delegate
		{
			var delegates = GetLifecycleEventDelegates<TDelegate>();

			foreach (var del in delegates)
				action(del);
		}

		IEnumerable<TDelegate> GetLifecycleEventDelegates<TDelegate>(string? eventName = null)
			where TDelegate : Delegate
		{
			var service = MauiApplication.Current?.Services?.GetServices<ILifecycleEventService>();
			if (service == null)
				yield break;

			if (eventName == null)
				eventName = typeof(TDelegate).Name;

			foreach (var events in service)
				foreach (var del in events.GetDelegates<TDelegate>(eventName))
					yield return del;
		}

		bool HasLifecycleEventDelegates<TDelegate>(string? eventName = null)
			where TDelegate : Delegate
		{
			var services = MauiApplication.Current?.Services?.GetServices<ILifecycleEventService>();
			if (services == null)
				return false;

			if (eventName == null)
				eventName = typeof(TDelegate).Name;

			foreach (var events in services)
				if (events.HasDelegates(eventName))
					return true;

			return false;
		}
	}
}

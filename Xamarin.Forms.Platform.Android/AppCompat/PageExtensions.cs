using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;

namespace Xamarin.Forms.Platform.Android
{
	public static class PageExtensions
	{
		public static Fragment CreateFragment(this ContentPage view, Context context)
		{
			if (!Forms.IsInitialized)
				throw new InvalidOperationException("call Forms.Init() before this");

			if (!(view.RealParent is Application))
			{
				Application app = new DefaultApplication();
				app.MainPage = view;
			}

			var platform = new Platform(context, true);
			platform.SetPage(view);

			var vg = platform.GetViewGroup();

			return new EmbeddedFragment(vg, platform);
		}

		public static global::Android.Support.V4.App.Fragment CreateSupportFragment(this ContentPage view, Context context)
		{
			if (!Forms.IsInitialized)
				throw new InvalidOperationException("call Forms.Init() before this");

			if (!(view.RealParent is Application))
			{
				Application app = new DefaultApplication();
				app.MainPage = view;
			}

			var platform = new Platform(context, true);
			platform.SetPage(view);

			var vg = platform.GetViewGroup();

			return new EmbeddedSupportFragment(vg, platform);
		}

		class DefaultApplication : Application
		{
		}

		class EmbeddedFragment : Fragment
		{
			readonly ViewGroup _content;
			readonly Platform _platform;
			bool _disposed;

			// ReSharper disable once UnusedMember.Local (Android uses this on configuration change
			public EmbeddedFragment()
			{
			}

			public EmbeddedFragment(ViewGroup content, Platform platform)
			{
				_content = content;
				_platform = platform;
			}

			public override global::Android.Views.View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
			{
				return _content;
			}

			protected override void Dispose(bool disposing)
			{
				if (_disposed)
				{
					return;
				}

				_disposed = true;

				if (disposing)
				{
					(_platform as IDisposable)?.Dispose();
				}

				base.Dispose(disposing);
			}
		}

		class EmbeddedSupportFragment : global::Android.Support.V4.App.Fragment
		{
			readonly ViewGroup _content;
			readonly Platform _platform;
			bool _disposed;

			// ReSharper disable once UnusedMember.Local (Android uses this on configuration change
			public EmbeddedSupportFragment()
			{
			}

			public EmbeddedSupportFragment(ViewGroup content, Platform platform)
			{
				_content = content;
				_platform = platform;
			}

			public override global::Android.Views.View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
			{
				return _content;
			}

			protected override void Dispose(bool disposing)
			{
				if (_disposed)
				{
					return;
				}

				_disposed = true;

				if (disposing)
				{
					(_platform as IDisposable)?.Dispose();
				}

				base.Dispose(disposing);
			}
		}
	}
}
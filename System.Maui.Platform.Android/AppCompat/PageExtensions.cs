using System.ComponentModel;
using Android.Content;
using Android.OS;
using Android.Views;
#if __ANDROID_29__
using Fragment = AndroidX.Fragment.App.Fragment;
#else
using Fragment = global::Android.Support.V4.App.Fragment;
#endif

namespace System.Maui.Platform.Android
{
	public static class PageExtensions
	{
#pragma warning disable 618
		[Obsolete("ContentPage.CreateFragment() is obsolete as of version 3.2. Please use ContentPage.CreateSupportFragment() instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static Fragment CreateFragment(this ContentPage view, Context context)
		{
			if (!System.Maui.Maui.IsInitialized)
				throw new InvalidOperationException("call System.Maui.Maui.Init() before this");

			if (!(view.RealParent is Application))
			{
				Application app = new DefaultApplication();
				app.MainPage = view;
			}

			var platform = new AppCompat.Platform(context);
			platform.SetPage(view);

			var vg = platform.GetViewGroup();

			return new EmbeddedFragment(vg, platform);
		}

		class EmbeddedFragment : Fragment
		{
			readonly ViewGroup _content;
			readonly AppCompat.Platform _platform;
			bool _disposed;

			// ReSharper disable once UnusedMember.Local (Android uses this on configuration change
			public EmbeddedFragment()
			{
			}

			public EmbeddedFragment(ViewGroup content, AppCompat.Platform platform)
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
#pragma warning restore 618

		public static Fragment CreateSupportFragment(this ContentPage view, Context context)
		{
			if (!System.Maui.Maui.IsInitialized)
				throw new InvalidOperationException("call System.Maui.Maui.Init() before this");

			if (!(view.RealParent is Application))
			{
				Application app = new DefaultApplication();
				app.MainPage = view;
			}

			var platform = new AppCompat.Platform(context);
			platform.SetPage(view);

			var vg = platform.GetViewGroup();

			return new EmbeddedSupportFragment(vg, platform);
		}

		class DefaultApplication : Application
		{
		}

		class EmbeddedSupportFragment : Fragment
		{
			readonly ViewGroup _content;
			readonly AppCompat.Platform _platform;
			bool _disposed;

			// ReSharper disable once UnusedMember.Local (Android uses this on configuration change
			public EmbeddedSupportFragment()
			{
			}

			public EmbeddedSupportFragment(ViewGroup content, AppCompat.Platform platform)
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
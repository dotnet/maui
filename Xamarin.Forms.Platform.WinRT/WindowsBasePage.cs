using System;
using System.ComponentModel;
using Windows.ApplicationModel;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	public abstract class WindowsBasePage : Windows.UI.Xaml.Controls.Page
	{
		public WindowsBasePage()
		{
			if (!DesignMode.DesignModeEnabled)
			{
				Windows.UI.Xaml.Application.Current.Suspending += OnApplicationSuspending;
				Windows.UI.Xaml.Application.Current.Resuming += OnApplicationResuming;
			}
		}

		protected Platform Platform { get; private set; }

		protected abstract Platform CreatePlatform();

		protected void LoadApplication(Application application)
		{
			if (application == null)
				throw new ArgumentNullException("application");

			Application.Current = application;
			Platform = CreatePlatform();
			Platform.SetPage(Application.Current.MainPage);
			application.PropertyChanged += OnApplicationPropertyChanged;

			Application.Current.SendStart();
		}

		void OnApplicationPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "MainPage")
				Platform.SetPage(Application.Current.MainPage);
		}

		void OnApplicationResuming(object sender, object e)
		{
			Application.Current.SendResume();
		}

		async void OnApplicationSuspending(object sender, SuspendingEventArgs e)
		{
			SuspendingDeferral deferral = e.SuspendingOperation.GetDeferral();
			await Application.Current.SendSleepAsync();
			deferral.Complete();
		}
	}
}
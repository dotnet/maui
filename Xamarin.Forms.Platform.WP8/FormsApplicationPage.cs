using System.ComponentModel;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Xamarin.Forms.Platform.WinPhone
{
	public class FormsApplicationPage : PhoneApplicationPage
	{
		Application _application;
		Platform _platform;

		protected FormsApplicationPage()
		{
			PhoneApplicationService.Current.Launching += OnLaunching;
			PhoneApplicationService.Current.Activated += OnActivated;
			PhoneApplicationService.Current.Deactivated += OnDeactivated;
			PhoneApplicationService.Current.Closing += OnClosing;

			MessagingCenter.Send(this, Forms.WP8DeviceInfo.BWPorientationChangedName, Orientation.ToDeviceOrientation());
			OrientationChanged += OnOrientationChanged;
			//DeserializePropertyStore ();
		}

		protected void LoadApplication(Application application)
		{
			Application.SetCurrentApplication(application);
			application.PropertyChanged += ApplicationOnPropertyChanged;
			_application = application;

			// Hack around the fact that OnLaunching will haev already happened by this point, sad but needed.
			application.SendStart();

			SetMainPage();
		}

		void ApplicationOnPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			if (args.PropertyName == "MainPage")
				SetMainPage();
		}

		void OnActivated(object sender, ActivatedEventArgs e)
		{
			// TODO : figure out consistency of get this to fire
			// Check whether tombstoned (terminated, but OS retains information about navigation state and state dictionarys) or dormant
			_application.SendResume();
		}

		// when app gets tombstoned, user press back past first page
		async void OnClosing(object sender, ClosingEventArgs e)
		{
			// populate isolated storage.
			//SerializePropertyStore ();
			await _application.SendSleepAsync();
		}

		async void OnDeactivated(object sender, DeactivatedEventArgs e)
		{
			// populate state dictionaries, properties
			//SerializePropertyStore ();
			await _application.SendSleepAsync();
		}

		void OnLaunching(object sender, LaunchingEventArgs e)
		{
			// TODO : not currently firing, is fired before MainPage ctor is called
			_application.SendStart();
		}

		void OnOrientationChanged(object sender, OrientationChangedEventArgs e)
		{
			MessagingCenter.Send(this, Forms.WP8DeviceInfo.BWPorientationChangedName, e.Orientation.ToDeviceOrientation());
		}

		void SetMainPage()
		{
			if (_platform == null)
				_platform = new Platform(this);

			_platform.SetPage(_application.MainPage);

			if (!ReferenceEquals(Content, _platform))
				Content = _platform.GetCanvas();
		}
	}
}
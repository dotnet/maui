using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using ElmSharp;
using Tizen.Applications;
using Xamarin.Forms.Internals;
using ELayout = ElmSharp.Layout;
using DeviceOrientation = Xamarin.Forms.Internals.DeviceOrientation;

namespace Xamarin.Forms.Platform.Tizen
{

	public class FormsApplication : CoreUIApplication
	{
		ITizenPlatform _platform;
		Application _application;
		bool _isInitialStart;
		Window _window;

		protected FormsApplication()
		{
			_isInitialStart = true;
		}

		/// <summary>
		/// Gets the main window or <c>null</c> if it's not set.
		/// </summary>
		/// <value>The main window or <c>null</c>.</value>
		public Window MainWindow
		{
			get
			{
				return _window;
			}
			protected set
			{
				_window = value;
				InitializeWindow();
			}
		}

		public ELayout BaseLayout
		{
			get; protected set;
		}

		protected override void OnPreCreate()
		{
			base.OnPreCreate();
			Application.ClearCurrent();
			MainWindow = new Window("FormsWindow");
		}

		protected override void OnTerminate()
		{
			base.OnTerminate();
			if (_platform != null)
			{
				_platform.Dispose();
			}
		}

		protected override void OnAppControlReceived(AppControlReceivedEventArgs e)
		{
			base.OnAppControlReceived(e);

			if (!_isInitialStart && _application != null)
			{
				_application.SendResume();
			}
			_isInitialStart = false;
		}

		protected override void OnPause()
		{
			base.OnPause();
			if (_application != null)
			{
				_application.SendSleepAsync();
			}
		}

		protected override void OnResume()
		{
			base.OnResume();
			if (_application != null)
			{
				_application.SendResume();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static Func<Task> RequestingUserConsentFunc { get; set; } = null;

		public async void LoadApplication(Application application)
		{
			if (RequestingUserConsentFunc != null)
			{
				await RequestingUserConsentFunc();
			}

			if (null == MainWindow)
			{
				throw new NullReferenceException("MainWindow is not prepared. This method should be called in OnCreated().");
			}

			if (null == application)
			{
				throw new ArgumentNullException("application");
			}
			_application = application;
			Application.Current = application;
			application.SendStart();
			application.PropertyChanged += new PropertyChangedEventHandler(this.AppOnPropertyChanged);
			SetPage(_application.MainPage);
		}

		void AppOnPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			if ("MainPage" == args.PropertyName)
			{
				SetPage(_application.MainPage);
			}
		}

		void SetPage(Page page)
		{
			if (!Forms.IsInitialized)
			{
				throw new InvalidOperationException("Call Forms.Init (UIApplication) before this");
			}

			if (_platform != null)
			{
				_platform.SetPage(page);
				return;
			}

			_platform = Platform.CreatePlatform(BaseLayout);
			_platform.HasAlpha = MainWindow.Alpha;
			BaseLayout.SetContent(_platform.GetRootNativeView());

			_platform.RootNativeViewChanged += (s, e) => BaseLayout.SetContent(e.RootNativeView);

			if (_application != null)
			{
				_application.Platform = _platform;
			}
			_platform.SetPage(page);
		}

		void InitializeWindow()
		{
			Debug.Assert(MainWindow != null, "Window cannot be null");

			MainWindow.Active();
			MainWindow.Show();

			var conformant = new Conformant(MainWindow);
			conformant.Show();

			var layout = new ELayout(conformant);
			layout.SetTheme("layout", "application", "default");
			layout.Show();

			BaseLayout = layout;
			conformant.SetContent(BaseLayout);

			MainWindow.AvailableRotations = DisplayRotation.Degree_0 | DisplayRotation.Degree_90 | DisplayRotation.Degree_180 | DisplayRotation.Degree_270;

			MainWindow.Deleted += (s, e) =>
			{
				Exit();
			};

			Device.Info.CurrentOrientation = MainWindow.GetDeviceOrientation();

			MainWindow.RotationChanged += (sender, e) =>
			{
				Device.Info.CurrentOrientation = MainWindow.GetDeviceOrientation();
			};

			MainWindow.BackButtonPressed += (sender, e) =>
			{
				if (_platform != null)
				{
					if (!_platform.SendBackButtonPressed())
					{
						Exit();
					}
				}
			};
		}

		public void Run()
		{
			Run(System.Environment.GetCommandLineArgs());
		}

		/// <summary>
		/// Exits the application's main loop, which initiates the process of its termination
		/// </summary>
		public override void Exit()
		{
			if (_platform == null)
			{
				Log.Warn("Exit was already called or FormsApplication is not initialized yet.");
				return;
			}
			try
			{
				_platform.Dispose();
				_platform = null;
			}
			catch (Exception e)
			{
				Log.Error("Exception thrown from Dispose: {0}", e.Message);
			}

			base.Exit();
		}
	}
	static class WindowExtension
	{
		public static DeviceOrientation GetDeviceOrientation(this Window window)
		{
			DeviceOrientation orientation = DeviceOrientation.Other;
			var isPortraitDevice = Forms.NaturalOrientation.IsPortrait();
			switch (window.Rotation)
			{
				case 0:
				case 180:
					orientation = isPortraitDevice ? DeviceOrientation.Portrait : DeviceOrientation.Landscape;
					break;

				case 90:
				case 270:
					orientation = isPortraitDevice ? DeviceOrientation.Landscape : DeviceOrientation.Portrait;
					break;
			}
			return orientation;
		}
	}
}

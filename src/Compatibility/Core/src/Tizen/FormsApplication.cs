using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Tizen.Common;
using Tizen.NUI;
using NWindow = Tizen.NUI.Window;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{

	[Obsolete]
	public class FormsApplication : NUIApplication
	{
		ITizenPlatform _platform;
		Application _application;
		NWindow _window;

		protected FormsApplication()
		{
		}

		/// <summary>
		/// Gets the main window or <c>null</c> if it's not set.
		/// </summary>
		/// <value>The main window or <c>null</c>.</value>
		public NWindow MainWindow
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

		protected override void OnPreCreate()
		{
			base.OnPreCreate();
			Application.ClearCurrent();

			if (DotnetUtil.TizenAPIVersion < 5)
			{
				// We should set the env variable to support IsolatedStorageFile on tizen 4.0 or lower version.
				Environment.SetEnvironmentVariable("XDG_DATA_HOME", Current.DirectoryInfo.Data);
			}

			MainWindow = NWindow.Instance;
		}

		protected override void OnTerminate()
		{
			base.OnTerminate();
			_platform?.Dispose();
		}

		protected override void OnPause()
		{
			base.OnPause();
			_application?.SendSleep();
		}

		protected override void OnResume()
		{
			base.OnResume();
			_application?.SendResume();
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
				throw new InvalidOperationException("MainEWindow is not prepared. This method should be called in OnCreated().");
			}

			if (null == application)
			{
				throw new ArgumentNullException(nameof(application));
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
			_platform.SetPage(page);
		}

		void InitializeWindow()
		{
			MainWindow.Show();

			MainWindow.KeyEvent += (s, e) =>
			{
				if (e.Key.State == Key.StateType.Down && (e.Key.KeyPressedName == "XF86Back" || e.Key.KeyPressedName == "Escape"))
				{

					if (global::Tizen.UIExtensions.NUI.Popup.HasOpenedPopup)
					{
						global::Tizen.UIExtensions.NUI.Popup.CloseLast();
						return;
					}

					if (!(_platform?.SendBackButtonPressed() ?? false))
					{
						Exit();
					}
				}
			};

			MainWindow.AddAvailableOrientation(NWindow.WindowOrientation.Landscape);
			MainWindow.AddAvailableOrientation(NWindow.WindowOrientation.LandscapeInverse);
			MainWindow.AddAvailableOrientation(NWindow.WindowOrientation.Portrait);
			MainWindow.AddAvailableOrientation(NWindow.WindowOrientation.PortraitInverse);

			_platform = new DefaultPlatform();
			MainWindow.GetDefaultLayer().Add(_platform.GetRootNativeView());
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
}

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using ElmSharp;
using ElmSharp.Wearable;
using Microsoft.Maui.Controls.Compatibility.Platform.Tizen.Native;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Devices;
using Tizen.Applications;
using Tizen.Common;
using EDisplayRotation = ElmSharp.DisplayRotation;
using ELayout = ElmSharp.Layout;
using EWindow = ElmSharp.Window;
using Specific = Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Application;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{

	[Obsolete]
	public class FormsApplication : CoreUIApplication
	{
		ITizenPlatform _platform;
		Application _application;
		EWindow _window;
		bool _useBezelInteration;

		protected FormsApplication()
		{
		}

		/// <summary>
		/// Gets the main window or <c>null</c> if it's not set.
		/// </summary>
		/// <value>The main window or <c>null</c>.</value>
		public EWindow MainWindow
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

		public CircleSurface BaseCircleSurface
		{
			get; protected set;
		}

		public bool UseBezelInteration => _useBezelInteration;

		protected override void OnPreCreate()
		{
			base.OnPreCreate();
			Application.ClearCurrent();

			if (DotnetUtil.TizenAPIVersion < 5)
			{
				// We should set the env variable to support IsolatedStorageFile on tizen 4.0 or lower version.
				Environment.SetEnvironmentVariable("XDG_DATA_HOME", Current.DirectoryInfo.Data);
			}

			var type = typeof(EWindow);
			// Use reflection to avoid breaking compatibility. ElmSharp.Window.CreateWindow() is has been added since API6.
			var methodInfo = type.GetMethod("CreateWindow", BindingFlags.NonPublic | BindingFlags.Static);
			EWindow window = null;
			if (methodInfo != null)
			{
				window = (EWindow)methodInfo.Invoke(null, new object[] { "FormsWindow" });
				BaseLayout = (ELayout)window.GetType().GetProperty("BaseLayout")?.GetValue(window);
				BaseCircleSurface = (CircleSurface)window.GetType().GetProperty("BaseCircleSurface")?.GetValue(window);
				Forms.CircleSurface = BaseCircleSurface;
			}
			else // in case of Xamarin Preload
			{
				window = PreloadedWindow.GetInstance() ?? new EWindow("FormsWindow");
				if (window is PreloadedWindow precreated)
				{
					BaseLayout = precreated.BaseLayout;
				}
			}
			MainWindow = window;
		}

		protected override void OnTerminate()
		{
			base.OnTerminate();
			if (_platform != null)
			{
				_platform.Dispose();
			}
		}

		protected override void OnPause()
		{
			base.OnPause();
			if (_application != null)
			{
				_application.SendSleep();
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
			if (DeviceInfo.Idiom == DeviceIdiom.Watch)
			{
				_useBezelInteration = Specific.GetUseBezelInteraction(_application);
				UpdateOverlayContent();
			}
		}

		void AppOnPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			if ("MainPage" == args.PropertyName)
			{
				SetPage(_application.MainPage);
			}
			else if (DeviceInfo.Idiom == DeviceIdiom.Watch)
			{
				if (Specific.UseBezelInteractionProperty.PropertyName == args.PropertyName)
				{
					_useBezelInteration = Specific.GetUseBezelInteraction(_application);
				}
				else if (Specific.OverlayContentProperty.PropertyName == args.PropertyName)
				{
					UpdateOverlayContent();
				}
			}
		}

		void UpdateOverlayContent()
		{
			EvasObject nativeView = null;
			var content = Specific.GetOverlayContent(_application);
			if (content != null)
			{
				var renderer = Platform.GetOrCreateRenderer(content);
				(renderer as ILayoutRenderer)?.RegisterOnLayoutUpdated();
				nativeView = renderer?.NativeView;
			}
			Forms.BaseLayout.SetOverlayPart(nativeView);
		}

		void SetPage(Page page)
		{
			if (!Forms.IsInitialized)
			{
				throw new InvalidOperationException("Call Forms.Init (UIApplication) before this");
			}

			_platform.HasAlpha = MainWindow.Alpha;
			_platform.SetPage(page);
		}

		void InitializeWindow()
		{
			Debug.Assert(MainWindow != null, "EWindow cannot be null");

			MainWindow.Active();
			MainWindow.Show();

			// in case of no use of preloaded window
			if (BaseLayout == null)
			{
				var conformant = new Conformant(MainWindow);
				conformant.Show();

				var layout = new ApplicationLayout(conformant);

				layout.Show();

				BaseLayout = layout;

				if (DeviceInfo.Idiom == DeviceIdiom.Watch)
				{
					BaseCircleSurface = new CircleSurface(conformant);
					Forms.CircleSurface = BaseCircleSurface;
				}
				conformant.SetContent(BaseLayout);
			}

			MainWindow.AvailableRotations = EDisplayRotation.Degree_0 | EDisplayRotation.Degree_90 | EDisplayRotation.Degree_180 | EDisplayRotation.Degree_270;

			MainWindow.Deleted += (s, e) =>
			{
				Exit();
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

			_platform = Platform.CreatePlatform(BaseLayout);
			BaseLayout.SetContent(_platform.GetRootNativeView());
			_platform.RootNativeViewChanged += (s, e) => BaseLayout.SetContent(e.RootNativeView);
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

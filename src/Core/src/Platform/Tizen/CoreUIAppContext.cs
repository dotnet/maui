using System;
using System.Reflection;
using Tizen.Common;
using Tizen.Applications;
using ElmSharp;
using ElmSharp.Wearable;
using Tizen.UIExtensions.ElmSharp;
using ELayout = ElmSharp.Layout;

namespace Microsoft.Maui
{
	public class CoreUIAppContext
	{
		DisplayResolutionUnit _displayResolutionUnit = DisplayResolutionUnit.DP;
		double _viewPortWidth = -1;

		static CoreUIAppContext? _instance = null;

		Func<bool>? _handleBackButtonPressed;

		public static bool IsInitialized { get; private set; }

		public static CoreUIAppContext GetInstance(CoreApplication application, Window? window = null)
		{
			if (IsInitialized)
				return _instance!;


			_instance = (window == null) ? new CoreUIAppContext(application) : new CoreUIAppContext(application, window);
			return _instance;
		}

		public CoreApplication CurrentApplication { get; private set; }

		public string ResourceDir => CurrentApplication.DirectoryInfo.Resource;

		public EvasObject NativeParent => BaseLayout;

		public Window MainWindow { get; set; }

		public ELayout BaseLayout { get; set; }

		public ModalStack ModalStack { get; private set; }

		public CircleSurface? BaseCircleSurface { get; set; }

		public DeviceType DeviceType => DeviceInfo.GetDeviceType();

		public DisplayResolutionUnit DisplayResolutionUnit
		{
			get => _displayResolutionUnit;
			set
			{
				_displayResolutionUnit = value;
				DeviceInfo.DisplayResolutionUnit = _displayResolutionUnit;
			}
		}

		public double ViewportWidth
		{
			get => _viewPortWidth;
			set
			{
				_viewPortWidth = value;
				// TODO. DeviceInfo.ViewportWidth is readonly, fix it
				//ViewportWidth = _viewPortWidth;
			}
		}

		protected CoreUIAppContext(CoreApplication application) : this(application, CreateDefaultWindow())
		{
		}

		protected CoreUIAppContext(CoreApplication application, Window window)
		{
			_ = application ?? throw new ArgumentNullException(nameof(application));
			_ = window ?? throw new ArgumentNullException(nameof(window));

			if (DisplayResolutionUnit == DisplayResolutionUnit.VP && ViewportWidth < 0)
				throw new InvalidOperationException($"ViewportWidth should be set in case of DisplayResolutionUnit == VP");

			Elementary.Initialize();
			Elementary.ThemeOverlay();
			CurrentApplication = application;
			MainWindow = window;
			InitializeMainWindow();

			_ = BaseLayout ?? throw new ArgumentNullException(nameof(BaseLayout));

			if (DotnetUtil.TizenAPIVersion < 5)
			{
				// We should set the env variable to support IsolatedStorageFile on tizen 4.0 or lower version.
				Environment.SetEnvironmentVariable("XDG_DATA_HOME", CurrentApplication.DirectoryInfo.Data);
			}

			ModalStack = new ModalStack(NativeParent)
			{
				AlignmentX = -1,
				AlignmentY = -1,
				WeightX = 1,
				WeightY= 1,
			};
			ModalStack.Show();
			BaseLayout.SetContent(ModalStack);

			IsInitialized = true;
		}

		public void SetContent(EvasObject content)
		{
			content.SetAlignment(-1, -1);
			content.SetWeight(1, 1);
			content.Show();
			ModalStack.Push(content);
		}

		public void SetBackButtonPressedHandler(Func<bool> handler)
		{
			_handleBackButtonPressed = handler;
		}

		static Window CreateDefaultWindow()
		{
			return GetPreloadedWindow() ?? new Window("XamarinWindow");
		}

		static Window? GetPreloadedWindow()
		{
			var type = typeof(Window);
			// Use reflection to avoid breaking compatibility. ElmSharp.Window.CreateWindow() is has been added since API6.
			var methodInfo = type.GetMethod("CreateWindow", BindingFlags.NonPublic | BindingFlags.Static);

			return (Window?)methodInfo?.Invoke(null, new object[] { "FormsWindow" });
		}

		void InitializeMainWindow()
		{
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8601 // Possible null reference assignment.
			BaseLayout = (ELayout)MainWindow.GetType().GetProperty("BaseLayout")?.GetValue(MainWindow);
			BaseCircleSurface = (CircleSurface)MainWindow.GetType().GetProperty("BaseCircleSurface")?.GetValue(MainWindow);
#pragma warning restore CS8601 // Possible null reference assignment.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

			if (BaseLayout == null)
			{
				var conformant = new Conformant(MainWindow);
				conformant.Show();

				var layout = new ApplicationLayout(conformant);
				layout.Show();

				BaseLayout = layout;

				if (DeviceType == DeviceType.Watch)
				{
					BaseCircleSurface = new CircleSurface(conformant);
				}
				conformant.SetContent(BaseLayout);

				if (DeviceType == DeviceType.Watch)
				{
					BaseCircleSurface = new CircleSurface(conformant);
				}
			}

			MainWindow.Active();
			MainWindow.Show();
			MainWindow.AvailableRotations = DisplayRotation.Degree_0 | DisplayRotation.Degree_90 | DisplayRotation.Degree_180 | DisplayRotation.Degree_270;
			
			MainWindow.Deleted += (s, e) => CurrentApplication.Exit();

			MainWindow.RotationChanged += (sender, e) =>
			{
				// TODO : should update later
			};

			MainWindow.BackButtonPressed += OnBackButtonPressed;



		}

		void OnBackButtonPressed(object sender, EventArgs e)
		{
			if (!(_handleBackButtonPressed?.Invoke() ?? false))
			{
				CurrentApplication.Exit();
			}
		}
	}
}

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using ElmSharp;
using Tizen.Applications;
using Xamarin.Forms.Internals;
using EButton = ElmSharp.Button;
using EColor = ElmSharp.Color;
using ELayout = ElmSharp.Layout;
using EProgressBar = ElmSharp.ProgressBar;

namespace Xamarin.Forms.Platform.Tizen
{

	public class FormsApplication : CoreUIApplication
	{
		Platform _platform;
		Application _application;
		bool _isInitialStart;
		int _pageBusyCount;
		Native.Dialog _pageBusyDialog;
		Window _window;

		protected FormsApplication()
		{
			_isInitialStart = true;
			_pageBusyCount = 0;
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
			MessagingCenter.Unsubscribe<Page, AlertArguments>(this, "Xamarin.SendAlert");
			MessagingCenter.Unsubscribe<Page, bool>(this, "Xamarin.BusySet");
			MessagingCenter.Unsubscribe<Page, ActionSheetArguments>(this, "Xamarin.ShowActionSheet");
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

		static void ActionSheetSignalNameHandler(Page sender, ActionSheetArguments arguments)
		{
			Native.Dialog alert = new Native.Dialog(Forms.Context.MainWindow);

			alert.Title = arguments.Title;
			Box box = new Box(alert);

			if (null != arguments.Destruction)
			{
				Native.Button destruction = new Native.Button(alert)
				{
					Text = arguments.Destruction,
					TextColor = EColor.Red,
					AlignmentX = -1
				};
				destruction.Clicked += (s, evt) =>
				{
					arguments.SetResult(arguments.Destruction);
					alert.Dismiss();
				};
				destruction.Show();
				box.PackEnd(destruction);
			}

			foreach (string buttonName in arguments.Buttons)
			{
				Native.Button button = new Native.Button(alert)
				{
					Text = buttonName,
					AlignmentX = -1
				};
				button.Clicked += (s, evt) =>
				{
					arguments.SetResult(buttonName);
					alert.Dismiss();
				};
				button.Show();
				box.PackEnd(button);
			}

			box.Show();
			alert.Content = box;

			if (null != arguments.Cancel)
			{
				EButton cancel = new EButton(Forms.Context.MainWindow) { Text = arguments.Cancel };
				alert.NegativeButton = cancel;
				cancel.Clicked += (s, evt) =>
				{
					alert.Dismiss();
				};
			}

			alert.BackButtonPressed += (s, evt) =>
			{
				alert.Dismiss();
			};

			alert.Show();
		}

		static void AlertSignalNameHandler(Page sender, AlertArguments arguments)
		{
			Native.Dialog alert = new Native.Dialog(Forms.Context.MainWindow);
			alert.Title = arguments.Title;
			var message = arguments.Message.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace(Environment.NewLine, "<br>");
			alert.Text = message;

			EButton cancel = new EButton(alert) { Text = arguments.Cancel };
			alert.NegativeButton = cancel;
			cancel.Clicked += (s, evt) =>
			{
				arguments.SetResult(false);
				alert.Dismiss();
			};

			if (arguments.Accept != null)
			{
				EButton ok = new EButton(alert) { Text = arguments.Accept };
				alert.NeutralButton = ok;
				ok.Clicked += (s, evt) =>
				{
					arguments.SetResult(true);
					alert.Dismiss();
				};
			}

			alert.BackButtonPressed += (s, evt) =>
			{
				arguments.SetResult(false);
				alert.Dismiss();
			};

			alert.Show();
		}

		void AppOnPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			if ("MainPage" == args.PropertyName)
			{
				SetPage(_application.MainPage);
			}
		}

		void ShowActivityIndicatorDialog(bool enabled)
		{
			if (null == _pageBusyDialog)
			{
				_pageBusyDialog = new Native.Dialog(Forms.Context.MainWindow)
				{
					Orientation = PopupOrientation.Top,
				};

				var activity = new EProgressBar(_pageBusyDialog)
				{
					Style = "process_large",
					IsPulseMode = true,
				};
				activity.PlayPulse();
				activity.Show();

				_pageBusyDialog.Content = activity;

			}
			_pageBusyCount = Math.Max(0, enabled ? _pageBusyCount + 1 : _pageBusyCount - 1);
			if (_pageBusyCount > 0)
			{
				_pageBusyDialog.Show();
			}
			else
			{
				_pageBusyDialog.Dismiss();
				_pageBusyDialog = null;
			}
		}

		void BusySetSignalNameHandler(Page sender, bool enabled)
		{
			ShowActivityIndicatorDialog(enabled);
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

			MessagingCenter.Subscribe<Page, bool>(this, Page.BusySetSignalName, BusySetSignalNameHandler);
			MessagingCenter.Subscribe<Page, AlertArguments>(this, Page.AlertSignalName, AlertSignalNameHandler);
			MessagingCenter.Subscribe<Page, ActionSheetArguments>(this, Page.ActionSheetSignalName, ActionSheetSignalNameHandler);

			_platform = new Platform(this);
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

			// Create the base (default) layout for the application
			var layout = new ELayout(conformant);
			layout.SetTheme("layout", "application", "default");
			layout.Show();

			conformant.SetContent(layout);
			BaseLayout = layout;
			MainWindow.AvailableRotations = DisplayRotation.Degree_0 | DisplayRotation.Degree_90 | DisplayRotation.Degree_180 | DisplayRotation.Degree_270;

			MainWindow.Deleted += (s, e) =>
			{
				Exit();
			};
			MainWindow.RotationChanged += (sender, e) =>
			{
				switch (MainWindow.Rotation)
				{
					case 0:
						Device.Info.CurrentOrientation = Internals.DeviceOrientation.PortraitUp;
						break;

					case 90:
						Device.Info.CurrentOrientation = Internals.DeviceOrientation.LandscapeLeft;
						break;

					case 180:
						Device.Info.CurrentOrientation = Internals.DeviceOrientation.PortraitDown;
						break;

					case 270:
						Device.Info.CurrentOrientation = Internals.DeviceOrientation.LandscapeRight;
						break;
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
			// before everything is closed, inform the MainPage that it is disappearing
			try
			{
				(_platform?.Page as IPageController)?.SendDisappearing();
				_platform = null;
			}
			catch (Exception e)
			{
				Log.Error("Exception thrown from SendDisappearing: {0}", e.Message);
			}

			base.Exit();
		}
	}
}

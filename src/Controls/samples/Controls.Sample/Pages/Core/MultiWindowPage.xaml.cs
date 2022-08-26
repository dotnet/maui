using System;
using System.Diagnostics;
using Maui.Controls.Sample.Pages.Base;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;

namespace Maui.Controls.Sample.Pages
{
	public partial class MultiWindowPage : BasePage
	{
		static int windowCounter = 0;

		int currentWindow = 0;

		public MultiWindowPage()
		{
			windowCounter++;
			currentWindow = windowCounter;

			InitializeComponent();

			BindingContext = this;

			Loaded += OnLoaded;
		}

		public int WindowCount => windowCounter;

		void OnNewWindowClicked(object sender, EventArgs e)
		{
			Application.Current.OpenWindow(new Window(new MultiWindowPage()));
		}

		void OnCloseWindowClicked(object sender, EventArgs e)
		{
			Application.Current.CloseWindow(Window);
		}

		void OnSetMaxSize(object sender, EventArgs e)
		{
			Window.MaximumWidth = 800;
			Window.MaximumHeight = 600;
		}

		void OnSetMinSize(object sender, EventArgs e)
		{
			Window.MinimumWidth = 640;
			Window.MinimumHeight = 480;
		}

		void OnSetFreeSize(object sender, EventArgs e)
		{
			Window.MaximumWidth = double.PositiveInfinity;
			Window.MaximumHeight = double.PositiveInfinity;

			Window.MinimumWidth = -1d;
			Window.MinimumHeight = -1d;
		}
		void OnSetCustomSize(object sender, EventArgs e)
		{
			Window.Width = 700;
			Window.Height = 500;
		}

		void OnCenterWindow(object sender, EventArgs e)
		{
			var disp = DeviceDisplay.MainDisplayInfo;

			Window.X = (disp.Width / disp.Density - Window.Width) / 2;
			Window.Y = (disp.Height / disp.Density - Window.Height) / 2;
		}

		void OnLoaded(object sender, EventArgs e)
		{
			var window = Window;

			window.SizeChanged += OnWindowSizeChanged;

			Unloaded += OnUnloaded;

			void OnUnloaded(object sender, EventArgs e)
			{
				Unloaded -= OnUnloaded;
				window.SizeChanged -= OnWindowSizeChanged;
			}
		}

		void OnWindowSizeChanged(object sender, EventArgs e)
		{
			Debug.WriteLine($"Window Size Changed ({currentWindow}): {Window.Frame.Size}");
		}
	}
}
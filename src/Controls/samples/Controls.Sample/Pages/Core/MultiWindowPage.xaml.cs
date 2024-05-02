using System;
using Maui.Controls.Sample.Pages.Base;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;

namespace Maui.Controls.Sample.Pages
{
	public partial class MultiWindowPage : BasePage
	{
		public MultiWindowPage()
		{
			InitializeComponent();

			BindingContext = this;
		}

		void OnNewWindowClicked(object sender, EventArgs e)
		{
			Application.Current!.OpenWindow(new Window(new MultiWindowPage()));
		}

		void OnCloseWindowClicked(object sender, EventArgs e)
		{
			Application.Current!.CloseWindow(Window);
		}

		async void OnOpenDialogClicked(object sender, EventArgs e)
		{
			await DisplayAlert("Information", "The dialog should open by Window.", "Ok");
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
	}
}
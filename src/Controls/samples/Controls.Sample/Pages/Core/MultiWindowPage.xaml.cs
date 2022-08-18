using System;
using Maui.Controls.Sample.Pages.Base;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class MultiWindowPage : BasePage
	{
		static int windowCounter = 1;

		public MultiWindowPage()
		{
			windowCounter++;

			InitializeComponent();

			BindingContext = this;
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
	}
}
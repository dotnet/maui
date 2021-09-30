using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class TemplatePage
	{
		public TemplatePage()
		{
			InitializeComponent();
		}

		private void OnCounterClicked(object sender, EventArgs e)
		{
#if WINDOWS
			var window = MauiWinUIApplication.Current.CreateNativeWindow(newWindow: new Window(new ContentPage()));
			window.Activate();
#endif
		}
	}
}
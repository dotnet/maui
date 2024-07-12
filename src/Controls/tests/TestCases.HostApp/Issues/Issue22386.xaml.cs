using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.None, 22386, "[WinUI] Application.Current.CloseWindow Crash When AppWindow.Hide is called in .Net Maui 9.0 Prev 3", PlatformAffected.UWP)]
	public partial class Issue22386 : ContentPage
	{
		public Issue22386()
		{
			InitializeComponent();
		}
		void OnUpdateWindow(object sender, EventArgs e)
		{
			var issue22386Window2 = new Issue22386Window2();

			Application.Current.OpenWindow(new Window(issue22386Window2));

			var mauiWindow = (sender as View).GetVisualElementWindow() as Window;

#if WINDOWS
            var window = mauiWindow.Handler.PlatformView as Microsoft.UI.Xaml.Window;
            var handle = WinRT.Interop.WindowNative.GetWindowHandle(window);
            var id = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(handle);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(id);

            appWindow.Hide();

#endif
			Application.Current.CloseWindow(mauiWindow);
		}
	}

	public class Issue22386Window2 : ContentPage
	{
		public Issue22386Window2()
		{
			Content = new Label { AutomationId = "Success", Text = "Success" };
		}
	}
}
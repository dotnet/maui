//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System;
using Microsoft.Maui.Controls.ControlGallery.Issues;
using OpenQA.Selenium.Appium.Windows;
using Xamarin.UITest;

namespace Microsoft.Maui.Controls.Compatibility.UITests
{
	internal class WindowsTestServer : ITestServer
	{
		readonly WindowsDriver<WindowsElement> _session;
		readonly WinDriverApp _winDriverApp;

		public WindowsTestServer(WindowsDriver<WindowsElement> session, WinDriverApp winDriverApp)
		{
			_session = session;
			_winDriverApp = winDriverApp;
		}

		public string Post(string endpoint, object arguments = null)
		{
			throw new NotImplementedException();
		}

		public string Put(string endpoint, byte[] data)
		{
			throw new NotImplementedException();
		}

		public string Get(string endpoint)
		{
			if (endpoint == "version")
			{
				try
				{
					return _session.CurrentWindowHandle;
				}
				catch (OpenQA.Selenium.WebDriverException we)
				when (we.IsWindowClosedException())
				{
					_winDriverApp.RestartFromCrash();
				}
				catch (Exception exception)
				{
					WindowsTestBase.HandleAppClosed(exception);
					throw;
				}
			}

			return endpoint;
		}
	}
}
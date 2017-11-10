using System;
using OpenQA.Selenium.Appium.Windows;
using Xamarin.UITest;

namespace Xamarin.Forms.Core.UITests
{
	internal class WindowsTestServer : ITestServer
	{
		readonly WindowsDriver<WindowsElement> _session;

		public WindowsTestServer(WindowsDriver<WindowsElement> session)
		{
			_session = session;
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
using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui23347
{
	public Maui23347() => InitializeComponent();


	public class Test : IDisposable
	{
		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			Application.SetCurrentApplication(null);
			DispatcherProvider.SetCurrent(null);
			Application.Current = null;
		}

		[Theory]
		[Values]
		public void FontImageSourceIssue(XamlInflator inflator)
		{
			Application.Current.UserAppTheme = AppTheme.Light;
			var page = new Maui23347(inflator);
			Application.Current.MainPage = page;
		}
	}
}

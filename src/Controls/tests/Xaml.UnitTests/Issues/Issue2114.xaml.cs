using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Issue2114 : Application
{
	public Issue2114() => InitializeComponent();


	public class Tests : IDisposable
	{
		public Tests()
		{
			// Clear any existing application to ensure clean state
			Application.SetCurrentApplication(null);
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
			DispatcherProvider.SetCurrent(null);
			Application.SetCurrentApplication(null);
		}

		[Theory]
		[Values]
		public void StaticResourceOnApplication(XamlInflator inflator)
		{
			var app = new Issue2114(inflator);
			Application.SetCurrentApplication(app);

			Assert.True(Application.Current.Resources.ContainsKey("ButtonStyle"));
			Assert.True(Application.Current.Resources.ContainsKey("NavButtonBlueStyle"));
			Assert.True(Application.Current.Resources.ContainsKey("NavButtonGrayStyle"));
		}
	}
}
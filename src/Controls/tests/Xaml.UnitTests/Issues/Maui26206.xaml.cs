using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui26206 : ContentPage
{
	public Maui26206()
	{
		InitializeComponent();
	}


	[Collection("Issue")]
	public class Tests : IDisposable
	{
		public Tests()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
		}

		[Theory]
		[XamlInflatorData]
		internal void MultipleResourcesInRD(XamlInflator inflator)
		{
			var page = new Maui26206(inflator);
			Assert.Equal(25d, ((StackBase)page.Content).Spacing);
		}
	}
}

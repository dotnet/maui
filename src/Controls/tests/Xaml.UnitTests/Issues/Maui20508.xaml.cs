using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui20508
{
	public Maui20508() => InitializeComponent();

	[Collection("Issue")]
	public class Test : IDisposable
	{
		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose() => AppInfo.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		internal void ToolBarItemBinding(XamlInflator inflator)
		{
			var page = new Maui20508(inflator) { BindingContext = new { Icon = "boundIcon.png" } };
			Assert.Equal("boundIcon.png", ((FileImageSource)page.ToolbarItems[0].IconImageSource).File);
		}
	}
}

using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Unreported010
{
	public Unreported010() => InitializeComponent();

	public class Test : IDisposable
	{
		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[Theory]
		[Values]
		public void LocalDynamicResources(XamlInflator inflator)
		{
			var page = new Unreported010(inflator);
			Assert.Equal(Colors.Blue, page.button0.BackgroundColor);
			page.Resources["Foo"] = Colors.Red;
			Assert.Equal(Colors.Red, page.button0.BackgroundColor);
		}

		public void Dispose()
		{
			Application.Current = null;
			DispatcherProvider.SetCurrent(null);
		}
	}

}

using System;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class U001Page : ContentPage
{
	public U001Page()
	{
	}

}

public partial class Unreported001 : TabbedPage
{
	public Unreported001() => InitializeComponent();

	public class Tests : IDisposable
	{
		public Tests()
		{
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose() => DispatcherProvider.SetCurrent(null);

		[Theory]
		[Values]
		public void DoesNotThrow(XamlInflator inflator)
		{
			var p = new Unreported001(inflator);
			Assert.IsType<U001Page>(p.navpage.CurrentPage);
		}
	}
}
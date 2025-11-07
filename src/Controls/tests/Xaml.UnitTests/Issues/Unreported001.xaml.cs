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

		public void Dispose() { }
		// TODO: Convert to IDisposable or constructor - [MemberData(nameof(InitializeTest))] // TODO: Convert to IDisposable or constructor public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());

		[Theory]
		[Values]
		public void DoesNotThrow(XamlInflator inflator)
		{
			var p = new Unreported001(inflator);
			Assert.IsType<U001Page>(p.navpage.CurrentPage);
		}
	}
}
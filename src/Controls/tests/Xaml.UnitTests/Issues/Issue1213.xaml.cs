using System;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Issue1213 : TabbedPage
{
	public Issue1213() => InitializeComponent();


	public class Tests : IDisposable
	{

		public void Dispose() { }
		// TODO: Convert to IDisposable or constructor - [MemberData(nameof(InitializeTest))] // TODO: Convert to IDisposable or constructor public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());

		[Theory]
		[Values]
		public void MultiPageAsContentPropertyAttribute(XamlInflator inflator)
		{
			var page = new Issue1213(inflator);
			Assert.Equal(2, page.Children.Count);
		}
	}
}
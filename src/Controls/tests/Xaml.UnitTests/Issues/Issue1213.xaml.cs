using System;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Issue1213 : TabbedPage
{
	public Issue1213() => InitializeComponent();

	[Collection("Issue")]
	public class Tests : IDisposable
	{
		public Tests() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		public void Dispose() => DispatcherProvider.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		internal void MultiPageAsContentPropertyAttribute(XamlInflator inflator)
		{
			var page = new Issue1213(inflator);
			Assert.Equal(2, page.Children.Count);
		}
	}
}
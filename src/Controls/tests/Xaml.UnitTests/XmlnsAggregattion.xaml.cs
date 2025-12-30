using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

[assembly: XmlnsDefinition("http://schemas.microsoft.com/dotnet/maui/global", "http://schemas.microsoft.com/dotnet/2021/maui")]

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class XmlnsAggregattion : ContentPage
{
	public XmlnsAggregattion() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests : IDisposable
	{
		public Tests() => AppInfo.SetCurrent(new MockAppInfo());
		public void Dispose() => AppInfo.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		internal void XamlWithAggregatedXmlns(XamlInflator inflator)
		{
			var layout = new XmlnsAggregattion(inflator);
			Assert.Equal("Welcome to .NET MAUI!", layout.label.Text);
		}
	}
}